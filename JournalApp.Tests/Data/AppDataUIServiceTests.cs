using CommunityToolkit.Maui.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace JournalApp.Tests.Data;

public class AppDataUIServiceTests : JaTestContext
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        AddDbContext();
    }

    [Fact]
    public async Task StartImportWizard_DoesNotModifyDataOrPreferences_WhenUserCancelsConfirmation()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        var appDataService = Services.GetService<AppDataService>();
        var preferenceService = Services.GetService<PreferenceService>();

        var originalExportDate = new DateTimeOffset(2026, 3, 15, 12, 0, 0, TimeSpan.FromHours(-5));
        preferenceService.LastExportDate = originalExportDate;
        preferenceService.HideNotes = true;
        preferenceService.SelectedAppTheme = AppTheme.Dark;
        preferenceService.SafetyPlan = new SafetyPlan { WarningSigns = "stay safe" };

        appDbSeeder.SeedCategories();
        var originalDates = new DateOnly(2024, 1, 1).DatesTo(new(2024, 1, 3));
        appDbSeeder.SeedDays(originalDates);

        List<Guid> originalDayGuids;
        using (var db = await dbFactory.CreateDbContextAsync())
        {
            originalDayGuids = db.Days.Select(day => day.Guid).ToList();
        }

        var importPath = Path.Combine(Path.GetTempPath(), $"cancelled-import-{Guid.NewGuid()}.journalapp");
        var importBackup = CreateBackup(
            new DateOnly(2024, 2, 1),
            "Imported plan",
            "imported-palette");
        await importBackup.WriteArchive(importPath);

        var dialogService = new TestDialogService([null]);
        var service = new AppDataUIService(
            NullLogger<AppDataUIService>.Instance,
            appDataService,
            new TestFileSaver(),
            preferenceService);

        try
        {
            // Act
            var imported = await service.StartImportWizard(dialogService, importPath);

            // Assert
            imported.Should().BeFalse();

            using (var db = await dbFactory.CreateDbContextAsync())
            {
                db.Days.Select(day => day.Guid).Should().BeEquivalentTo(originalDayGuids);
                db.Days.Should().HaveCount(originalDates.Count());
            }

            preferenceService.LastExportDate.Should().Be(originalExportDate);
            preferenceService.HideNotes.Should().BeTrue();
            preferenceService.SelectedAppTheme.Should().Be(AppTheme.Dark);
            preferenceService.SafetyPlan.WarningSigns.Should().Be("stay safe");

            dialogService.Messages.Should().HaveCount(1);
            dialogService.Messages.Single().Should().Contain("This will replace ALL current data");
        }
        finally
        {
            File.Delete(importPath);
        }
    }

    [Fact]
    public async Task StartImportWizard_DoesNotRestorePreferencesOrUpdateLastExport_WhenReplaceFails()
    {
        // Arrange
        var dbFactory = Services.GetService<IDbContextFactory<AppDbContext>>();
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        var appDataService = Services.GetService<AppDataService>();
        var preferenceService = Services.GetService<PreferenceService>();

        var originalExportDate = new DateTimeOffset(2026, 3, 15, 12, 0, 0, TimeSpan.FromHours(-5));
        preferenceService.LastExportDate = originalExportDate;
        preferenceService.SafetyPlan = new SafetyPlan { WarningSigns = "original warning" };

        appDbSeeder.SeedCategories();
        var originalDates = new DateOnly(2024, 1, 1).DatesTo(new(2024, 1, 2));
        appDbSeeder.SeedDays(originalDates);

        List<Guid> originalDayGuids;
        using (var db = await dbFactory.CreateDbContextAsync())
        {
            originalDayGuids = db.Days.Select(day => day.Guid).ToList();
        }

        var duplicateGuid = Guid.NewGuid();
        var invalidBackup = new BackupFile
        {
            Days = [],
            Categories =
            [
                new() { Guid = duplicateGuid, Name = "Cat 1", Group = "Test", Type = PointType.Bool, Enabled = true },
                new() { Guid = duplicateGuid, Name = "Cat 2", Group = "Test", Type = PointType.Bool, Enabled = true }
            ],
            Points = [],
            PreferenceBackups =
            [
                new("safety_plan", """{"WarningSigns":["imported warning"]}""")
            ]
        };

        var importPath = Path.Combine(Path.GetTempPath(), $"failed-import-{Guid.NewGuid()}.journalapp");
        await invalidBackup.WriteArchive(importPath);

        var dialogService = new TestDialogService([true]);
        var service = new AppDataUIService(
            NullLogger<AppDataUIService>.Instance,
            appDataService,
            new TestFileSaver(),
            preferenceService);

        try
        {
            // Act
            var imported = await service.StartImportWizard(dialogService, importPath);

            // Assert
            imported.Should().BeFalse();

            using (var db = await dbFactory.CreateDbContextAsync())
            {
                db.Days.Select(day => day.Guid).Should().BeEquivalentTo(originalDayGuids);
                db.Days.Should().HaveCount(originalDates.Count());
                db.Points.Should().NotBeEmpty();
            }

            preferenceService.LastExportDate.Should().Be(originalExportDate);
            preferenceService.SafetyPlan.WarningSigns.Should().Be("original warning");
            dialogService.Messages.Should().Contain(message => message.StartsWith("Import failed:"));
        }
        finally
        {
            File.Delete(importPath);
        }
    }

    [Fact]
    public async Task StartExportWizard_UpdatesLastExportDateOnlyAfterSuccessfulSave()
    {
        // Arrange
        var appDbSeeder = Services.GetService<AppDbSeeder>();
        var appDataService = Services.GetService<AppDataService>();
        var preferenceService = Services.GetService<PreferenceService>();

        appDbSeeder.SeedCategories();
        appDbSeeder.SeedDays(new DateOnly(2024, 1, 1).DatesTo(new(2024, 1, 2)));

        var oldExportDate = new DateTimeOffset(2024, 1, 10, 12, 0, 0, TimeSpan.Zero);
        preferenceService.LastExportDate = oldExportDate;

        var fileSaver = new TestFileSaver
        {
            ResultFactory = (fileName, _) => new FileSaverResult(Path.Combine(Path.GetTempPath(), fileName), null)
        };
        var dialogService = new TestDialogService();
        var service = new AppDataUIService(
            NullLogger<AppDataUIService>.Instance,
            appDataService,
            fileSaver,
            preferenceService);

        // Act
        await service.StartExportWizard(dialogService);

        // Assert
        fileSaver.SaveCalls.Should().ContainSingle();
        fileSaver.SaveCalls.Single().FileName.Should().MatchRegex(@"^backup-\d{4}-\d{2}-\d{2}\.journalapp$");
        fileSaver.SaveCalls.Single().Bytes.Should().NotBeEmpty();
        preferenceService.LastExportDate.Should().BeAfter(oldExportDate);
        dialogService.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task StartExportWizard_DoesNotUpdateLastExportDate_WhenSaveFails()
    {
        // Arrange
        var appDataService = Services.GetService<AppDataService>();
        var preferenceService = Services.GetService<PreferenceService>();

        var oldExportDate = new DateTimeOffset(2024, 1, 10, 12, 0, 0, TimeSpan.Zero);
        preferenceService.LastExportDate = oldExportDate;

        var fileSaver = new TestFileSaver
        {
            ResultFactory = (_, _) => new FileSaverResult(null, new InvalidOperationException("disk full"))
        };
        var dialogService = new TestDialogService();
        var service = new AppDataUIService(
            NullLogger<AppDataUIService>.Instance,
            appDataService,
            fileSaver,
            preferenceService);

        // Act
        await service.StartExportWizard(dialogService);

        // Assert
        fileSaver.SaveCalls.Should().ContainSingle();
        preferenceService.LastExportDate.Should().Be(oldExportDate);
        dialogService.Messages.Should().ContainSingle()
            .Which.Should().Contain("Export failed: disk full");
    }

    private static BackupFile CreateBackup(DateOnly date, string safetyPlan, string moodPalette)
    {
        var day = Day.Create(date);
        var category = new DataPointCategory
        {
            Guid = Guid.NewGuid(),
            Group = "Mood",
            Name = "Mood",
            Index = 1,
            Type = PointType.Mood,
            Enabled = true,
        };
        var point = DataPoint.Create(day, category);
        point.Guid = Guid.NewGuid();
        point.Mood = "😀";

        day.Points.Add(point);
        category.Points.Add(point);

        return new BackupFile
        {
            Days = [day],
            Categories = [category],
            Points = [point],
            PreferenceBackups =
            [
                new("safety_plan", safetyPlan),
                new("mood_palette", moodPalette)
            ]
        };
    }

    private sealed class TestFileSaver : IFileSaver
    {
        public List<(string FileName, byte[] Bytes)> SaveCalls { get; } = [];

        public Func<string, byte[], FileSaverResult> ResultFactory { get; set; } = (fileName, _) =>
            new(Path.Combine(Path.GetTempPath(), fileName), null);

        public Task<FileSaverResult> SaveAsync(string initialPath, string fileName, Stream stream, CancellationToken cancellationToken = default) =>
            SaveAsync(fileName, stream, cancellationToken);

        public async Task<FileSaverResult> SaveAsync(string fileName, Stream stream, CancellationToken cancellationToken = default)
        {
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);
            var bytes = memoryStream.ToArray();
            SaveCalls.Add((fileName, bytes));
            return ResultFactory(fileName, bytes);
        }

        public Task<FileSaverResult> SaveAsync(string initialPath, string fileName, Stream stream, IProgress<double> progress, CancellationToken cancellationToken = default) =>
            SaveAsync(fileName, stream, cancellationToken);

        public Task<FileSaverResult> SaveAsync(string fileName, Stream stream, IProgress<double> progress, CancellationToken cancellationToken = default) =>
            SaveAsync(fileName, stream, cancellationToken);
    }

    private sealed class TestDialogService(params IReadOnlyCollection<bool?> responses) : IDialogService
    {
        private readonly Queue<bool?> _responses = new(responses);

        public List<string> Messages { get; } = [];

        public event Func<IDialogReference, Task> DialogInstanceAddedAsync
        {
            add { }
            remove { }
        }

        public event Action<IDialogReference, DialogResult> OnDialogCloseRequested
        {
            add { }
            remove { }
        }

        public Task<IDialogReference> ShowAsync<TComponent>() where TComponent : global::Microsoft.AspNetCore.Components.IComponent =>
            ShowAsync(typeof(TComponent), string.Empty, new DialogParameters(), null);

        public Task<IDialogReference> ShowAsync<TComponent>(string title) where TComponent : global::Microsoft.AspNetCore.Components.IComponent =>
            ShowAsync(typeof(TComponent), title, new DialogParameters(), null);

        public Task<IDialogReference> ShowAsync<TComponent>(string title, DialogOptions options) where TComponent : global::Microsoft.AspNetCore.Components.IComponent =>
            ShowAsync(typeof(TComponent), title, new DialogParameters(), options);

        public Task<IDialogReference> ShowAsync<TComponent>(string title, DialogParameters parameters) where TComponent : global::Microsoft.AspNetCore.Components.IComponent =>
            ShowAsync(typeof(TComponent), title, parameters, null);

        public Task<IDialogReference> ShowAsync<TComponent>(string title, DialogParameters parameters, DialogOptions options) where TComponent : global::Microsoft.AspNetCore.Components.IComponent =>
            ShowAsync(typeof(TComponent), title, parameters, options);

        public Task<IDialogReference> ShowAsync(Type component) =>
            ShowAsync(component, string.Empty, new DialogParameters(), null);

        public Task<IDialogReference> ShowAsync(Type component, string title) =>
            ShowAsync(component, title, new DialogParameters(), null);

        public Task<IDialogReference> ShowAsync(Type component, string title, DialogOptions options) =>
            ShowAsync(component, title, new DialogParameters(), options);

        public Task<IDialogReference> ShowAsync(Type component, string title, DialogParameters parameters) =>
            ShowAsync(component, title, parameters, null);

        public Task<IDialogReference> ShowAsync(Type component, string title, DialogParameters parameters, DialogOptions options)
        {
            var message = parameters.TryGet<string>(nameof(MessageBoxOptions.Message));

            if (!string.IsNullOrEmpty(message))
            {
                Messages.Add(message);
            }

            var reference = new DialogReference(Guid.NewGuid(), this);
            reference.InjectOptions(options ?? new DialogOptions());
            reference.Dismiss(GetNextResult());

            return Task.FromResult<IDialogReference>(reference);
        }

        public IDialogReference CreateReference() => new DialogReference(Guid.NewGuid(), this);

        public Task<bool?> ShowMessageBoxAsync(string title, string message, string yesText = "OK", string noText = null, string cancelText = null, DialogOptions options = null)
        {
            Messages.Add(message);
            var result = _responses.Count > 0 ? _responses.Dequeue() : null;
            return Task.FromResult(result);
        }

        public Task<bool?> ShowMessageBoxAsync(string title, global::Microsoft.AspNetCore.Components.MarkupString markupMessage, string yesText = "OK", string noText = null, string cancelText = null, DialogOptions options = null) =>
            ShowMessageBoxAsync(title, markupMessage.Value, yesText, noText, cancelText, options);

        public Task<bool?> ShowMessageBoxAsync(MessageBoxOptions messageBoxOptions, DialogOptions options = null) =>
            ShowMessageBoxAsync(messageBoxOptions.Title, messageBoxOptions.Message?.ToString() ?? string.Empty, messageBoxOptions.YesText, messageBoxOptions.NoText, messageBoxOptions.CancelText, options);

        public void Close(IDialogReference dialog)
        {
        }

        public void Close(IDialogReference dialog, DialogResult result)
        {
        }

        private DialogResult GetNextResult()
        {
            var next = _responses.Count > 0 ? _responses.Dequeue() : null;
            return next == true ? DialogResult.Ok(true) : DialogResult.Cancel();
        }
    }
}
