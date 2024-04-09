namespace JournalApp.Tests;

public class IndexTests : JaTestContext
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        AddDbContext();
        Services.GetService<AppDbSeeder>().SeedCategories();
    }

    [Fact(Skip = "Stub")]
    public async Task CanSwitchDays() { }

    [Fact(Skip = "Stub")]
    public async Task SwitchDaysPersistsData() { }

    [Fact(Skip = "Stub")]
    public async Task CannotGoIntoFuture() { }

    [Fact(Skip = "Stub")]
    public async Task OpenCalendar() { }

    [Fact(Skip = "Stub")]
    public async Task HideTodaysNotes() { }
}
