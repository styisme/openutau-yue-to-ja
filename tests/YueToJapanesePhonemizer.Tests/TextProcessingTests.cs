using OpenUtau.Plugin.Builtin;

namespace YueToJapanesePhonemizer.Tests;

public class TextProcessingTests {

    [Theory]
    [InlineData("你好，世界！", "你好 世界")]
    [InlineData("a.b;c!", "a b c")]
    [InlineData("（括号）", "括号")]
    [InlineData("純中文", "純中文")]
    public void ReplacePunctuation_MapsToSpacesAndTrims(string input, string expected) {
        Assert.Equal(expected, Yue2JaCore.ReplacePunctuation(input));
    }

    [Theory]
    [InlineData("5", "ng")]
    [InlineData("12", "sap ji")]
    [InlineData("20", "ji sap")]
    [InlineData("25", "ji sap ng")]
    [InlineData("2024", "ji ling ji sei")]
    public void ReplaceDigits_ConvertsNumberSequences(string input, string expected) {
        Assert.Equal(expected, Yue2JaCore.ReplaceDigits(input));
    }

    [Theory]
    [InlineData(new[] { 5 }, "ng")]
    [InlineData(new[] { 1, 2 }, "sap ji")]
    [InlineData(new[] { 2, 0 }, "ji sap")]
    [InlineData(new[] { 2, 5 }, "ji sap ng")]
    [InlineData(new[] { 1, 0, 0 }, "jat ling ling")]
    public void ConvertDigitSequence_HandlesRanges(int[] digits, string expected) {
        Assert.Equal(expected, Yue2JaCore.ConvertDigitSequence(digits.ToList()));
    }

    [Theory]
    [InlineData("こーぼー", "こぼ")]
    [InlineData("がー", "が")]
    [InlineData("无长音", "无长音")]
    public void StripLongMarks_RemovesChouonpu(string input, string expected) {
        Assert.Equal(expected, Yue2JaCore.StripLongMarks(input));
    }

    [Theory]
    [InlineData("nei5", "nei")]
    [InlineData("hou2", "hou")]
    [InlineData("noprefix", "noprefix")]
    public void StripDigits_RemovesAsciiDigits(string input, string expected) {
        Assert.Equal(expected, Yue2JaCore.StripDigits(input));
    }

    [Theory]
    [InlineData("こんにちは", true)]
    [InlineData("カタカナ", true)]
    [InlineData("nei hou", false)]
    [InlineData("zou", false)]
    public void ContainsJapaneseKana_DetectsKanaRanges(string input, bool expected) {
        Assert.Equal(expected, Yue2JaCore.ContainsJapaneseKana(input));
    }
}
