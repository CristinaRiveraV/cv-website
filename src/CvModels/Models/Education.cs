public class Education()
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Institution { get; set; }
    public string Location { get; set; }
    public int? StartYear { get; set; }
    public int EndYear { get; set; }
    public string Description { get; set; }
    public List<Course> Courses { get; set; }
}
