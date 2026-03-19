public class Education
{
    public Education(Guid id, string name, string institution, string location, int? startYear, int endYear, string description, List<Course> courses)
    {
        Id = id;
        Name = name;
        Institution = institution;
        Location = location;
        StartYear = startYear;
        EndYear = endYear;
        Description = description;
        Courses = courses ?? new List<Course>();
    }
    public Guid Id { get; }
    public string Name { get; }
    public string Institution { get; }
    public string Location { get; }
    public int? StartYear { get; }
    public int EndYear { get; }
    public string Description { get; }
    public List<Course> Courses { get; }
}
