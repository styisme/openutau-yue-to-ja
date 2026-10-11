using OpenUtau.Plugin.Builtin;

namespace YueToJapanesePhonemizer.Tests;

public class JyutpingParseBasicTests {

    [Fact]
    public void Empty_ReturnsNull() {
        Assert.Null(Yue2JaCore.ParseJyutping(""));
        Assert.Null(Yue2JaCore.ParseJyutping("!!!"));
    }

    [Theory]
    [InlineData("nei", new[] { "n", "ei" })]
    [InlineData("hou", new[] { "h", "ou" })]
    [InlineData("zou", new[] { "z", "ou" })]
    [InlineData("dik", new[] { "d", "ik" })]
    [InlineData("jat", new[] { "j", "at" })]
    [InlineData("wan", new[] { "w", "an" })]
    public void SyllableWithInitial_SplitsIntoInitialAndFinal(string jyutping, string[] expected) {
        Assert.Equal(expected, Yue2JaCore.ParseJyutping(jyutping));
    }

    [Theory]
    [InlineData("aai", new[] { "aai" })]
    [InlineData("eoi", new[] { "eoi" })]
    [InlineData("oeng", new[] { "oeng" })]
    public void FinalOnly_ParsesAsFinal(string jyutping, string[] expected) {
        Assert.Equal(expected, Yue2JaCore.ParseJyutping(jyutping));
    }

    [Theory]
    [InlineData("ng", new[] { "ng" })]
    [InlineData("m", new[] { "m" })]
    public void NasalOnly_Parses(string jyutping, string[] expected) {
        Assert.Equal(expected, Yue2JaCore.ParseJyutping(jyutping));
    }

    [Fact]
    public void NonJyutpingWords_ReturnNull() {
        Assert.Null(Yue2JaCore.ParseJyutping("xyzzy"));
    }
}
