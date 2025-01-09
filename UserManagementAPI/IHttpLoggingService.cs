namespace UserManagementAPI;

public interface IHttpLoggingService
{
    Task<string> ReadRequestBody(HttpRequest request);
    Task<string> ReadResponseBody(Stream stream);
}