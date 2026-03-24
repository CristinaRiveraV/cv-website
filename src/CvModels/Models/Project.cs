public class Project
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<Skill> Skills { get; set; }
    public string? Url { get; set; }
    public string? RepoUrl { get; set; }
    public string? ImageUrl { get; set; }
    public Education? Education { get; set; }
    public Experience? Experience { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsOngoing => !EndDate.HasValue || EndDate.Value >= DateOnly.FromDateTime(DateTime.Now);
}