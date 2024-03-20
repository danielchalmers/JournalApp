namespace JournalApp.Tests;

public class MarkupUtilTests
{
    [Theory]
    [InlineData("helloworld", "helloworld")]
    [InlineData("Hello World", "hello-world")]
    [InlineData("Hello World!", "hello-world")]
    [InlineData("Hello & World", "hello-world")]
    public void ToClassName(string actual, string expected)
    {
        MarkupUtil.ToClassName(actual).Should().Be(expected);
    }
}
