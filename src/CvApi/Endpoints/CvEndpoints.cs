using CvApi.Services;

namespace CvApi.Endpoints;
public static class CvEndpoints
{
    public static RouteGroupBuilder MapCvEndpoints(this RouteGroupBuilder cvGroup)
    {
        cvGroup.MapGet("", (CvService cv) => cv.GetFullCv())
            .Produces<Person>()
            .WithSummary("Get the full CV");

        cvGroup.MapGet("/identity", (CvService cv) => cv.GetIdentity())
            .Produces<Identity>()
            .WithSummary("Get name, title, and personal summary");

        cvGroup.MapPut("/identity", (Identity identity, CvService cv) => Results.Ok(cv.UpdateIdentity(identity)))
             .RequireAuthorization()
             .Accepts<Identity>("application/json")
             .Produces<Identity>()
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden)
             .WithSummary("Replace name, title, summary, and location");

        cvGroup.MapGet("/contact", (CvService cv) => cv.GetContactInformation())
            .Produces<ContactInformation>()
            .WithSummary("Get email, phonenumber, linkedin, github and portfolio links");

        cvGroup.MapGet("/experiences", (CvService cv) => cv.GetExperiences())
            .Produces<List<Experience>>()
            .WithSummary("Get a list of all experiences");

        cvGroup.MapGet("/experiences/{id}", (string id, CvService cv) =>
            {
                var experience = cv.GetExperience(id);
                return experience is not null
                    ? Results.Ok(experience)
                    : Results.NotFound(new { error = "Experience not found" });
            })
            .Produces<Experience>()
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Get a specific work experience by ID");

        cvGroup.MapGet("/education", (CvService cv) => cv.GetEducation())
            .Produces<List<Education>>()
            .WithSummary("Get a list of all education history");

        cvGroup.MapGet("/education/{id}", (string id, CvService cv) =>
            {
                var education = cv.GetEducation(id);
                return education is not null
                    ? Results.Ok(education)
                    : Results.NotFound(new { error = "Education not found" });
            })
            .Produces<Education>()
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Get a specific education by ID");

        cvGroup.MapGet("/projects", (CvService cv) => cv.GetProjects())
            .Produces<List<Project>>()
            .WithSummary("Get a list of all projects");

        cvGroup.MapGet("/projects/{id}", (string id, CvService cv) =>
            {
                var projects = cv.GetProject(id);
                return projects is not null
                    ? Results.Ok(projects)
                    : Results.NotFound(new { error = "Projects not found" });
            })
            .Produces<Project>()
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Get a specific project by ID");

        cvGroup.MapGet("/skills", (CvService cv) => cv.GetSkills())
            .Produces<List<Skill>>()
            .WithSummary("Get a list of all skills");

        cvGroup.MapGet("/languages", (CvService cv) => cv.GetLanguages())
            .Produces<List<Language>>()
            .WithSummary("Get a list of all languages");

        return cvGroup;
    }
}