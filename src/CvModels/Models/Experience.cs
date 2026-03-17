public class Experience
{
    public Experience(Guid id, string role, string company, string location, string summary, DateOnly startDate, DateOnly? endDate, WorkMode mode, List<Skill> skills, List<Responsibility> responsibilities)
    {
        Id = id;
        Role = role;
        Company = company;
        Location = location;
        Summary = summary;
        StartDate = startDate;
        EndDate = endDate;
        Mode = mode;
        Skills = skills ?? new List<Skill>();
        Responsibilities = responsibilities ?? new List<Responsibility>();
    }
    public Guid Id { get; }
    public string Role{ get; }
    public string Company { get; }
    public string Location { get; }
    public string Summary { get; }
    public DateOnly StartDate { get; }
    public DateOnly? EndDate { get; }
    public WorkMode Mode { get; }
    public List<Skill> Skills { get; }
    public List<Responsibility> Responsibilities { get; }
}
