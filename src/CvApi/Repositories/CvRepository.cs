using CvApi.Settings;
using MongoDB.Driver;

namespace CvApi.Repositories;

public class CvRepository
{
    private readonly IMongoCollection<Person> _people;

    public CvRepository(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _people = database.GetCollection<Person>("people");
    }

    public Person? GetPerson() =>
        _people.Find(_ => true).FirstOrDefault();

    public void InsertPerson(Person person) =>
        _people.InsertOne(person);

    public void SeedFromJson(string jsonFilePath)
    {
        // Only seed if the collection is empty
        if (_people.CountDocuments(_ => true) > 0)
            return;

        var options = new System.Text.Json.JsonSerializerOptions
        {
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        var json = File.ReadAllText(jsonFilePath);
        var person = System.Text.Json.JsonSerializer.Deserialize<Person>(json, options);

        if (person != null)
            _people.InsertOne(person);
    }
}