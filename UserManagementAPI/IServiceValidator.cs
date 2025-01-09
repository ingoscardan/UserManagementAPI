namespace UserManagementAPI;

public interface IServiceValidator
{
    bool TryValidate<T>(T obj, out IDictionary<string, string[]> errors);
}