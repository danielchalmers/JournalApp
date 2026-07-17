# 🙂 Journal App: Mood & Habits

**A private, open-source daily journal and mood tracker for Android.** Log your mood with an emoji, write about your day, and track habits, sleep, and medications. Everything stays on your device.

➡️ **[Get it free on the Play Store](https://play.google.com/store/apps/details?id=com.danielchalmers.journalapp)**

## Features

- 🙂 **Mood tracking** with expressive emojis
- 📝 **Daily notes** for journaling as much or as little as you like
- 💊 **Medication tracking** with doses
- ✅ **Custom categories** to track habits, sleep, or anything else
- 📅 **Mood calendar** that shows your month at a glance
- 📈 **Trends** to help you spot patterns over time
- 📚 **Guided worksheets** for mental health check-ins
- 🚨 **Safety Plan** with your coping strategies and support contacts
- 💾 **Local backups** you can export and import as a single file
- 🎨 **Material You colors** that match your wallpaper on Android 12+
- 🌙 **Light and dark themes**
- 📳 **Haptic feedback** that makes logging quick entries feel tactile

## Privacy

Your data never leaves your device. There is no account, no sign-up, and no cloud. See the [privacy policy](PRIVACY_POLICY.md) for details.

## Tech stack

- [.NET MAUI](https://learn.microsoft.com/dotnet/maui/) with [Blazor Hybrid](https://learn.microsoft.com/aspnet/core/blazor/hybrid/) for the UI
- [MudBlazor](https://mudblazor.com/) component library with custom theming that follows [Material 3 Expressive](https://m3.material.io/), including dynamic color generated from your device palette
- [EF Core](https://learn.microsoft.com/ef/core/) with SQLite for local storage
- [ApexCharts](https://apexcharts.github.io/Blazor-ApexCharts/) for trends

## Contributing

Bug reports and feature requests are welcome in the [issue tracker](https://github.com/danielchalmers/JournalApp/issues).

To build locally, install the .NET SDK with the MAUI workload and run the `JournalApp` project.
