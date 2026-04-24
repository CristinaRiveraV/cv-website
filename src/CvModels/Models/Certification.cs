public class Certification
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Issuer { get; set; }
    public required DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public required string Description { get; set; }
}