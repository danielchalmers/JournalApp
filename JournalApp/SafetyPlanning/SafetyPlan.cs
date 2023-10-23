namespace JournalApp.SafetyPlanning;

public record SafetyPlan
{
    public static string Preamble { get; } = "A safety plan is a written list of coping strategies and sources of support can be used before or during a crisis. The plan should be brief, easy to read, and in your own words.";
    public string WarningSigns { get; set; }
    public string CopingStrategies { get; set; }
    public string SocialDistractions { get; set; }
    public string SocialContacts { get; set; }
    public string ProfessionalContacts { get; set; } = "https://en.wikipedia.org/wiki/List_of_suicide_crisis_lines\n";
    public string EnvironmentChanges { get; set; }
    public string Purpose { get; set; }
}
