namespace JournalApp.Tests;

public class MarkupUtilTests
{
    [Theory]
    [InlineData("helloworld", "helloworld")]
    [InlineData("helloworld", "Hello World!")]
    [InlineData("helloworld", "Hello & World")]
    public void ToClassName(string expected, string actual)
    {
        Assert.Equal(expected, MarkupUtil.ToClassName(actual));
    }
}
