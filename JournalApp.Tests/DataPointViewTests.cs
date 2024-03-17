namespace JournalApp.Tests;

public class DataPointViewTests : JaTestContext
{
    [Fact]
    public async Task Mood()
    {
        // TODO: Don't use DB, or maybe initialize all the properties below at the start.

        AddDbContext();
        var db = Services.GetService<AppDbContext>();
        var category = db.Categories.Single(c => c.Guid.ToString() == "D90D89FB-F5B9-47CF-AE4E-3EC0D635E783");
        var day = await db.GetOrCreateDayAndAddPoints(new(2024, 01, 01));
        var point = DataPoint.Create(day, category);
        point.Mood = "🤩";

        var cut = RenderComponent<DataPointView>(p =>
            p.Add(x => x.Point, point)
        );

        cut.Find(".mud-menu-activator").Click();

        // Add a razor file with a popover provider so we can check for it.
        // https://github.com/MudBlazor/MudBlazor/blob/1607b06f596bd1ca11bcbfe6c1c4b26e064f9551/src/MudBlazor.UnitTests.Viewer/TestComponents/Menu/MenuTest1.razor
        //cut.FindAll("div.mud-popover-open").Count.Should().Be(1);
    }

    [Fact]
    public async Task Sleep()
    {
        // TODO: Don't use DB.

        AddDbContext();
        var db = Services.GetService<AppDbContext>();
        var category = db.Categories.Single(c => c.Guid.ToString() == "D8657B36-F3A0-486F-BF80-0CF057919C7D");
        var day = await db.GetOrCreateDayAndAddPoints(new(2024, 01, 01));
        var point = DataPoint.Create(day, category);
        point.SleepHours = 8.5m;

        var cut = RenderComponent<DataPointView>(p =>
            p.Add(x => x.Point, point)
        );

        //var slider = cut.FindComponent<MudSlider<decimal>>();
        //slider.Instance.Min.Should().Be(0m);
        //slider.Instance.Max.Should().Be(24m);
    }

    [Fact]
    public void LowToHigh()
    {
        var category = new DataPointCategory
        {
            Guid = Guid.NewGuid(),
            Type = PointType.LowToHigh,
        };

        var day = Day.Create(new(2024, 01, 01));
        var point = DataPoint.Create(day, category);

        var cut = RenderComponent<DataPointView>(p =>
            p.Add(x => x.Point, point)
        );

        point.ScaleIndex.Should().Be(null);
        cut.FindAll(".mud-toggle-item").Count.Should().Be(4);
        cut.FindAll(".mud-toggle-item-selected-border").Count.Should().Be(0);

        cut.FindAll(".mud-toggle-item")[1].Click();
        cut.FindAll(".mud-toggle-item")[1].ClassList.Should().Contain("mud-toggle-item-selected-border");
        point.ScaleIndex.Should().Be(1);

        cut.FindAll(".mud-toggle-item")[2].Click();
        cut.FindAll(".mud-toggle-item")[2].ClassList.Should().Contain("mud-toggle-item-selected-border");
        point.ScaleIndex.Should().Be(3);

        cut.FindAll(".mud-toggle-item")[3].Click();
        cut.FindAll(".mud-toggle-item")[3].ClassList.Should().Contain("mud-toggle-item-selected-border");
        point.ScaleIndex.Should().Be(5);

        cut.FindAll(".mud-toggle-item")[3].Click();
        cut.FindAll(".mud-toggle-item")[3].ClassList.Should().NotContain("mud-toggle-item-selected-border");
        point.ScaleIndex.Should().Be(null);
    }
}
