using System.Text.Json.Serialization;

public enum SkillCategory
{
    Other,
    [JsonStringEnumMemberName("Soft Skills")] SoftSkills,
    Backend,
    Frontend,
    Database,
    DevOps, 
    Testing,
    [JsonStringEnumMemberName("AI & Tooling")] AIandTooling
}