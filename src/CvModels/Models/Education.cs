public class Education
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Institution { get; set; }
    public required string Location { get; set; }
    public int? StartYear { get; set; }
    public required int EndYear { get; set; }
    public required string Description { get; set; }
    public List<Course>? Courses { get; set; }
}
