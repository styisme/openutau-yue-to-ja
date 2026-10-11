using OpenUtau.Plugin.Builtin;

namespace YueToJapanesePhonemizer.Tests;

public class KanaConversionTests {

    private static Dictionary<string, string[]> DefaultMap() {
        var map = new Dictionary<string, string[]>();
        foreach (var kv in Yue2JaCore.DefaultFullJyutpingMap) map[kv.Key] = kv.Value;
        return map;
    }

    [Theory]
    [InlineData("ka", "か")]
    [InlineData("shi", "し")]
    [InlineData("byo", "びょ")]
    [InlineData("n", "ん")]
    [InlineData("unknown", "unknown")]
    [InlineData("", "")]
    public void ToKana_TranslatesRomaji(string romaji, string expected) {
        Assert.Equal(expected, Yue2JaCore.ToKana(romaji));
    }

    [Fact]
    public void GenerateMoras_Nei_TwoMoras() {
        var moras = Yue2JaCore.GenerateMoras(new[] { "n" }, "ei", DefaultMap());
        Assert.Equal(new[] { "ね", "い" }, moras);
    }

    [Fact]
    public void GenerateMoras_Hou_TwoMoras() {
        var moras = Yue2JaCore.GenerateMoras(new[] { "h" }, "ou", DefaultMap());
        Assert.Equal(new[] { "ほ", "う" }, moras);
    }

    [Fact]
    public void GenerateMoras_Gwong_ThreeMoras() {
        var moras = Yue2JaCore.GenerateMoras(new[] { "gw" }, "ong", DefaultMap());
        Assert.Equal(new[] { "ぐ", "お", "ん" }, moras);
    }

    [Fact]
    public void GenerateMoras_Ngo_OneMora() {
        var moras = Yue2JaCore.GenerateMoras(new[] { "ng" }, "o", DefaultMap());
        Assert.Equal(new[] { "ご" }, moras);
    }

    [Fact]
    public void GenerateMoras_Hoeng_TwoMoras() {
        var moras = Yue2JaCore.GenerateMoras(new[] { "h" }, "oeng", DefaultMap());
        Assert.Equal(new[] { "へ", "ん" }, moras);
    }

    [Theory]
    [InlineData("", "-")]
    [InlineData("-", "-")]
    [InlineData("R", "R")]
    [InlineData("aa", "a")]
    [InlineData("aai", "i")]
    [InlineData("eoi", "i")]
    [InlineData("yu", "u")]
    [InlineData("oeng", "n")]
    public void ConvertToJapaneseVowel_ReturnsTrailingVowel(string jyutpingFinal, string expected) {
        Assert.Equal(expected, Yue2JaCore.ConvertToJapaneseVowel(jyutpingFinal));
    }

    [Theory]
    [InlineData("", "-")]
    [InlineData("あ", "a")]
    [InlineData("ん", "n")]
    [InlineData("きゃ", "a")]
    [InlineData("し", "i")]
    [InlineData("ふ", "u")]
    [InlineData("べ", "e")]
    [InlineData("と", "o")]
    [InlineData("！", "-")]
    public void GetLastJapaneseVowel_MapsKanaToVowel(string mora, string expected) {
        Assert.Equal(expected, Yue2JaCore.GetLastJapaneseVowel(mora));
    }
}
