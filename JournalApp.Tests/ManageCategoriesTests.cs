namespace JournalApp.Tests;

public class ManageCategoriesTests : JaTestContext
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        AddDbContext();
    }

    [Fact(Skip = "Stub")]
    public async Task ClickCategoryOpensEditorAndUpdatesList() { }

    [Fact(Skip = "Stub")]
    public async Task ClickPlusOpensEditorAndAddsNewCategoryToList() { }

    [Fact(Skip = "Stub")]
    public async Task ClickCategoryOpensEditor() { }

    [Fact(Skip = "Stub")]
    public async Task CannotEditReadOnlyCategories() { }

    [Fact(Skip = "Stub")]
    public async Task Reorder() { }
}
