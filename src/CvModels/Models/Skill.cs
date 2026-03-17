public class Skill
{
    public Skill(string name, SkillCategory category, int proficiency)
    {
        if(!IsValidProficiency(proficiency))        
            throw new ArgumentOutOfRangeException(nameof(proficiency), "Proficiency must be between 0 and 10.");
        
        Name = name;
        Category = category;
        Proficiency = proficiency;
    }
    public string Name { get; }
    public SkillCategory Category { get; }
    public int Proficiency { get; private set;}

    public void UpdateProficiency(int newProficiency)
    {
        if(!IsValidProficiency(newProficiency))        
            throw new ArgumentOutOfRangeException(nameof(newProficiency), "Proficiency must be between 0 and 10.");
        
        Proficiency = newProficiency;
    }

    private bool IsValidProficiency(int proficiency)
    {
        return proficiency >= 0 && proficiency <= 10;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Skill other) return false;
        return Name == other.Name && Category == other.Category;
    }

    public override int GetHashCode()
  {
      return HashCode.Combine(Name, Category);
  }
}
