public class Experience()
{
    public string Id { get; set; }
    public string Role{ get; set; }
    public string Company { get; set; }
    public string Location { get; set; }
    public string Summary { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public WorkMode Mode { get; set; }
    public List<Skill> Skills { get; set; }
    public List<Responsibility> Responsibilities { get; set; }
}
