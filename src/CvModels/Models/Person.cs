public class Person
{
    public Person(Identity identity, ContactInformation contactInformation, List<Experience> experiences, List<Education> education, List<Project> projects, List<Skill> additionalSkills, List<Language> languages)
    {
        Identity = identity;
        ContactInformation = contactInformation;
        Experiences = experiences ?? new List<Experience>();
        Education = education ?? new List<Education>();
        Projects = projects ?? new List<Project>();
        AdditionalSkills = additionalSkills ?? new List<Skill>();
        Languages = languages ?? new List<Language>();
    }
    public Identity Identity { get; }
    public ContactInformation ContactInformation { get; }
    public List<Experience> Experiences { get; }
    public List<Education> Education { get; }
    public List<Project> Projects { get; }    
    public List<Skill> AdditionalSkills { get; }
    public List<Language> Languages { get; }
    public List<Skill> AllSkills => Experiences
      .SelectMany(e => e.Skills)
      .Concat(Projects.SelectMany(p => p.Skills))
      .Concat(AdditionalSkills)
      .GroupBy(s => new { s.Name, s.Category })
      .Select(g => g.OrderByDescending(s => s.Proficiency).First())
      .ToList();
}
