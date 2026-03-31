using CvApi.Endpoints;
using CvApi.Repositories;
using CvApi.Services;
using CvApi.Settings;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Read MongoDB settings from appsettings
var mongoSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDbSettings>();
if (mongoSettings == null)
{
    Console.WriteLine("Error: MongoDB settings not found in configuration.");
    return;
}

// Register services
builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.WriteIndented = true;
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSingleton(mongoSettings);
builder.Services.AddSingleton<CvRepository>();
builder.Services.AddSingleton<CvService>();

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.Authority = "https://criveravaldez.uk.auth0.com/";
        options.Audience = "https://cv-api";
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            error = "An unexpected error occurred. Please try again later."
        });
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

var cvGroup = app.MapGroup("/cv")
    .WithTags("CV")
    .RequireAuthorization();

cvGroup.MapCvEndpoints();

app.Run();