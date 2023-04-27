namespace JournalApp;

internal class FocusService
{
    public void ChangeFocus(JournalEntry entry = default)
    {
        FocusChanged?.Invoke(this, entry);
    }

    public event EventHandler<JournalEntry> FocusChanged;
}
