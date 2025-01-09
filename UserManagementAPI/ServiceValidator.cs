using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI;

public class ServiceValidator : IServiceValidator
{
    public bool TryValidate<T>(T obj, out IDictionary<string, string[]> errors)
    {
        var validationResults = new List<ValidationResult>();
        if (obj != null)
        {
            var context = new ValidationContext(obj);
            var isValid = Validator.TryValidateObject(obj, context, validationResults, true);

            errors = validationResults.GroupBy(
                vr => vr.MemberNames.FirstOrDefault() ?? string.Empty,
                vr => vr.ErrorMessage
            ).ToDictionary(
                g => g.Key,
                g => g.ToArray()
            )!;

            return isValid;
        }

        errors = null!;
        return false;
    }
}