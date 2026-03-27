using CvApi.Settings;
using MongoDB.Driver;

namespace CvApi.Repositories;

public class CvRepository
{
    private readonly IMongoCollection<Person> _profiles;

    public CvRepository(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _profiles = database.GetCollection<Person>("Profiles");
    }

    public Person? GetPerson() =>
        _profiles.Find(_ => true).FirstOrDefault();

    public void InsertPerson(Person person) =>
        _profiles.InsertOne(person);
}