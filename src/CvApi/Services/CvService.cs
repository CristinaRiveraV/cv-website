using CvApi.Repositories;

namespace CvApi.Services;
public class CvService
{
    private readonly CvRepository _repository;
    private Person? _cachedPerson;

    public CvService(CvRepository repository)
    {
        _repository = repository;
    }
    private Person GetPerson()
    {
        _cachedPerson ??= _repository.GetPerson();
        return _cachedPerson ?? throw new InvalidOperationException("No CV data found in database.");
    }

    public Person GetFullCv() => GetPerson();
    public Identity GetIdentity() =>GetPerson().Identity;
    public ContactInformation GetContactInformation() =>GetPerson().ContactInformation;
    public List<Skill> GetSkills() =>GetPerson().AllSkills;
    public List<Language> GetLanguages() =>GetPerson().Languages;
    public List<Experience> GetExperiences() =>GetPerson().Experiences;
    public List<Education> GetEducation() =>GetPerson().Education;
    public List<Project> GetProjects() =>GetPerson().Projects;

    public Education? GetEducation(string id) =>GetPerson().Education.FirstOrDefault(e => e.Id == id);
    public Project? GetProject(string id) =>GetPerson().Projects.FirstOrDefault(s => s.Id == id);
    public Experience? GetExperience(string id) =>GetPerson().Experiences.FirstOrDefault(e => e.Id == id);

}