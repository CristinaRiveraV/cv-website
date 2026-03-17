using System.Text.Json;
using System.Text.Json.Serialization;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapGet("/cv", () => person);
app.MapGet("/cv/identity", () => person.Identity);
app.MapGet("/cv/contact", () => person.ContactInformation);
app.MapGet("/cv/experiences", () => person.Experiences);
app.MapGet("/cv/education", () => person.Education);
app.MapGet("/cv/projects", () => person.Projects);
app.MapGet("/cv/skills", () => person.AllSkills);
app.MapGet("/cv/languages", () => person.Languages);
app.MapGet("/", () => new
{
    message = "CV API",
    endpoints = new
    {
        cv = "/cv",
        identity = "/cv/identity",
        contact = "/cv/contact",
        experiences = "/cv/experiences",
        education = "/cv/education",
        projects = "/cv/projects",
        skills = "/cv/skills",
        languages = "/cv/languages"
    }
});

app.Run();