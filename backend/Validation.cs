namespace CampusEvents;
public interface ISeatAvailability {
    bool HasAvailableSeat(int eventId);
}
public sealed class RegistrationValidator(ISeatAvailability seats) {
    public static string NormalizeEmail(string? email) => (email ?? "").Trim().ToLowerInvariant();
    public static bool IsStudentEmail(string? email) {
        var value = NormalizeEmail(email);
        if (value.Length > 254 || value.Length == 0 || value.Any(char.IsWhiteSpace)) return false;
        if (!System.Net.Mail.MailAddress.TryCreate(value, out var parsed)) return false;
        return parsed.Address == value && parsed.Host.Equals("univ.edu.ph", StringComparison.OrdinalIgnoreCase)
            && parsed.User.Length > 0 && !parsed.User.Contains('@');
    }
    public string? Validate(int eventId, string? name, string? email) {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100) return "Enter a name of 1 to 100 characters.";
        if (!IsStudentEmail(email)) return "Use your @univ.edu.ph student email.";
        if (eventId <= 0) return "Choose an event.";
        return seats.HasAvailableSeat(eventId) ? null : "This event is full, unavailable, or already started.";
    }
}
