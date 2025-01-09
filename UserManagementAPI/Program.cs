using System.ComponentModel.DataAnnotations;
using UserManagementAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IServiceValidator, ServiceValidator>();

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
