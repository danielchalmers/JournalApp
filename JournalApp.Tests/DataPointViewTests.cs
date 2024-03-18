namespace JournalApp.Tests;

public class DataPointViewTests : JaTestContext
{
    [Fact]
    public void Mood()
    {
        var category = new DataPointCategory
        {
            Guid = Guid.NewGuid(),
            Type = PointType.Mood,
        };

        var day = Day.Create(new(2024, 01, 01));
        var point = DataPoint.Create(day, category);
        point.Mood = "🤩";

        var cut = RenderComponent<DataPointView>(p =>
            p.Add(x => x.Point, point)
        );

        cut.Find(".mud-menu-activator").Click();

        // TODO: Add a razor file with a popover provider so we can check for it.
        // Could we make a generic wrapper and pass the DataPointView as a ChildContent?
        // https://github.com/MudBlazor/MudBlazor/blob/1607b06f596bd1ca11bcbfe6c1c4b26e064f9551/src/MudBlazor.UnitTests.Viewer/TestComponents/Menu/MenuTest1.razor
        // cut.FindAll("div.mud-popover-open").Count.Should().Be(1);
    }

    [Fact]
    public void Sleep()
    {
        var category = new DataPointCategory
        {
            Guid = Guid.NewGuid(),
            Type = PointType.Sleep,
        };

        var day = Day.Create(new(2024, 01, 01));
        var point = DataPoint.Create(day, category);
        point.SleepHours = 8.5m;

        var cut = RenderComponent<DataPointView>(p =>
            p.Add(x => x.Point, point)
        );

        cut.Find(".sleep-hours").TextContent.Should().Be("08.5");

        for (var i = 0; i < 50; i++)
            cut.Find(".less-sleep").Click();

        point.SleepHours.Should().Be(0);
        cut.Find(".sleep-hours").TextContent.Should().Be("00.0");

        for (var i = 0; i < 50; i++)
            cut.Find(".more-sleep").Click();

        point.SleepHours.Should().Be(24);
        cut.Find(".sleep-hours").TextContent.Should().Be("24.0");
    }

    [Fact]
    public void Scale()
    {
        var category = new DataPointCategory
        {
            Guid = Guid.NewGuid(),
            Type = PointType.Scale,
        };

        var day = Day.Create(new(2024, 01, 01));
        var point = DataPoint.Create(day, category);

        var cut = RenderComponent<DataPointView>(p =>
            p.Add(x => x.Point, point)
        );

        point.ScaleIndex.Should().Be(null);
        cut.FindAll(".mud-rating-item").Count.Should().Be(5);

        cut.FindAll(".mud-rating-item")[2].Click();
        point.ScaleIndex.Should().Be(3);

        cut.FindAll(".mud-rating-item")[2].Click();
        point.ScaleIndex.Should().Be(null);
    }

    [Theory]
    [InlineData(PointType.LowToHigh)]
    [InlineData(PointType.MildToSevere)]
    public void ScaleEnums(PointType type)
    {
        var category = new DataPointCategory
        {
            Guid = Guid.NewGuid(),
            Type = type,
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

    [Fact]
    public void Number()
    {
        var category = new DataPointCategory
        {
            Guid = Guid.NewGuid(),
            Type = PointType.Number,
        };

        var day = Day.Create(new(2024, 01, 01));
        var point = DataPoint.Create(day, category);

        var cut = RenderComponent<DataPointView>(p =>
            p.Add(x => x.Point, point)
        );

        point.Number.Should().Be(null);
        cut.Find("input").Input("321");
        point.Number.Should().Be(321);
    }

    [Fact]
    public void Text()
    {
        var category = new DataPointCategory
        {
            Guid = Guid.NewGuid(),
            Type = PointType.Text,
        };

        var day = Day.Create(new(2024, 01, 01));
        var point = DataPoint.Create(day, category);

        var cut = RenderComponent<DataPointView>(p =>
            p.Add(x => x.Point, point)
        );

        point.Text.Should().Be(null);
        cut.Find("input").Input("321");
        point.Text.Should().Be("321");
    }

    [Fact]
    public void Note()
    {
        var category = new DataPointCategory
        {
            Guid = Guid.NewGuid(),
            Type = PointType.Note,
        };

        var day = Day.Create(new(2024, 01, 01));
        var point = DataPoint.Create(day, category);
        point.Text = "Hello world";

        var cut = RenderComponent<DataPointView>(p =>
            p.Add(x => x.Point, point)
        );

        cut.FindAll(".mud-link").Count.Should().Be(1);
        // TODO: Test note edit dialog.
        //cut.Find(".mud-link").Click();
    }

    [Fact]
    public void Medication()
    {
        var category = new DataPointCategory
        {
            Guid = Guid.NewGuid(),
            Type = PointType.Medication,
            MedicationDose = 1,
        };

        var day = Day.Create(new(2024, 01, 01));
        var point = DataPoint.Create(day, category);
        point.Text = "Hello world";

        var cut = RenderComponent<DataPointView>(p =>
            p.Add(x => x.Point, point)
        );

        cut.FindAll(".mud-link").Count.Should().Be(1);
        // TODO: Test dose edit dialog and make sure it makes the bool true.
        //cut.Find(".mud-link").Click();

        point.Bool.Should().Be(null);
        cut.FindAll(".mud-toggle-item").Count.Should().Be(2);
        cut.FindAll(".mud-toggle-item-selected-border").Count.Should().Be(0);

        // A changed dose should get reset to default when clicking No.
        point.MedicationDose--;

        cut.FindAll(".mud-toggle-item")[0].Click();
        cut.FindAll(".mud-toggle-item")[0].ClassList.Should().Contain("mud-toggle-item-selected-border");
        point.Bool.Should().Be(false);

        point.MedicationDose.Should().Be(point.Category.MedicationDose);

        cut.FindAll(".mud-toggle-item")[1].Click();
        cut.FindAll(".mud-toggle-item")[1].ClassList.Should().Contain("mud-toggle-item-selected-border");
        point.Bool.Should().Be(true);

        // A changed dose should get reset to default when unselecting.
        point.MedicationDose--;

        cut.FindAll(".mud-toggle-item")[1].Click();
        cut.FindAll(".mud-toggle-item")[1].ClassList.Should().NotContain("mud-toggle-item-selected-border");
        point.Bool.Should().Be(null);

        point.MedicationDose.Should().Be(point.Category.MedicationDose);
    }
}
