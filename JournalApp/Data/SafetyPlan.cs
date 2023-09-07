namespace JournalApp;

public record SafetyPlan
{
    public string WarningSigns { get; set; }
    public string CopingStrategies { get; set; }
    public string SocialDistractions { get; set; }
    public string SocialContacts { get; set; }
    public string ProfessionalContacts { get; set; } = "https://en.wikipedia.org/wiki/List_of_suicide_crisis_lines\n";
    public string EnvironmentChanges { get; set; }
    public string Purpose { get; set; }
}
