namespace JournalApp;

public static class MarkupUtil
{
    public static string ToClassName(string input)
    {
        var sb = new StringBuilder();
        var invalidChars = " ~!@$%^&*()_+-=,./’;:”?><[]{}|`#".ToCharArray();

        var lastCharWasInvalid = false;
        for (var i = 0; i < input.Length; i++)
        {
            if (invalidChars.Contains(input[i]))
            {
                if (lastCharWasInvalid)
                {
                    // Collapse invalid chars.
                }
                else
                {
                    lastCharWasInvalid = true;
                    sb.Append('-');
                }
            }
            else
            {
                lastCharWasInvalid = false;
                sb.Append(input[i]);
            }
        }

        return sb.ToString().Trim('-').ToLowerInvariant();
    }
}
