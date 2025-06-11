namespace JournalApp;

/// <summary>
/// Seeds the database with initial data and prepares it for use.
/// </summary>
public class AppDbSeeder(ILogger<AppDbSeeder> logger, IDbContextFactory<AppDbContext> dbFactory)
{
    /// <summary>
    /// Prepares the database by applying any pending migrations and ensuring it's ready to use.
    /// </summary>
    public void PrepareDatabase()
    {
        logger.LogInformation("Preparing database");
        using var db = dbFactory.CreateDbContext();
        var sw = Stopwatch.StartNew();

#if DEBUG && false
            // ⚠️ Dangerous: This block deletes the database and all preferences.
            // It's intentionally disabled and delayed to prevent accidental use.
            logger.LogCritical("ERASING DATABASE AND PREFERENCES");
            Thread.Sleep(5_000);
            Preferences.Clear();
            db.Database.EnsureDeleted();
#endif
        try
        {
            var anyPendingMigrations = db.Database.GetPendingMigrations().Any();
            logger.LogInformation($"Pending migrations: {anyPendingMigrations}");

            if (anyPendingMigrations)
            {
                logger.LogInformation($"Migrating database after {sw.ElapsedMilliseconds}ms");
                db.Database.Migrate();
                logger.LogInformation($"Migrated database after {sw.ElapsedMilliseconds}ms");
            }
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 14)
        {
            // Handles Sqlite "unable to open database file" error.
            // See: https://stackoverflow.com/a/38562947.
            logger.LogError(ex, "Sqlite Error Code 14");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration error");
            throw;
        }

        db.SaveChanges();
        logger.LogInformation($"Finished preparing database after {sw.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Seeds the database with predefined tracking categories.
    /// </summary>
    public void SeedCategories()
    {
        logger.LogInformation("Seeding categories");
        using var db = dbFactory.CreateDbContext();
        var sw = Stopwatch.StartNew();

        void AddOrUpdate(
            string guidString,
            PointType type,
            string group = null,
            string name = null,
            string details = null,
            bool readOnly = false,
            bool enabled = true,
            decimal? medDose = null,
            string medUnit = null,
            DateTimeOffset? medEveryDaySince = null)
        {
            // Use existing category or create a new one.
            var guid = new Guid(guidString);
            var category = db.Categories.SingleOrDefault(x => x.Guid == guid);
            var doesExist = category != null;
            category ??= new();

            // Always set fixed properties.
            category.Guid = guid;
            category.Group = group;
            category.ReadOnly = readOnly;

            // Set default enabled flag only if new.
            if (!doesExist)
            {
                category.Enabled = enabled;
            }

            // Set modifiable properties if it's new or marked read-only.
            if (!doesExist || readOnly)
            {
                category.Type = type;
                category.Name = name;
                category.Details = details;
                category.MedicationDose = medDose;
                category.MedicationEveryDaySince = medEveryDaySince;
                category.MedicationUnit = medUnit;
                category.Deleted = false;
            }

            if (doesExist)
            {
                logger.LogDebug($"Updating category <{guidString}>");
            }
            else
            {
                logger.LogDebug($"Adding category <{guidString}>");
                db.AddCategory(category);
            }

            db.SaveChanges();
        }

        AddOrUpdate(
            "BF394F35-2228-4933-BF38-AF5B1B97AEF7",
            PointType.Note,
            group: "Notes",
            readOnly: true);

        AddOrUpdate(
            "D90D89FB-F5B9-47CF-AE4E-3EC0D635E783",
            PointType.Mood,
            name: "Overall mood",
            details: "How I felt overall today",
            readOnly: true);

        AddOrUpdate(
            "D8657B36-F3A0-486F-BF80-0CF057919C7D",
            PointType.Sleep,
            name: "Last night's sleep",
            details: "Total hours of sleep I got last night");

        AddOrUpdate(
            "7330B995-0B56-46FF-9DD6-9CFC550FF5C8",
            PointType.MildToSevere,
            name: "Most depressed mood",
            details: "How low my mood felt today at its worst",
            enabled: false);

        AddOrUpdate(
            "4955EB49-0BCF-433B-873E-2092F292CC6B",
            PointType.MildToSevere,
            name: "Most elevated mood",
            details: "How high or energized I felt today at my peak",
            enabled: false);

        AddOrUpdate(
            "E9B7E4BE-FD17-4171-B1D4-D38B6009FDA0",
            PointType.MildToSevere,
            name: "Irritability",
            details: "How irritable or easily frustrated I felt today",
            enabled: false);

        AddOrUpdate(
            "0FB54AFF-9ECC-4C17-BAB5-B908B794CEA9",
            PointType.MildToSevere,
            name: "Anxiety",
            details: "How anxious or on edge I felt today",
            enabled: false);

        AddOrUpdate(
            "40B5AF7B-4F4E-4E77-BD6B-F7855CF773AB",
            PointType.LowToHigh,
            name: "Productivity",
            details: "How productive I felt today");

        AddOrUpdate(
            "DE394B38-9007-4349-AE31-429541AAB947",
            PointType.LowToHigh,
            name: "Physical activity",
            details: "How physically active I was today");

        AddOrUpdate(
            "14A348F7-139F-44A4-B032-1D11EBB63F90",
            PointType.LowToHigh,
            name: "Physical pain",
            details: "How much physical pain I felt today",
            enabled: false);

        AddOrUpdate(
            "480DC07D-1330-486F-9B30-EC83A3D4E6F0",
            PointType.Number,
            name: "Weight",
            details: "My current weight using my preferred units");

        AddOrUpdate(
            "2EEA42EE-4586-4E7D-ABF1-012BED1C0753",
            PointType.Medication,
            group: "Medications",
            name: "Example (AM)",
            details: "Just an example of a morning medication",
            enabled: true);

        AddOrUpdate(
            "4A00F59D-7A66-4CA5-B05A-F7D67F4BB6B5",
            PointType.Medication,
            group: "Medications",
            name: "Example (PM)",
            details: "Just an example of an evening medication",
            enabled: true);

        AddOrUpdate(
            "01A8F325-3002-40C4-B076-234E26172E82",
            PointType.Medication,
            group: "Medications",
            name: "Vitamin D",
            details: "Often taken for bone or immune support",
            enabled: false,
            medDose: 2000,
            medUnit: "IU",
            medEveryDaySince: DateTimeOffset.Now);

        AddOrUpdate(
            "545E8EBE-3C5C-4289-B4AD-E11CAC7B9324",
            PointType.Medication,
            group: "Medications",
            name: "Trazodone",
            details: "Commonly used for sleep or mood support",
            enabled: false,
            medDose: 50,
            medUnit: "mg",
            medEveryDaySince: DateTimeOffset.Now);

        AddOrUpdate(
            "A3AD777F-680A-481F-AFF2-E1CE6BCEA29E",
            PointType.Medication,
            group: "Medications",
            name: "Gabapentin",
            details: "Often taken for nerve pain, sleep, or seizures",
            enabled: false,
            medDose: 300,
            medUnit: "mg",
            medEveryDaySince: DateTimeOffset.Now);

        logger.LogInformation($"Seeded categories in {sw.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Seeds the database with debug data for a specific set of days.
    /// </summary>
    /// <param name="dates">The dates to populate with sample data.</param>
    public void SeedDays(IEnumerable<DateOnly> dates)
    {
        using var db = dbFactory.CreateDbContext();

        // Only seed new days — skip dates that already exist.
        var existingDates = db.Days.Where(d => dates.Contains(d.Date)).Select(d => d.Date).ToHashSet();
        var newDays = dates.Except(existingDates).Order().Select(Day.Create).ToList();

        if (newDays.Count == 0)
            return;

        var newPoints = new List<DataPoint>();

        foreach (var category in db.Categories)
        {
            void SeedDaysWithCategory()
            {
                var d = 0;
                while (true)
                {
                    // Simulate a stretch of similar data points to show trends.
                    var chunkLength = Random.Shared.Next(1, 6);
                    var seed = Guid.NewGuid().GetHashCode();

                    for (var i = 0; i < chunkLength; i++)
                    {
                        // Occasionally skip a day to simulate missed entries.
                        var fill = Random.Shared.Next(0, 10) > 0;

                        // Generate points for that day and category using the seed.
                        newPoints.AddRange(db.GetMissingPoints(newDays[d], category, fill ? new(seed) : null));

                        d++;
                        if (d == newDays.Count)
                            return;
                    }
                }
            }

            SeedDaysWithCategory();
        }

        db.Points.AddRange(newPoints);
        db.Days.AddRange(newDays);
        db.SaveChanges();
    }

    /// <summary>
    /// Seeds the database with debug data covering a wide date range.
    /// </summary>
    public void SeedDays()
    {
        var sw = Stopwatch.StartNew();
        var startDate = DateOnly.FromDateTime(DateTime.Now - TimeSpan.FromDays(365 * 5));
        var endDate = DateOnly.FromDateTime(DateTime.Now + TimeSpan.FromDays(7));
        var dates = new List<DateOnly>();

        dates.AddRange(startDate.DatesTo(endDate));

        // Add a few outlier months for edge-case testing (e.g. visualizations, reports).
        foreach (var relativeMonth in new int[] { -30, -36, -42, -48 })
            startDate.AddMonths(relativeMonth);

        logger.LogInformation($"Seeding up to {dates.Count} days");

        SeedDays(dates);

        logger.LogInformation($"Seeded days in {sw.ElapsedMilliseconds}ms");
    }
}
