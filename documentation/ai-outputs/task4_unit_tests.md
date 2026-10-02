# Task 4 - Unit tests with mocks

Tool: Google Gemini (gemini.google.com), 2026-10-02

## Prompt

```text
Write xUnit unit tests for a C# RegistrationValidator that checks @univ.edu.ph emails and seat availability, using a mock object for seat availability.
```

## Gemini output

Here is an implementation of the unit tests using **xUnit** and **Moq**, assuming an `IRegistrationValidator` that coordinates domain logic and an `ISeatAvailabilityService` dependency.

## Class Contracts Under Test

```csharp
public interface ISeatAvailabilityService
{
    Task<bool> HasAvailableSeatsAsync(string courseCode);
}

public class RegistrationValidator
{
    private readonly ISeatAvailabilityService _seatService;
    private const string AllowedDomain = "@univ.edu.ph";

    public RegistrationValidator(ISeatAvailabilityService seatService)
    {
        _seatService = seatService;
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(string email, string courseCode)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.EndsWith(AllowedDomain, StringComparison.OrdinalIgnoreCase))
        {
            return (false, $"Email must end with {AllowedDomain}.");
        }

        bool hasSeats = await _seatService.HasAvailableSeatsAsync(courseCode);
        if (!hasSeats)
        {
            return (false, "No seats available for this course.");
        }

        return (true, null);
    }
}
```

## xUnit Test Suite

```csharp
using System;
using System.Threading.Tasks;
using Moq;
using Xunit;

public class RegistrationValidatorTests
{
    private readonly Mock<ISeatAvailabilityService> _seatServiceMock;
    private readonly RegistrationValidator _validator;

    public RegistrationValidatorTests()
    {
        _seatServiceMock = new Mock<ISeatAvailabilityService>();
        _validator = new RegistrationValidator(_seatServiceMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_ValidEmailAndSeatsAvailable_ReturnsSuccess()
    {
        // Arrange
        const string email = "student@univ.edu.ph";
        const string courseCode = "CS101";

        _seatServiceMock
            .Setup(s => s.HasAvailableSeatsAsync(courseCode))
            .ReturnsAsync(true);

        // Act
        var (isValid, errorMessage) = await _validator.ValidateAsync(email, courseCode);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
        _seatServiceMock.Verify(s => s.HasAvailableSeatsAsync(courseCode), Times.Once);
    }

    [Theory]
    [InlineData("student@gmail.com")]
    [InlineData("student@univ.edu")]
    [InlineData("student@univ.edu.ph.fake.com")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidateAsync_InvalidEmail_FailsWithoutCheckingSeatAvailability(string? invalidEmail)
    {
        // Arrange
        const string courseCode = "CS101";

        // Act
        var (isValid, errorMessage) = await _validator.ValidateAsync(invalidEmail!, courseCode);

        // Assert
        Assert.False(isValid);
        Assert.Contains("@univ.edu.ph", errorMessage);

        // Ensure the external seat check is short-circuited when email validation fails
        _seatServiceMock.Verify(s => s.HasAvailableSeatsAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_ValidEmail_CaseInsensitiveDomain_ReturnsSuccess()
    {
        // Arrange
        const string email = "student@UNIV.EDU.PH";
        const string courseCode = "MATH101";

        _seatServiceMock
            .Setup(s => s.HasAvailableSeatsAsync(courseCode))
            .ReturnsAsync(true);

        // Act
        var (isValid, errorMessage) = await _validator.ValidateAsync(email, courseCode);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Fact]
    public async Task ValidateAsync_ValidEmailButNoSeatsAvailable_ReturnsFailure()
    {
        // Arrange
        const string email = "student@univ.edu.ph";
        const string courseCode = "ENG201";

        _seatServiceMock
            .Setup(s => s.HasAvailableSeatsAsync(courseCode))
            .ReturnsAsync(false);

        // Act
        var (isValid, errorMessage) = await _validator.ValidateAsync(email, courseCode);

        // Assert
        Assert.False(isValid);
        Assert.Equal("No seats available for this course.", errorMessage);
        _seatServiceMock.Verify(s => s.HasAvailableSeatsAsync(courseCode), Times.Once);
    }
}
```

## Key Highlights

- **Short-circuit verification (`Times.Never`):** Verifies that if the email is invalid, the mock service is never queried, preventing unnecessary external calls.
- **Parametrized checks (`[Theory]` + `[InlineData]`):** Handles empty strings, nulls, wrong TLDs, and prefix spoofing like `@univ.edu.ph.fake.com`.
- **Case-insensitivity:** Confirms institutional domains like `@UNIV.EDU.PH` pass validation according to standard email URI handling.
