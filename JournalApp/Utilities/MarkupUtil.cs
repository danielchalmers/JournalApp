namespace JournalApp;

public static class MarkupUtil
{
    public static string ToClassName(string input)
    {
        var sb = new StringBuilder();

        var chars = " ~!@$%^&*()_+-=,./’;:”?><[]{}|`#".ToCharArray();
        for (var i = 0; i < input.Length; i++)
        {
            if (!chars.Contains(input[i]))
                sb.Append(input[i]);
        }

        return sb.ToString().ToLowerInvariant();
    }
}
