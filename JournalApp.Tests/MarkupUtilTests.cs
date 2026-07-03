namespace JournalApp.Tests;

public class MarkupUtilTests
{
    [Theory]
    [InlineData("helloworld", "helloworld")]
    [InlineData("Hello World", "hello-world")]
    [InlineData("Hello World!", "hello-world")]
    [InlineData("Hello & World", "hello-world")]
    [InlineData("!Hello", "hello")] // leading invalid char is trimmed
    [InlineData("!@#$", "")]        // all-invalid collapses to one dash then trims to empty
    public void ToClassName(string actual, string expected)
    {
        MarkupUtil.ToClassName(actual).Should().Be(expected);
    }
}
