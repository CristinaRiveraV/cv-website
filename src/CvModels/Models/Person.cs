using MongoDB.Bson.Serialization.Attributes;

public class Person
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string Id { get; set; }
    public Identity Identity { get; set; }
    public ContactInformation ContactInformation { get; set; }
    public List<Experience> Experiences { get; set; }
    public List<Education> Education { get; set; }
    public List<Project> Projects { get; set; }    
    public List<Skill> AdditionalSkills { get; set; }
    public List<Language> Languages { get; set; }
    public List<Skill> AllSkills => Experiences
      .SelectMany(e => e.Skills)
      .Concat(Projects.SelectMany(p => p.Skills))
      .Concat(AdditionalSkills)
      .GroupBy(s => new { s.Name, s.Category })
      .Select(g => g.OrderByDescending(s => s.Proficiency).First())
      .ToList();
}
