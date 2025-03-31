// filepath: /UserApiProject/UserApiProject/Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserApiProject.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<UserApiProject.Services.UserService>();

var app = builder.Build();

// Add the error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Add the token validation middleware
app.UseMiddleware<TokenValidationMiddleware>();

// Add the logging middleware
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
