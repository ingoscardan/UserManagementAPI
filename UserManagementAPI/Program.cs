using System.ComponentModel.DataAnnotations;
using UserManagementAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

List<User> users = new List<User>();

// Create (POST)
app.MapPost("/users", (User user) =>
{
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
app.MapPut("/users/{id}", (int id, User updatedUser) =>
{
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

// Helper function for validation
static bool TryValidate<T>(T obj, out IDictionary<string, string[]> errors)
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

app.Run();
