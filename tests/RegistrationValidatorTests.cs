using CampusEvents;
using Xunit;
public class RegistrationValidatorTests {
    // Hand-written mock with invocation recording; no SQL Server connection is used.
    private sealed class SeatMock(bool available) : ISeatAvailability {
        public int Calls {get;private set;}
        public int LastEventId {get;private set;}
        public bool HasAvailableSeat(int eventId) {Calls++;LastEventId=eventId;return available;}
    }
    [Theory]
    [InlineData("student@univ.edu.ph")]
    [InlineData("  STUDENT@UNIV.EDU.PH  ")]
    public void AcceptsUniversityEmailAndCallsMock(string email) {
        var mock=new SeatMock(true);var validator=new RegistrationValidator(mock);
        Assert.Null(validator.Validate(7,"Student",email));
        Assert.Equal(1,mock.Calls);Assert.Equal(7,mock.LastEventId);
    }
    [Theory]
    [InlineData(null)] [InlineData("")] [InlineData("student@gmail.com")]
    [InlineData("student@univ.edu.ph.evil.com")]
    [InlineData("student@sub.univ.edu.ph")]
    [InlineData("a@@univ.edu.ph")] [InlineData("a b@univ.edu.ph")]
    public void InvalidEmailDoesNotCallDependency(string? email) {
        var mock=new SeatMock(true);var validator=new RegistrationValidator(mock);
        Assert.NotNull(validator.Validate(7,"Student",email));Assert.Equal(0,mock.Calls);
    }
    [Fact] public void FullEventIsRejected() {
        var mock=new SeatMock(false);
        Assert.NotNull(new RegistrationValidator(mock).Validate(7,"Student","a@univ.edu.ph"));Assert.Equal(1,mock.Calls);
    }
    [Theory] [InlineData(0)] [InlineData(-1)]
    public void InvalidEventDoesNotCallDependency(int id) {
        var mock=new SeatMock(true);Assert.NotNull(new RegistrationValidator(mock).Validate(id,"Student","a@univ.edu.ph"));Assert.Equal(0,mock.Calls);
    }
    [Fact] public void MissingNameDoesNotCallDependency() {
        var mock=new SeatMock(true);Assert.NotNull(new RegistrationValidator(mock).Validate(1," ","a@univ.edu.ph"));Assert.Equal(0,mock.Calls);
    }
    [Fact] public void LongNameIsRejected() {
        var mock=new SeatMock(true);Assert.NotNull(new RegistrationValidator(mock).Validate(1,new string('a',101),"a@univ.edu.ph"));Assert.Equal(0,mock.Calls);
    }
}
