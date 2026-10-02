using CampusEvents;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.FileProviders;
var builder=WebApplication.CreateBuilder(args);
var connectionString=builder.Configuration["ConnectionStrings:CampusEvents"]
    ?? throw new InvalidOperationException("Set ConnectionStrings__CampusEvents before starting.");
var adminKey=builder.Configuration["AdminKey"];
if(string.IsNullOrWhiteSpace(adminKey) || adminKey.Length<20)
    throw new InvalidOperationException("Set AdminKey to a random secret of at least 20 characters.");
builder.Services.AddSingleton(new EventRepository(connectionString));
builder.Services.AddSingleton<ISeatAvailability>(sp=>sp.GetRequiredService<EventRepository>());
builder.Services.AddSingleton<RegistrationValidator>();
builder.Services.AddSingleton(new RegistrationService(connectionString));
var app=builder.Build();
app.Use(async (ctx,next)=> {
    ctx.Response.Headers["X-Content-Type-Options"]="nosniff";
    ctx.Response.Headers["Content-Security-Policy"]="default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self'; frame-ancestors 'none'; base-uri 'self'";
    try { await next(ctx); }
    catch(Exception ex) {
        app.Logger.LogError(ex,"Request failed");
        ctx.Response.StatusCode=500;
        await ctx.Response.WriteAsJsonAsync(new {error="Unable to complete the request. Please retry."});
    }
});
var frontend=Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath,"../frontend"));
if(!Directory.Exists(frontend)) frontend=Path.Combine(AppContext.BaseDirectory,"wwwroot");
var files=new PhysicalFileProvider(frontend);
app.UseDefaultFiles(new DefaultFilesOptions {FileProvider=files});
app.UseStaticFiles(new StaticFileOptions {FileProvider=files});
app.MapGet("/api/events",(EventRepository db)=>Results.Ok(db.GetUpcoming()));
app.MapPost("/api/registrations",IResult (RegistrationRequest request,RegistrationValidator validator,EventRepository db)=> {
    var error=validator.Validate(request.EventId,request.FullName,request.Email);
    if(error!=null) return Results.BadRequest(new {error});
    try { return Results.Ok(new {registrationId=db.Register(request)}); }
    catch(SqlException ex) when(ex.Number is 2601 or 2627) {return Results.Conflict(new {error="You are already registered for this event."});}
    catch(InvalidOperationException ex) {return Results.Conflict(new {error=ex.Message});}
});
app.MapGet("/api/admin/events/{eventId:int}/attendees",IResult (int eventId,HttpRequest request,EventRepository db)=> {
    if(!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
       System.Text.Encoding.UTF8.GetBytes(request.Headers["X-Admin-Key"].ToString()),
       System.Text.Encoding.UTF8.GetBytes(adminKey))) return Results.Unauthorized();
    return Results.Ok(db.GetAttendees(eventId));
});
app.Run();
