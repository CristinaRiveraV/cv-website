using System.Runtime.CompilerServices;

public class Project
{
    public Project(string name, string description, List<Skill> skills, string? url = null, string? repoUrl = null, string? imageUrl = null, Education? education = null, Experience? experience = null, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        Name = name;
        Description = description;
        Skills = skills ?? new List<Skill>();
        Url = url;
        RepoUrl = repoUrl;
        ImageUrl = imageUrl;
        Education = education;
        Experience = experience;
        StartDate = startDate;
        EndDate = endDate;
    }
    public string Name { get; }
    public string Description { get; }
    public List<Skill> Skills { get; }
    public string? Url { get; }
    public string? RepoUrl { get; }
    public string? ImageUrl { get; }
    public Education? Education { get; }
    public Experience? Experience { get; }
    public DateOnly? StartDate { get; }
    public DateOnly? EndDate { get; }
    public bool IsOngoing => !EndDate.HasValue || EndDate.Value >= DateOnly.FromDateTime(DateTime.Now);
}