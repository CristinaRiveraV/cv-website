public class Language
{
    public Language(string name, LanguageProficiency proficiency)
    {
        Name = name;
        Proficiency = proficiency;
    }
    public string Name { get; }
    public LanguageProficiency Proficiency { get; }
}
