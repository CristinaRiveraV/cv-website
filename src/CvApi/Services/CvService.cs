namespace CvApi.Services;
public class CvService
{
    private readonly Person _person;

    public CvService(Person person)
    {
        _person = person;
    }

    public Person GetPerson() => _person;
    public Identity GetIdentity() => _person.Identity;
    public ContactInformation GetContactInformation() => _person.ContactInformation;
    public List<Skill> GetSkills() => _person.AllSkills;
    public List<Language> GetLanguages() => _person.Languages;
    public List<Experience> GetExperiences() => _person.Experiences;
    public List<Education> GetEducation() => _person.Education;
    public List<Project> GetProjects() => _person.Projects;

    public Education? GetEducation(Guid id) => _person.Education.FirstOrDefault(e => e.Id == id);
    public Project? GetProject(Guid id) => _person.Projects.FirstOrDefault(s => s.Id == id);
    public Experience? GetExperience(Guid id) => _person.Experiences.FirstOrDefault(e => e.Id == id);

}