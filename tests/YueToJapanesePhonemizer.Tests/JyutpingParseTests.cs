using OpenUtau.Plugin.Builtin;
using Xunit;

namespace YueToJapanesePhonemizer.Tests;

public class JyutpingParseTests {
    private static readonly string[] StandardSyllables = (
        // 常用字
        "nei hou zou san nin fai lok do ze " +
        // 数字
        "jat ji saam sei ng luk cat baat gau sap " +
        // 长 a 系列
        "baa baai baau baam baan baang baap baat baak " +
        "paa paai paau paam paan paang paap paat paak " +
        "maa maai maau maam maan maang maap maat maak " +
        "faa faai faau faam faan faang faap faat faak " +
        "daa daai daau daam daan daang daap daat daak " +
        "taa taai taau taam taan taang taap taat taak " +
        "naa naai naau naam naan naang naap naat naak " +
        "laa laai laau laam laan laang laap laat laak " +
        "gaa gaai gaau gaam gaan gaang gaap gaat gaak " +
        "kaa kaai kaau kaam kaan kaang kaap kaat kaak " +
        "haa haai haau haam haan haang haap haat haak " +
        "waa waai waan waang waat waak " +
        "zaa zaai zaau zaam zaan zaang zaap zaat zaak " +
        "caa caai caau caam caan caang caap caat caak " +
        "saa saai saau saam saan saang saap saat saak " +
        "jaa jaai jaau jaam jaan jaang jaap jaat jaak " +
        "gwai gwan gwang gwat gwok " +
        "kwai kwan kwang kwat kwok " +
        "nga ngai ngan ngang ngat ngak " +
        // 韵母独立（无辅音）
        "aai aau aam aan aang aap aat aak " +
        "ai au am an ang ap at ak " +
        "ei eu em en eng ep et ek " +
        "ing ik " +
        "o oi ong ot ok " +
        "ou " +
        "u un ung ut uk " +
        "ui " +
        "e eo eoi eon eot " +
        "oe oeng " +
        "yu yun yut " +
        "iu im in ip it " +
        "m ng"
    ).Split(' ', StringSplitOptions.RemoveEmptyEntries);

    [Fact]
    public void StandardSyllables_AllParseToNonNull() {
        var failed = StandardSyllables
            .Where(s => Yue2JaCore.ParseJyutping(s) == null)
            .ToList();
        Assert.True(failed.Count == 0,
            "ParseJyutping 对以下标准音节返回 null: " + string.Join(", ", failed));
    }

    [Theory]
    [InlineData("nei", new[] { "n", "ei" })]
    [InlineData("hou", new[] { "h", "ou" })]
    [InlineData("zou", new[] { "z", "ou" })]
    [InlineData("ngo", new[] { "ng", "o" })]
    [InlineData("gwai", new[] { "gw", "ai" })]
    [InlineData("kwong", new[] { "kw", "ong" })]
    [InlineData("zoeng", new[] { "z", "oeng" })]
    [InlineData("jyu", new[] { "j", "yu" })]
    public void InitialFinal_SplitCorrectly(string jyutping, string[] expected) {
        Assert.Equal(expected, Yue2JaCore.ParseJyutping(jyutping));
    }

    [Theory]
    [InlineData("m", new[] { "m" })]
    [InlineData("ng", new[] { "ng" })]
    public void NasalSyllables_Parse(string jyutping, string[] expected) {
        Assert.Equal(expected, Yue2JaCore.ParseJyutping(jyutping));
    }

    [Fact]
    public void NonJyutpingWords_ReturnNull() {
        Assert.Null(Yue2JaCore.ParseJyutping("xyzzy"));
        Assert.Null(Yue2JaCore.ParseJyutping("qqqq"));
    }
}