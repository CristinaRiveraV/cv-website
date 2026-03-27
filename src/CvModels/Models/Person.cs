using MongoDB.Bson.Serialization.Attributes;

public class Person
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string Id { get; set; }
    public required Identity Identity { get; set; }
    public required ContactInformation ContactInformation { get; set; }
    public required List<Experience> Experiences { get; set; }
    public required List<Education> Education { get; set; }
    public required List<Project> Projects { get; set; }    
    public required List<Skill> AdditionalSkills { get; set; }
    public required List<Language> Languages { get; set; }
    public List<Skill> AllSkills => Experiences
      .SelectMany(e => e.Skills)
      .Concat(Projects.SelectMany(p => p.Skills))
      .Concat(AdditionalSkills)
      .GroupBy(s => new { s.Name, s.Category })
      .Select(g => g.OrderByDescending(s => s.Proficiency).First())
      .ToList();
}
