using System.Text.Json;
using System.Text.Json.Serialization;

var options = new JsonSerializerOptions
  {
      Converters = { new JsonStringEnumConverter() }
  };

var json = File.ReadAllText( Path.Combine(AppContext.BaseDirectory, "appsettings.Development.template.json") );
var personWrapper = JsonSerializer.Deserialize<PersonWrapper>(json, options);
var person = personWrapper?.Person;

if (person?.Identity == null || person?.ContactInformation == null)
{
    Console.WriteLine("Error: Could not load required config sections.");
    return;
}

Console.WriteLine($"Name: {person.Identity.Name}");
Console.WriteLine($"Job Title: {person.Identity.JobTitle}");
Console.WriteLine($"Email: {person.ContactInformation.Email}");
Console.WriteLine($"Phone: {person.ContactInformation.Phone}");
Console.WriteLine($"LinkedIn: {person.ContactInformation.LinkedIn}");
Console.WriteLine($"GitHub: {person.ContactInformation.GitHub}");
Console.WriteLine($"Language: {person.Languages[0].Name} ({person.Languages[0].Proficiency})");
Console.WriteLine($"First Experience: {person.Experiences[0].Company} - {person.Experiences[0].Role}");
