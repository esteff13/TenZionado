using System.Data;
using Microsoft.Data.SqlClient;
namespace CampusEvents;
public record EventView(int EventId,string Title,string Description,string Venue,DateTime StartsAt,int AvailableSeats);
public record AttendeeView(string FullName,string Email,DateTime RegisteredAt);
public record RegistrationRequest(int EventId,string FullName,string Email);
public sealed class EventRepository(string connectionString) : ISeatAvailability {
    public List<EventView> GetUpcoming() {
        using var connection=new SqlConnection(connectionString);
        using var command=new SqlCommand("""
            SELECT e.EventId,e.Title,e.Description,e.Venue,e.StartsAt,
                   e.Capacity-(SELECT COUNT(*) FROM dbo.Registrations r WHERE r.EventId=e.EventId)
            FROM dbo.Events e WHERE e.StartsAt>SYSUTCDATETIME() ORDER BY e.StartsAt,e.EventId;
            """,connection);
        connection.Open();using var reader=command.ExecuteReader();var result=new List<EventView>();
        while(reader.Read()) result.Add(new(reader.GetInt32(0),reader.GetString(1),reader.GetString(2),reader.GetString(3),
            DateTime.SpecifyKind(reader.GetDateTime(4),DateTimeKind.Utc),reader.GetInt32(5)));
        return result;
    }
    public bool HasAvailableSeat(int eventId) => GetUpcoming().Any(e=>e.EventId==eventId && e.AvailableSeats>0);
    public List<AttendeeView> GetAttendees(int eventId) {
        using var connection=new SqlConnection(connectionString);
        using var command=new SqlCommand("""
            SELECT u.FullName,u.Email,r.RegisteredAt FROM dbo.Registrations r
            JOIN dbo.Users u ON u.UserId=r.UserId WHERE r.EventId=@Id
            ORDER BY r.RegisteredAt,r.RegistrationId;
            """,connection);
        command.Parameters.Add("@Id",SqlDbType.Int).Value=eventId;
        connection.Open();using var reader=command.ExecuteReader();var result=new List<AttendeeView>();
        while(reader.Read()) result.Add(new(reader.GetString(0),reader.GetString(1),DateTime.SpecifyKind(reader.GetDateTime(2),DateTimeKind.Utc)));
        return result;
    }
    public int Register(RegistrationRequest request) {
        using var connection=new SqlConnection(connectionString);connection.Open();
        using var transaction=connection.BeginTransaction(IsolationLevel.Serializable);
        // Serializes registration writes in this small prototype, including the same email across events.
        // Also prevents two callers from consuming the last seat simultaneously.
        using(var lockCommand=new SqlCommand("""
            DECLARE @result int;
            EXEC @result=sys.sp_getapplock @Resource=N'CampusEvents.Register',
                @LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
            SELECT @result;
            """,connection,transaction)) {
            if(Convert.ToInt32(lockCommand.ExecuteScalar())<0) throw new InvalidOperationException("Registration is busy. Please retry.");
        }
        using(var seat=new SqlCommand("""
            SELECT Capacity-(SELECT COUNT(*) FROM dbo.Registrations WHERE EventId=@Id)
            FROM dbo.Events WITH(UPDLOCK,HOLDLOCK) WHERE EventId=@Id AND StartsAt>SYSUTCDATETIME();
            """,connection,transaction)) {
            seat.Parameters.Add("@Id",SqlDbType.Int).Value=request.EventId;
            var available=seat.ExecuteScalar();
            if(available is null or DBNull || Convert.ToInt32(available)<=0)
                throw new InvalidOperationException("This event is full, unavailable, or already started.");
        }
        using var user=new SqlCommand("""
            SELECT UserId FROM dbo.Users WHERE Email=@Email;
            """,connection,transaction);
        user.Parameters.Add("@Email",SqlDbType.NVarChar,254).Value=RegistrationValidator.NormalizeEmail(request.Email);
        var userId=user.ExecuteScalar();
        if(userId is null or DBNull) {
            using var insertUser=new SqlCommand("INSERT INTO dbo.Users(FullName,Email) OUTPUT INSERTED.UserId VALUES(@Name,@Email);",connection,transaction);
            insertUser.Parameters.Add("@Name",SqlDbType.NVarChar,100).Value=request.FullName.Trim();
            insertUser.Parameters.Add("@Email",SqlDbType.NVarChar,254).Value=RegistrationValidator.NormalizeEmail(request.Email);
            userId=insertUser.ExecuteScalar();
        }
        using var register=new SqlCommand("INSERT INTO dbo.Registrations(UserId,EventId) OUTPUT INSERTED.RegistrationId VALUES(@User,@Event);",connection,transaction);
        register.Parameters.Add("@User",SqlDbType.Int).Value=userId!;
        register.Parameters.Add("@Event",SqlDbType.Int).Value=request.EventId;
        var id=Convert.ToInt32(register.ExecuteScalar());transaction.Commit();return id;
    }
}
