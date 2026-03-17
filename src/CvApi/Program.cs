using CvApi.Services;
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
builder.Services.AddSingleton(person);
builder.Services.AddSingleton<CvService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

var cvGroup = app.MapGroup("/cv");

cvGroup.MapGet("", (CvService cv) => cv.GetPerson());
cvGroup.MapGet("/identity", (CvService cv) => cv.GetIdentity());
cvGroup.MapGet("/contact", (CvService cv) => cv.GetContactInformation());
cvGroup.MapGet("/experiences", (CvService cv) => cv.GetExperiences());
cvGroup.MapGet("/experiences/{id}", (Guid id, CvService cv) => 
{
    var experience = cv.GetExperience(id);
    return experience is not null
        ? Results.Ok(experience)
        : Results.NotFound(new { error = "Experience not found" });
});
cvGroup.MapGet("/education", (CvService cv) => cv.GetEducation());
cvGroup.MapGet("/education/{id}", (Guid id, CvService cv) =>
{
    var education = cv.GetEducation(id);
    return education is not null
        ? Results.Ok(education)
        : Results.NotFound(new { error = "Education not found" });
});
cvGroup.MapGet("/projects", (CvService cv) => cv.GetProjects());
cvGroup.MapGet("/projects/{id}", (Guid id, CvService cv) =>
{
    var projects = cv.GetProject(id);
    return projects is not null
        ? Results.Ok(projects)
        : Results.NotFound(new { error = "Projects not found" });
});
cvGroup.MapGet("/skills", (CvService cv) => cv.GetSkills());
cvGroup.MapGet("/languages", (CvService cv) => cv.GetLanguages());
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