namespace JournalApp.Tests;

public class EditCategoryTests : JaTestContext
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        AddDbContext();
        Services.GetService<AppDbSeeder>().SeedCategories();
    }

    [Fact(Skip = "Stub")]
    public async Task EditExistingCategory()
    {
        var category = new DataPointCategory
        {
            Guid = Guid.NewGuid(),
            Type = PointType.Note,
            Name = "My note",

        };

        var dialogService = Services.GetService<IDialogService>();
        var cut = RenderComponent<MudDialogProvider>();

        //var parameters = new DialogParameters<EditCategoryDialog> { { x => x.Group, category.Group }, { x => x.Category, category } };
        //var result = await DialogService.Show<EditCategoryDialog>(parameters).Result;

        IDialogReference dialogReference = null;
        await cut.InvokeAsync(() => dialogReference = dialogService?.Show<EditCategoryDialog>());
    }

    [Fact(Skip = "Stub")]
    public async Task EditNewCategory() { }

    [Fact(Skip = "Stub")]
    public async Task EditExistingMedication() { }

    [Fact(Skip = "Stub")]
    public async Task EditNewMedication() { }

    [Fact(Skip = "Stub")]
    public async Task DeleteCategory() { }

    [Fact(Skip = "Stub")]
    public async Task BackButtonSaves() { }

    [Fact(Skip = "Stub")]
    public async Task SaveButtonSaves() { }

    [Fact(Skip = "Stub")]
    public async Task CancelButtonCancels() { }
}
