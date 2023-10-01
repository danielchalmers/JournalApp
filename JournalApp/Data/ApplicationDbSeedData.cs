﻿namespace JournalApp;

public static class ApplicationDbSeedData
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        var sw = Stopwatch.StartNew();
        bool? databaseWasCreated = null;
        try
        {
#if DEBUG
            //db.Database.EnsureDeleted();
#endif
            databaseWasCreated = db.Database.EnsureCreated();
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 14)
        {
            // https://stackoverflow.com/a/38562947.
        }

        sw.Stop();
        Debug.WriteLine($"Ensured database was created in {sw.ElapsedMilliseconds}ms; Was created: {databaseWasCreated}");

        sw.Restart();
        SeedCategories(db);
        await db.SaveChangesAsync();
        sw.Stop();
        Debug.WriteLine($"Seeded categories in {sw.ElapsedMilliseconds}ms");

#if DEBUG
        sw.Restart();
        await SeedDays(db);
        await db.SaveChangesAsync();
        sw.Stop();
        Debug.WriteLine($"Seeded days in {sw.ElapsedMilliseconds}ms");
#endif
    }

    private static void SeedCategories(ApplicationDbContext db)
    {
        var existingCategoryGuids = db.Categories.Select(c => c.Guid).ToHashSet();
        var newCategories = new HashSet<DataPointCategory>();

        void TryAdd(DataPointCategory newCategory)
        {
            if (!existingCategoryGuids.Contains(newCategory.Guid))
                newCategories.Add(newCategory);
        }

        TryAdd(new()
        {
            Guid = new("D90D89FB-F5B9-47CF-AE4E-3EC0D635E783"),
            Name = "Overall mood",
            Type = DataType.Mood,
            Index = 1,
            ReadOnly = true,
        });
        TryAdd(new()
        {
            Guid = new("D8657B36-F3A0-486F-BF80-0CF057919C7D"),
            Name = "Last night's sleep",
            Type = DataType.Sleep,
            Index = 2,
            ReadOnly = true,
        });
        TryAdd(new()
        {
            Guid = new("7330B995-0B56-46FF-9DD6-9CFC550FF5C8"),
            Name = "Most depressed mood",
            Type = DataType.MildToSevere,
            Index = 3,
            ReadOnly = true,
            Enabled = false,
        });
        TryAdd(new()
        {
            Guid = new("4955EB49-0BCF-433B-873E-2092F292CC6B"),
            Name = "Most elevated mood",
            Type = DataType.MildToSevere,
            Index = 4,
            ReadOnly = true,
            Enabled = false,
        });
        TryAdd(new()
        {
            Guid = new("E9B7E4BE-FD17-4171-B1D4-D38B6009FDA0"),
            Name = "Irritability",
            Type = DataType.MildToSevere,
            Index = 5,
            ReadOnly = true,
            Enabled = false,
        });
        TryAdd(new()
        {
            Guid = new("0FB54AFF-9ECC-4C17-BAB5-B908B794CEA9"),
            Name = "Anxiety",
            Type = DataType.MildToSevere,
            Index = 6,
            ReadOnly = true,
            Enabled = false,
        });
        TryAdd(new()
        {
            Guid = new("40B5AF7B-4F4E-4E77-BD6B-F7855CF773AB"),
            Name = "Productivity",
            Type = DataType.LowToHigh,
            Index = 7,
            ReadOnly = true,
        });
        TryAdd(new()
        {
            Guid = new("DE394B38-9007-4349-AE31-429541AAB947"),
            Name = "Exercised or was active",
            Type = DataType.Bool,
            Index = 8,
            ReadOnly = true,
        });
        TryAdd(new()
        {
            Guid = new("EE8DE4D0-3A87-4CA4-B384-81BD7508A19F"),
            Name = "Menstruating",
            Type = DataType.Bool,
            Index = 9,
            ReadOnly = true,
            Enabled = false,
        });
        TryAdd(new()
        {
            Guid = new("C871C9F7-1A6E-4EA2-ACC9-94A256C9E2CC"),
            Name = "Did therapy today",
            Type = DataType.Bool,
            Index = 10,
            ReadOnly = true,
            Enabled = false,
        });
        TryAdd(new()
        {
            Guid = new("480DC07D-1330-486F-9B30-EC83A3D4E6F0"),
            Name = "Weight",
            Type = DataType.Number,
            Index = 11,
            ReadOnly = true,
        });
        TryAdd(new()
        {
            Guid = new("BF394F35-2228-4933-BF38-AF5B1B97AEF7"),
            Group = "Notes",
            Type = DataType.Note,
            ReadOnly = true,
        });
        TryAdd(new()
        {
            Guid = new("01A8F325-3002-40C4-B076-234E26172E82"),
            Group = "Medications",
            Name = "Vitamin D",
            Type = DataType.Medication,
            Index = 1,
            MedicationDose = 2000,
            MedicationUnit = "IU",
            MedicationEveryDaySince = DateTimeOffset.Now,
            ReadOnly = true,
            Enabled = false,
        });

        if (newCategories.Count > 0)
            db.Categories.AddRange(newCategories);
    }

    private static async Task SeedDays(ApplicationDbContext db)
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now - TimeSpan.FromDays(120));
        var endDate = DateOnly.FromDateTime(DateTime.Now + TimeSpan.FromDays(7));

        foreach (var dates in startDate.DatesTo(endDate).Chunk(3))
        {
            // We want the same values over a batch of days to represent trends.
            var seed = Guid.NewGuid().GetHashCode();
            foreach (var date in dates)
            {
                // Sometimes don't even generate the day as if the user forgot.
                var fillDay = Random.Shared.Next(0, 10) > 0;

                await db.GetOrCreateDay(date, false, fillDay ? new(seed) : null);
            }
        }

        // A few additional days to test multi-year features.
        foreach (var relativeMonth in new int[] { -12, -18, -24, -30, -36, -42, -48 })
        {
            await db.GetOrCreateDay(startDate.AddMonths(relativeMonth), false, Random.Shared);
        }
    }
}
