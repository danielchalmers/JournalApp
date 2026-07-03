using System.Runtime.CompilerServices;

// Allows the test project to exercise internal logic (e.g. TrendChartPoint, KeyEventService.OnBackButtonPressed)
// that has no public surface but carries real regression risk.
[assembly: InternalsVisibleTo("JournalApp.Tests")]
