using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using UserManagementAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IServiceValidator, ServiceValidator>();
builder.Services.AddTransient<IHttpLoggingService, HttpLoggingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionHandlerPathFeature =
            context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        // 1. Log the exception (important!)
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>(); // Use your appropriate logger class
        logger.LogError(exception, "An unhandled exception occurred.");


        // 2. Set the response status code
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;  // Or another appropriate status code
        context.Response.ContentType = "application/json"; // Set content type for JSON response

        // 3. Create an error response object (customize as needed)
        var errorResponse = new
        {
            Error = "An internal server error occurred.", // User-friendly message (avoid revealing sensitive details)
            // Optionally include more details for debugging in development (RequestId, etc.)
            // RequestId = context.TraceIdentifier  
        };

        // 4. Write the JSON response to the client
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));


    });
});

List<User> users = new List<User>();

app.Use(async (context, next) =>
{
    var httpLoggingService = context.RequestServices.GetRequiredService<IHttpLoggingService>();
    // Request logging
    var requestBody = await httpLoggingService.ReadRequestBody(context.Request);
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
    Console.WriteLine($"Request Body: {requestBody}");
    
    var originalBodyStream = context.Response.Body;
    using var responseBodyStream = new MemoryStream();
    context.Response.Body = responseBodyStream; // Assign Memory stream before next middleware

    await next(); // Call the next middleware in the pipeline

    // Rewind the memory stream after next middleware completes
    responseBodyStream.Position = 0;
    
    //Copy the content to the original stream
    await responseBodyStream.CopyToAsync(originalBodyStream);
    
    // Rewind the memory stream again after copying
    responseBodyStream.Position = 0;


    // Response logging
    var responseBody = await httpLoggingService.ReadResponseBody(responseBodyStream);
    Console.WriteLine($"Response: {context.Response.StatusCode}");
    Console.WriteLine($"Response Body: {responseBody}");

});

// Create (POST)
app.MapPost("/users", (User user, IServiceValidator validator) =>
{
    if (!validator.TryValidate(user, out var errors))
    {
        return Results.BadRequest(errors);
    }
    
    user.Id = users.Count + 1; // Simple ID generation (not production-ready)
    users.Add(user);
    return Results.Created($"/users/{user.Id}", user);
});

// Read (GET) - single user
app.MapGet("/users/{id}", (int id) =>
{
    var user = users.Find(u => u.Id == id);
    return user is not null ? Results.Ok(user) : Results.NotFound();
});

// Read (GET) - all users
app.MapGet("/users", () => Results.Ok(users));


// Update (PUT)
app.MapPut("/users/{id}", (int id, User updatedUser , IServiceValidator validator) =>
{
    if (!validator.TryValidate(updatedUser, out var errors))
    {
        return Results.BadRequest(errors);
    }
    var user = users.Find(u => u.Id == id);
    if (user is null) return Results.NotFound();
    // Update user properties (e.g., user.FirstName = updatedUser.FirstName)
    return Results.NoContent();
});

// Delete (DELETE)
app.MapDelete("/users/{id}", (int id) =>
{
    var user = users.Find(u => u.Id == id);
    if (user is null) return Results.NotFound();
    users.Remove(user);
    return Results.NoContent();
});


app.Run();
