using System.Diagnostics;

namespace JournalApp;

public partial class App : Application
{
    private readonly ApplicationDbContext _dbContext;

    public App(ApplicationDbContext dbContext)
    {
        InitializeComponent();
        _dbContext = dbContext;

        MainPage = new MainPage();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);
        if (window != null)
        {
            window.Deactivated += Window_Deactivated;
            window.Destroying += Window_Deactivated;
        }

        return window;
    }

    private async void Window_Deactivated(object sender, EventArgs e)
    {
        Debug.WriteLine("Window was deactivated or is being destroyed; Saving database...");
        await _dbContext.SaveChangesAsync();
    }
}
