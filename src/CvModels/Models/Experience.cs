public class Experience
{
    public required string Id { get; set; }
    public required string Role{ get; set; }
    public required string Company { get; set; }
    public required string Location { get; set; }
    public required string Summary { get; set; }
    public required DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public required WorkMode Mode { get; set; }
    public required List<Skill> Skills { get; set; }
    public required List<Responsibility> Responsibilities { get; set; }
}
