namespace JournalApp;

public class AppDbSeeder(ILogger<AppDbSeeder> logger, IDbContextFactory<AppDbContext> dbFactory)
{
    public void SeedDb()
    {
        logger.LogInformation("Seeding database");
        using var db = dbFactory.CreateDbContext();
        var sw = Stopwatch.StartNew();

        try
        {
#if DEBUG && false
            // So dangerous that it's kept in a block with a long delay instead of being added and removed as needed.
            logger.LogCritical("ERASING DATABASE AND PREFERENCES");
            Thread.Sleep(5_000);
            Preferences.Clear();
            db.Database.EnsureDeleted();
#endif
            db.Database.Migrate();
            db.SaveChanges();
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 14)
        {
            // https://stackoverflow.com/a/38562947.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration error");
            throw;
        }

        logger.LogInformation($"Migrated database in {sw.ElapsedMilliseconds}ms");
    }

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

            // Overwrite some properties that are supposed to be static.
            category.Guid = guid;
            category.Group = group;
            category.ReadOnly = readOnly;

            // Only set these properties if the category is new.
            if (!doesExist)
            {
                category.Enabled = enabled;
            }

            // Overwrite some flexible properties if it doesn't already exist OR is readonly and isn't allowed to change.
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

            // Add the new category.
            if (!doesExist)
                db.AddCategory(category);

