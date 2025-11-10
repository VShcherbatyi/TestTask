using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Threading.RateLimiting;
using testtask.Data;
using testtask.Middlewares;
using testtask.Services.Implementations;
using testtask.Services.Interfaces;
using testtask.Settings;

var builder = WebApplication.CreateBuilder(args);

var rateLimitOptions = new RateLimitOptions();
builder.Configuration.GetSection("RateLimiting").Bind(rateLimitOptions);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IDogService, DogService>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        return new BadRequestObjectResult(new
        {
            errors
        });
    };
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: "global",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitOptions.PermitLimit,
                Window = TimeSpan.FromSeconds(rateLimitOptions.WindowSeconds),
                QueueLimit = rateLimitOptions.QueueLimit,
                AutoReplenishment = rateLimitOptions.AutoReplenishment
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (System.Text.Json.JsonException)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"message\": \"Invalid JSON format in request body\"}");
    }
});

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
