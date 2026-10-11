using OpenUtau.Plugin.Builtin;
using YamlDotNet.Serialization;

namespace YueToJapanesePhonemizer.Tests;

public class Yue2JaConfigTests {

    private static Yue2JaConfig? Deserialize(string yaml) {
        var deserializer = new DeserializerBuilder().Build();
        return deserializer.Deserialize<Yue2JaConfig>(yaml);
    }

    [Fact]
    public void Defaults_MatchDocumentedValues() {
        var cfg = Deserialize("nasal_mode: short");
        Assert.NotNull(cfg);
        Assert.Equal("short", cfg!.NasalMode);
        Assert.Equal(120, cfg.NasalMs);
        Assert.True(cfg.UseWildcard);
        Assert.False(cfg.DisableVcv);
        Assert.False(cfg.DisableLightTone);
        Assert.False(cfg.DisableToneTiming);
        Assert.False(cfg.DisableDoubleChar);
        Assert.False(cfg.AskPolyphone);
        Assert.Empty(cfg.FullJyutping);
        Assert.Empty(cfg.Polyphones);
        Assert.Empty(cfg.AliasOverrides);
    }

    [Fact]
    public void AllKeys_AreDeserialized() {
        var yaml = """
            nasal_mode: none
            nasal_ms: 250
            use_wildcard: false
            disable_vcv: true
            disable_light_tone: true
            disable_tone_timing: true
            disable_double_char: true
            ask_polyphone: true
            """;
        var cfg = Deserialize(yaml);
        Assert.NotNull(cfg);
        Assert.Equal("none", cfg!.NasalMode);
        Assert.Equal(250, cfg.NasalMs);
        Assert.False(cfg.UseWildcard);
        Assert.True(cfg.DisableVcv);
        Assert.True(cfg.DisableLightTone);
        Assert.True(cfg.DisableToneTiming);
        Assert.True(cfg.DisableDoubleChar);
        Assert.True(cfg.AskPolyphone);
    }

    [Fact]
    public void CustomJyutpingAndPolyphones_AreDeserialized() {
        var yaml = """
            full_jyutping:
              heoi: [fu, i]
              zyu: [ju]
            polyphones:
              "haang gai": "hang gai"
            """;
        var cfg = Deserialize(yaml);
        Assert.NotNull(cfg);
        Assert.Equal(new[] { "fu", "i" }, cfg!.FullJyutping["heoi"]);
        Assert.Equal(new[] { "ju" }, cfg.FullJyutping["zyu"]);
        Assert.Equal("hang gai", cfg.Polyphones["haang gai"]);
    }

    [Fact]
    public void AliasOverrides_AreDeserialized() {
        var yaml = """
            alias_overrides:
              "ん": ["ん -", "n -", "n"]
            """;
        var cfg = Deserialize(yaml);
        Assert.NotNull(cfg);
        Assert.Equal(new[] { "ん -", "n -", "n" }, cfg!.AliasOverrides["ん"]);
    }

    [Fact]
    public void EmptyDocument_ReturnsNull() {
        Assert.Null(Deserialize(""));
    }
}
