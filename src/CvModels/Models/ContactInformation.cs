public class ContactInformation
{
    public ContactInformation(string email, string? phone, string linkedIn, string gitHub, string? portfolio)
    {
        Email = email;
        Phone = phone;
        LinkedIn = linkedIn;
        GitHub = gitHub;
        Portfolio = portfolio;
    }
    public string Email { get; }
    public string? Phone { get; }
    public string LinkedIn { get; }
    public string GitHub { get; }
    public string? Portfolio { get; }
}