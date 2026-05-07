using CvApi.Settings;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

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

    public void UpdateIdentity(Identity identity) =>
        _profiles.UpdateOne(
            Builders<Person>.Filter.Empty,
            Builders<Person>.Update.Set(p => p.Identity, identity));

    public void UpdateContactInformation(ContactInformation contact) =>
        _profiles.UpdateOne(
            Builders<Person>.Filter.Empty,
            Builders<Person>.Update.Set(p => p.ContactInformation, contact));

    public bool TryAddExperience(Experience experience)
    {
        var idTaken = _profiles
            .Find(Builders<Person>.Filter.ElemMatch(p => p.Experiences, e => e.Id == experience.Id))
            .Any();
        if (idTaken) return false;

        _profiles.UpdateOne(
            Builders<Person>.Filter.Empty,
            Builders<Person>.Update.Push(p => p.Experiences, experience));
        return true;
    }

    public bool TryUpdateExperience(string id, Experience experience)
    {
        var filter = Builders<Person>.Filter.ElemMatch(p => p.Experiences, e => e.Id == id);
        var update = Builders<Person>.Update.Set(p => p.Experiences.FirstMatchingElement(), experience);
        var result = _profiles.UpdateOne(filter, update);
        return result.MatchedCount > 0;
    }
}