using System.Text;

namespace UserManagementAPI;

public class HttpLoggingService : IHttpLoggingService
{
    public async Task<string> ReadRequestBody(HttpRequest request)
    {
        request.EnableBuffering();

        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        request.Body.Position = 0;
        return body;
    }

    public async Task<string> ReadResponseBody(Stream stream)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}