using OpenUtau.Plugin.Builtin;

namespace YueToJapanesePhonemizer.Tests;

public class NumberReadingTests {

    [Theory]
    [InlineData("5", "ng")]
    [InlineData("8", "baat")]
    public void SingleDigit(string input, string expected) {
        Assert.Equal(expected, Yue2JaCore.ReplaceDigits(input));
    }

    [Theory]
    [InlineData("12", "sap ji")]
    [InlineData("20", "ji sap")]
    [InlineData("25", "ji sap ng")]
    [InlineData("50", "ng sap")]
    [InlineData("99", "gau sap gau")]
    public void TwoDigit(string input, string expected) {
        Assert.Equal(expected, Yue2JaCore.ReplaceDigits(input));
    }

    [Theory]
    [InlineData("100", "jat ling ling")]
    [InlineData("2024", "ji ling ji sei")]
    [InlineData("12345", "jat ji saam sei ng")]
    [InlineData("1,000", "jat ling ling ling")]
    public void LongDigitByDigit(string input, string expected) {
        Assert.Equal(expected, Yue2JaCore.ReplaceDigits(input));
    }

    [Theory]
    [InlineData("2024-01-15", "ji ling ji sei nin jat jyut sap ng jat")]
    [InlineData("2024/1/5", "ji ling ji sei nin jat jyut ng jat")]
    public void Date(string input, string expected) {
        Assert.Equal(expected, Yue2JaCore.ReplaceDigits(input));
    }

    [Theory]
    [InlineData("12:30", "sap ji dim saam sap fan")]
    [InlineData("8:00", "baat dim")]
    [InlineData("23:59", "ji sap saam dim ng sap gau fan")]
    public void Time(string input, string expected) {
        Assert.Equal(expected, Yue2JaCore.ReplaceDigits(input));
    }

    [Theory]
    [InlineData("3.14", "saam dim jat sei")]
    [InlineData("0.5", "ling dim ng")]
    public void Decimal(string input, string expected) {
        Assert.Equal(expected, Yue2JaCore.ReplaceDigits(input));
    }

    [Theory]
    [InlineData("50%", "baak fan zi ng sap")]
    [InlineData("100%", "baak fan zi jat baak")]
    public void Percent(string input, string expected) {
        Assert.Equal(expected, Yue2JaCore.ReplaceDigits(input));
    }

    [Theory]
    [InlineData("abc2024def", "abc ji ling ji sei def")]
    [InlineData("x 5 y", "x ng y")]
    public void MixedText(string input, string expected) {
        Assert.Equal(expected, Yue2JaCore.ReplaceDigits(input));
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("hello", "hello")]
    public void NoDigit(string input, string expected) {
        Assert.Equal(expected, Yue2JaCore.ReplaceDigits(input));
    }

    [Theory]
    [InlineData("0", "ling")]
    [InlineData("00", "ling ling")]
    public void Zero(string input, string expected) {
        Assert.Equal(expected, Yue2JaCore.ReplaceDigits(input));
    }
}
