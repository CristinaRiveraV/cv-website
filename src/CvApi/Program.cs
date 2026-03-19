using CvApi.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using CvApi.Endpoints;

var options = new JsonSerializerOptions
{
    Converters = { new JsonStringEnumConverter() }
};

var json = File.ReadAllText(Path.Combine("..", "..", "appsettings.Development.template.json"));
var personWrapper = JsonSerializer.Deserialize<PersonWrapper>(json, options);
var person = personWrapper?.Person;

if (person?.Identity == null || person?.ContactInformation == null)
{
    Console.WriteLine("Error: Could not load required config sections.");
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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
builder.Services.AddSingleton(person);
builder.Services.AddSingleton<CvService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors();

var cvGroup = app.MapGroup("/cv")
    .WithTags("CV");

cvGroup.MapCvEndpoints();

app.Run();