public class Identity
{
    public Identity(string name, string jobTitle, string personalSummary)
    {
        Name = name;
        JobTitle = jobTitle;
        PersonalSummary = personalSummary;
    }
    public string Name { get; }
    public string JobTitle { get; }
    public string PersonalSummary { get; }
}