            // Save all the changed properties or the new category itself.
            db.SaveChanges();
        }

        AddOrUpdate(
            "BF394F35-2228-4933-BF38-AF5B1B97AEF7",
            PointType.Note,
            group: "Notes",
            readOnly: true
        );

        AddOrUpdate(
            "D90D89FB-F5B9-47CF-AE4E-3EC0D635E783",
            PointType.Mood,
            name: "Overall mood",
            details: "My rating for the day",
            readOnly: true
        );

        AddOrUpdate(
            "D8657B36-F3A0-486F-BF80-0CF057919C7D",
            PointType.Sleep,
            name: "Last night's sleep",
            details: "The total amount of sleep I got last night"
        );

        AddOrUpdate(
            "7330B995-0B56-46FF-9DD6-9CFC550FF5C8",
            PointType.MildToSevere,
            name: "Most depressed mood",
            details: "My most severe level of depression today",
            enabled: false
        );

        AddOrUpdate(
            "4955EB49-0BCF-433B-873E-2092F292CC6B",
            PointType.MildToSevere,
            name: "Most elevated mood",
            details: "My most severe level of elevation today",
            enabled: false
        );

        AddOrUpdate(
            "E9B7E4BE-FD17-4171-B1D4-D38B6009FDA0",
            PointType.MildToSevere,
            name: "Irritability",
            enabled: false
        );

        AddOrUpdate(
            "0FB54AFF-9ECC-4C17-BAB5-B908B794CEA9",
            PointType.MildToSevere,
            name: "Anxiety",
            enabled: false
        );

        AddOrUpdate(
            "40B5AF7B-4F4E-4E77-BD6B-F7855CF773AB",
            PointType.LowToHigh,
            name: "Productivity"
        );

        AddOrUpdate(
            "DE394B38-9007-4349-AE31-429541AAB947",
            PointType.LowToHigh,
            name: "Physical activity"
        );

        AddOrUpdate(
            "14A348F7-139F-44A4-B032-1D11EBB63F90",
            PointType.LowToHigh,
            name: "Physical pain",
            details: "The amount of physical pain I experienced today",
            enabled: false
        );

        AddOrUpdate(
            "EE8DE4D0-3A87-4CA4-B384-81BD7508A19F",
            PointType.Bool,
            name: "Menstruating",
            enabled: false
        );

        AddOrUpdate(
            "C871C9F7-1A6E-4EA2-ACC9-94A256C9E2CC",
            PointType.Bool,
            name: "Therapy",
            details: "Did I participate in group or individual therapy?",
            enabled: false
        );

        AddOrUpdate(
            "480DC07D-1330-486F-9B30-EC83A3D4E6F0",
            PointType.Number,
            name: "Weight",
            details: "My weight in the measurement I prefer"
        );

        AddOrUpdate(
            "3EA2D087-9D4C-4110-9B96-0A52FDA6BFD2",
            PointType.Note,
            name: "Day summary",
            details: "A summary of the whole day's events including any notes that were written",
            enabled: false
        );

        AddOrUpdate(
            "2EEA42EE-4586-4E7D-ABF1-012BED1C0753",
            PointType.Medication,
            group: "Medications",
            name: "Example (AM)",
            details: "Shown as an example of an AM/morning dose",
            enabled: true,
            medDose: 1,
            medUnit: " unit");

        AddOrUpdate(
            "4A00F59D-7A66-4CA5-B05A-F7D67F4BB6B5",
            PointType.Medication,
            group: "Medications",
            name: "Example (PM)",
            details: "Shown as an example of a PM/evening dose",
            enabled: true,
            medDose: 1,
            medUnit: " unit");

        AddOrUpdate(
            "01A8F325-3002-40C4-B076-234E26172E82",
            PointType.Medication,
            group: "Medications",
            name: "Vitamin D",
            enabled: false,
            medDose: 2000,
            medUnit: "IU",
            medEveryDaySince: DateTimeOffset.Now);

        AddOrUpdate(
            "7DBB09E6-4A83-4E71-94AE-C40F3422DF09",
            PointType.Medication,
            group: "Medications",
            name: "Cetirizine",
            details: "A second-generation antihistamine primarily used to treat hay fever and other allergies.",
            enabled: false,
            medDose: 10,
            medUnit: "mg",
            medEveryDaySince: DateTimeOffset.Now);

        AddOrUpdate(
            "545E8EBE-3C5C-4289-B4AD-E11CAC7B9324",
            PointType.Medication,
            group: "Medications",
            name: "Trazadone",
            details: "An antidepressant primarily used to treat depression, anxiety, and insomnia",
            enabled: false,
            medDose: 50,
            medUnit: "mg",
            medEveryDaySince: DateTimeOffset.Now);

        AddOrUpdate(
            "A3AD777F-680A-481F-AFF2-E1CE6BCEA29E",
            PointType.Medication,
            group: "Medications",
            name: "Gabapentin",
            details: "An anticonvulsant primarily used to treat partial seizures and neuropathic pain",
            enabled: false,
            medDose: 300,
            medUnit: "mg",
            medEveryDaySince: DateTimeOffset.Now);

        logger.LogInformation($"Seeded categories in {sw.ElapsedMilliseconds}ms");
    }

    public void SeedDays(IEnumerable<DateOnly> dates)
    {
        using var db = dbFactory.CreateDbContext();

        // Select and create only new days.
        var existingDates = db.Days.Where(d => dates.Contains(d.Date)).Select(d => d.Date).ToHashSet();
        var newDays = dates.Except(existingDates).Order().Select(Day.Create).ToList();

        if (newDays.Count == 0)
            return; // We only want to seed new days but there aren't any.

        var newPoints = new List<DataPoint>();

        foreach (var category in db.Categories)
        {
            void SeedDaysWithCategory()
            {
                var d = 0;
                while (true)
                {
                    // Use the same seed over a random number of days to represent trends.
                    var chunkLength = Random.Shared.Next(1, 6);
                    var seed = Guid.NewGuid().GetHashCode();

                    for (var i = 0; i < chunkLength; i++)
                    {
                        // Sometimes don't even generate the day as if the user forgot.
                        var fill = Random.Shared.Next(0, 10) > 0;

                        // Generate missing points with the random seed.
                        newPoints.AddRange(db.GetMissingPoints(newDays[d], category, fill ? new(seed) : null));

                        // Increment and end when we've gone through all the days.
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

    public void SeedDays()
    {
        logger.LogInformation("Seeding days");
        var sw = Stopwatch.StartNew();
        var startDate = DateOnly.FromDateTime(DateTime.Now - TimeSpan.FromDays(365 * 5));
        var endDate = DateOnly.FromDateTime(DateTime.Now + TimeSpan.FromDays(7));
        var dates = new List<DateOnly>();

        dates.AddRange(startDate.DatesTo(endDate));

        // A few additional days to test multi-year features.
        foreach (var relativeMonth in new int[] { -30, -36, -42, -48 })
            startDate.AddMonths(relativeMonth);

        SeedDays(dates);

        logger.LogInformation($"Seeded days in {sw.ElapsedMilliseconds}ms");
    }
}
