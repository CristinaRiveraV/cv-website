public class Responsibility
{
    public Responsibility(string description, string category, bool isAchievement = false)
    {
        Description = description;
        Category = category;
        IsAchievement = isAchievement;
    }
    public string Description { get; }
    public string Category { get; }
    public bool IsAchievement { get; }
}