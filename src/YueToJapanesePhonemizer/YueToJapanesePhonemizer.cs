// =====================================================================
// YueToJapanesePhonemizer v1.0.0
// 让日语音源唱粤语歌词的 OpenUtau 音素器
//
// 依赖：OpenUtau v0.1.570+ / .NET 10 / YamlDotNet / Serilog
// =====================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using OpenUtau.Api;
using OpenUtau.Core.Ustx;
using Serilog;
using YamlDotNet.Serialization;

namespace OpenUtau.Plugin.Builtin {

    public class Yue2JaConfig {
        [YamlMember(Alias = "full_jyutping")]
        public Dictionary<string, List<string>> FullJyutping { get; set; } = new();

        [YamlMember(Alias = "polyphones")]
        public Dictionary<string, string> Polyphones { get; set; } = new();

        [YamlMember(Alias = "alias_overrides")]
        public Dictionary<string, List<string>> AliasOverrides { get; set; } = new();

        [YamlMember(Alias = "ask_polyphone")]
        public bool AskPolyphone { get; set; } = false;

        [YamlMember(Alias = "polyphone_decisions")]
        public Dictionary<string, bool> PolyphoneDecisions { get; set; } = new();

        [YamlMember(Alias = "disable_light_tone")]
        public bool DisableLightTone { get; set; } = false;

        [YamlMember(Alias = "disable_tone_timing")]
        public bool DisableToneTiming { get; set; } = false;

        [YamlMember(Alias = "disable_double_char")]
        public bool DisableDoubleChar { get; set; } = false;

        [YamlMember(Alias = "disable_vcv")]
        public bool DisableVcv { get; set; } = false;

        [YamlMember(Alias = "nasal_mode")]
        public string NasalMode { get; set; } = "short";

        [YamlMember(Alias = "nasal_ms")]
        public int NasalMs { get; set; } = 120;

        [YamlMember(Alias = "use_wildcard")]
        public bool UseWildcard { get; set; } = true;
    }

    [Phonemizer("Cantonese to Japanese Phonemizer", "YUE to JA", "Deepseek", language: "ZH")]
    public class YueToJapanesePhonemizer : SyllableBasedPhonemizer {

        private readonly Dictionary<string, string[]> fullJyutpingMap = new();
        private readonly Dictionary<string, string> polyphoneMap = new();
        private readonly Dictionary<string, List<string>> aliasOverrides = new();
        private readonly Dictionary<(string, int), bool> otoCache = new();
        private readonly Dictionary<int, int> noteTones = new();
        private readonly Dictionary<int, bool> lightToneFlags = new();
        private readonly Dictionary<string, bool> polyphoneDecisions = new();

        private bool askPolyphone = false;
        private bool disableLightTone = false;
        private bool disableToneTiming = false;
        private bool disableDoubleChar = false;
        private bool disableVcv = false;
        private string nasalMode = "short";
        private int nasalMs = 120;
        private bool useWildcard = true;

        private USinger currentSinger;

        public YueToJapanesePhonemizer() {
            this.vowels = Yue2JaCore.Finals;
            this.consonants = Yue2JaCore.Initials;
            foreach (var kv in Yue2JaCore.DefaultFullJyutpingMap) fullJyutpingMap[kv.Key] = kv.Value;
            foreach (var kv in Yue2JaCore.DefaultPolyphoneMap) polyphoneMap[kv.Key] = kv.Value;
        }

        protected override string[] GetVowels() => Yue2JaCore.Finals;
        protected override string[] GetConsonants() => Yue2JaCore.Initials;
        protected override string GetDictionaryName() => null;

        public override void SetSinger(USinger singer) {
            base.SetSinger(singer);
            this.currentSinger = singer;
            if (singer == null || !singer.Loaded) return;

            Log.Information("[YUE to JA v1.0.0] Singer set");

            TryLoadConfig(Path.Combine(PluginDir, "yue2ja.yaml"));
            TryLoadConfig(Path.Combine(singer.Location, "yue2ja.yaml"));
            otoCache.Clear();
        }

        private void TryLoadConfig(string path) {
            if (!File.Exists(path)) return;
            try {
                var text = File.ReadAllText(path);
                var cfg = Core.Yaml.DefaultDeserializer.Deserialize<Yue2JaConfig>(text);
                if (cfg == null) return;

                if (cfg.FullJyutping != null)
                    foreach (var kv in cfg.FullJyutping)
                        if (kv.Value != null) fullJyutpingMap[kv.Key] = kv.Value.ToArray();

                if (cfg.Polyphones != null)
                    foreach (var kv in cfg.Polyphones) polyphoneMap[kv.Key] = kv.Value;

                if (cfg.AliasOverrides != null)
                    foreach (var kv in cfg.AliasOverrides)
                        if (kv.Value != null) aliasOverrides[kv.Key] = kv.Value;

                if (cfg.PolyphoneDecisions != null)
                    foreach (var kv in cfg.PolyphoneDecisions) polyphoneDecisions[kv.Key] = kv.Value;

                askPolyphone = cfg.AskPolyphone;
                disableLightTone = cfg.DisableLightTone;
                disableToneTiming = cfg.DisableToneTiming;
                disableDoubleChar = cfg.DisableDoubleChar;
                disableVcv = cfg.DisableVcv;
                nasalMode = cfg.NasalMode ?? "short";
                nasalMs = cfg.NasalMs;
                if (nasalMs < 30) nasalMs = 30;
                if (nasalMs > 500) nasalMs = 500;
                useWildcard = cfg.UseWildcard;

                Log.Information($"[YUE to JA] Loaded config: {path} (nasal={nasalMode}, ms={nasalMs}, wildcard={useWildcard})");
            } catch (Exception e) {
                Log.Error(e, $"Failed to load yue2ja config: {path}");
            }
        }

        public override void SetUp(Note[][] groups, UProject project, UTrack track) {
            noteTones.Clear();
            lightToneFlags.Clear();
            PreprocessNotes(groups);
            BaseChinesePhonemizer.RomanizeNotes(groups);
            FixPolyphones(groups);
            base.SetUp(groups, project, track);
        }

        private void PreprocessNotes(Note[][] groups) {
            foreach (var group in groups) {
                if (group == null || group.Length == 0) continue;
                var note = group[0];
                string lyric = note.lyric;
                if (string.IsNullOrEmpty(lyric)) continue;

                string trimmed = lyric.Trim();
                if (trimmed == "-" || trimmed == "R" || trimmed == "+") continue;

                lyric = Yue2JaCore.ReplacePunctuation(lyric);
                var (baseLyric, tone) = ParseJyutpingToneMarks(lyric);
                lyric = baseLyric;

                if (tone >= 1 && tone <= 6) noteTones[note.position] = tone;

                lyric = Yue2JaCore.ReplaceDigits(lyric);
                lyric = Yue2JaCore.StripLongMarks(lyric);

                group[0] = new Note {
                    lyric = lyric,
                    phoneticHint = note.phoneticHint,
                    tone = note.tone,
                    position = note.position,
                    duration = note.duration,
                    phonemeAttributes = note.phonemeAttributes,
                };
            }
        }

        // 粤拼声调：尾部数字 1–6
        private static (string, int) ParseJyutpingToneMarks(string s) {
            int tone = 0;
            string result = s;
            if (result.Length >= 2) {
                char last = result[^1];
                char prev = result[^2];
                if (char.IsDigit(last) && char.IsLetter(prev)) {
                    int t = last - '0';
                    if (t >= 1 && t <= 6) { tone = t; result = result[..^1]; }
                }
            }
            return (result, tone);
        }

        private void FixPolyphones(Note[][] groups) {
            var lyrics = new List<string>();
            var refs = new List<(int gi, int ni)>();

            for (int i = 0; i < groups.Length; i++) {
                if (groups[i].Length == 0) continue;
                var note = groups[i][0];
                lyrics.Add(string.IsNullOrEmpty(note.phoneticHint) ? note.lyric.ToLowerInvariant() : "__skip__");
                refs.Add((i, 0));
            }

            for (int i = 0; i < lyrics.Count; i++) {
                if (lyrics[i] == "__skip__") continue;
                for (int len = Math.Min(4, lyrics.Count - i); len >= 2; len--) {
                    bool hasSkip = false;
                    for (int j = 0; j < len; j++)
                        if (lyrics[i + j] == "__skip__") { hasSkip = true; break; }
                    if (hasSkip) continue;

                    string pattern = string.Join(" ", lyrics.Skip(i).Take(len));
                    if (!polyphoneMap.TryGetValue(pattern, out var replacement)) continue;

                    if (!polyphoneDecisions.TryGetValue(pattern, out bool useCorrection)) {
                        useCorrection = true;
                        polyphoneDecisions[pattern] = true;
                    }
                    if (!useCorrection) continue;

                    var parts = replacement.Split(' ');
                    if (parts.Length != len) break;
                    for (int j = 0; j < len; j++) {
                        var (gi, ni) = refs[i + j];
                        var oldNote = groups[gi][ni];
                        groups[gi][ni] = new Note {
                            lyric = parts[j],
                            phoneticHint = oldNote.phoneticHint,
                            tone = oldNote.tone,
                            position = oldNote.position,
                            duration = oldNote.duration,
                            phonemeAttributes = oldNote.phonemeAttributes,
                        };
                        lyrics[i + j] = parts[j];
                    }
                    i += len - 1;
                    break;
                }
            }

            if (!disableDoubleChar) {
                for (int i = 0; i < lyrics.Count - 1; i++) {
                    if (lyrics[i] == "__skip__" || string.IsNullOrEmpty(lyrics[i])) continue;
                    if (lyrics[i] == lyrics[i + 1]) {
                        var (gi, ni) = refs[i + 1];
                        lightToneFlags[groups[gi][ni].position] = true;
                    }
                }
            }
        }

        public override Result Process(Note[] notes, Note? prev, Note? next,
                                       Note? prevNeighbour, Note? nextNeighbour,
                                       Note[] prevNeighbours) {
            if (string.IsNullOrEmpty(notes[0].lyric))
                return new Result { phonemes = new Phoneme[0] };

            string rawLyric = notes[0].lyric;
            if (!string.IsNullOrEmpty(notes[0].phoneticHint))
                rawLyric = notes[0].phoneticHint;

            var parts = rawLyric.Split(new[] { ' ', ',', '，', '、', '/', '|' },
                StringSplitOptions.RemoveEmptyEntries);

            Result result;
            if (parts.Length > 1)
                result = ProcessMultipleSyllables(parts, notes, prevNeighbour);
            else
                result = base.Process(notes, prev, next, prevNeighbour, nextNeighbour, prevNeighbours);

            ShortenNasalTail(result, notes, next.HasValue);

            int notePos = notes[0].position;
            if (!disableLightTone && lightToneFlags.TryGetValue(notePos, out bool isLight) && isLight)
                ApplyLightTone(result, notes);

            if (!disableToneTiming && noteTones.TryGetValue(notePos, out int tone) && tone >= 1 && tone <= 6)
                ApplyToneTiming(result, notes, tone);

            return result;
        }

        private void ApplyLightTone(Result result, Note[] notes) {
            if (result.phonemes == null || result.phonemes.Length < 2) return;
            int totalDuration = notes.Sum(n => n.duration);
            int last = result.phonemes.Length - 1;
            int lastPos = result.phonemes[last].position;
            int currentDur = totalDuration - lastPos;
            int newDur = Math.Max(25, (int)(currentDur * 0.6));
            int newPos = totalDuration - newDur;
            if (newPos > 0 && newPos < totalDuration) {
                var p = result.phonemes[last];
                p.position = newPos;
                result.phonemes[last] = p;
            }
        }

        private void ApplyToneTiming(Result result, Note[] notes, int tone) {
            if (result.phonemes == null || result.phonemes.Length < 2) return;
            int totalDuration = notes.Sum(n => n.duration);
            int last = result.phonemes.Length - 1;

            // 粤语六声调时长微调
            float ratio = tone switch {
                1 => 1.00f, 2 => 0.95f, 3 => 1.00f,
                4 => 0.95f, 5 => 1.05f, 6 => 0.90f,
                _ => 1.00f,
            };

            int lastPos = result.phonemes[last].position;
            int currentDur = totalDuration - lastPos;
            int newDur = Math.Max(25, (int)(currentDur * ratio));
            int newPos = totalDuration - newDur;
            if (newPos > 0 && newPos < totalDuration) {
                var p = result.phonemes[last];
                p.position = newPos;
                result.phonemes[last] = p;
            }
        }

        private void ShortenNasalTail(Result result, Note[] notes, bool hasNext) {
            if (result.phonemes == null || result.phonemes.Length == 0) return;
            if (nasalMode == "full") return;
            int totalDuration = notes.Sum(n => n.duration);
            if (totalDuration <= 0) return;

            for (int i = result.phonemes.Length - 1; i >= 0; i--) {
                var p = result.phonemes[i];
                if (string.IsNullOrEmpty(p.phoneme)) continue;
                string ph = p.phoneme.ToLowerInvariant();
                bool isNasalTail = ph == "ん" || ph == "ン" || ph == "n" ||
                                   ph.EndsWith("ん") || ph.EndsWith("ン");
                if (!isNasalTail) break;
                if (result.phonemes.Length == 1) break;

                if (nasalMode == "none") {
                    var np = p;
                    np.position = totalDuration;
                    result.phonemes[i] = np;
                    continue;
                }
                int targetMs = hasNext ? (int)(nasalMs * 1.5) : nasalMs;
                int targetStart = totalDuration - targetMs;
                int minStart = totalDuration / 2;
                if (targetStart < minStart) targetStart = minStart;
                if (targetStart <= p.position) continue;
                var np2 = p;
                np2.position = targetStart;
                result.phonemes[i] = np2;
            }
        }

        protected override string[] GetSymbols(Note note) {
            if (!string.IsNullOrEmpty(note.phoneticHint))
                return note.phoneticHint.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string lyric = note.lyric;
            if (string.IsNullOrEmpty(lyric)) return null;
            if (lyric == "-" || lyric == "R" || lyric == "+") return new[] { lyric };

            if (Yue2JaCore.ContainsJapaneseKana(lyric))
                return lyric.Split(new[] { ' ', ',', '，', '、' }, StringSplitOptions.RemoveEmptyEntries);

            return Yue2JaCore.ParseJyutping(lyric);
        }

        protected override List<string> ProcessSyllable(Syllable syllable) {
            var phonemes = new List<string>();
            if (string.IsNullOrEmpty(syllable.v)) return phonemes;

            var moras = Yue2JaCore.GenerateMoras(syllable.cc, syllable.v, fullJyutpingMap);
            if (moras.Count == 0) return phonemes;

            string prevJaVowel = Yue2JaCore.ConvertToJapaneseVowel(syllable.prevV);
            string prevMora = null;

            for (int i = 0; i < moras.Count; i++) {
                string mora = moras[i];
                string alias = null;

                if (useWildcard && !disableVcv) {
                    if (i == 0) {
                        if (!string.IsNullOrEmpty(prevJaVowel) && prevJaVowel != "-") {
                            alias = PickAlias(syllable.vowelTone,
                                $"{prevJaVowel} * {mora}", $"{prevJaVowel} *{mora}",
                                $"{prevJaVowel}* {mora}", $"{prevJaVowel}*{mora}",
                                $"{prevJaVowel} {mora}", $"{prevJaVowel}{mora}",
                                $"{prevJaVowel} *", $"* {mora}", mora);
                        }
                    } else if (!string.IsNullOrEmpty(prevMora)) {
                        string prevVowel = Yue2JaCore.GetLastJapaneseVowel(prevMora);
                        if (!string.IsNullOrEmpty(prevVowel) && prevVowel != "-") {
                            alias = PickAlias(syllable.vowelTone,
                                $"{prevVowel} * {mora}", $"{prevVowel} *{mora}",
                                $"{prevVowel}* {mora}", $"{prevVowel}*{mora}",
                                $"{prevVowel} {mora}", $"{prevVowel}{mora}",
                                $"{prevVowel} *", $"* {mora}", mora);
                        }
                    }
                }

                if (string.IsNullOrEmpty(alias) || alias == mora) {
                    if (i == 0) {
                        if (disableVcv || string.IsNullOrEmpty(prevJaVowel) || prevJaVowel == "-")
                            alias = PickAlias(syllable.vowelTone, $"- {mora}", $"-{mora}", mora);
                        else
                            alias = PickAlias(syllable.vowelTone, $"{prevJaVowel} {mora}", $"{prevJaVowel}{mora}", mora);
                    } else {
                        alias = PickAlias(syllable.vowelTone, mora);
                    }
                }
                phonemes.Add(alias);
                prevMora = mora;
            }
            return phonemes;
        }

        protected override List<string> ProcessEnding(Ending ending) {
            var phonemes = new List<string>();
            string prevV = Yue2JaCore.ConvertToJapaneseVowel(ending.prevV);
            if (string.IsNullOrEmpty(prevV)) return phonemes;

            if (ending.cc != null && ending.cc.Length > 0) {
                foreach (var c in ending.cc) {
                    if (string.IsNullOrEmpty(c)) continue;
                    string cons = Yue2JaCore.InitialMap.TryGetValue(c, out var jc) ? jc : c;
                    if (HasOtoCached($"{prevV} {cons}", ending.tone)) phonemes.Add($"{prevV} {cons}");
                    else if (HasOtoCached($"{prevV}{cons}", ending.tone)) phonemes.Add($"{prevV}{cons}");
                }
            }

            if (phonemes.Count == 0) {
                if (HasOtoCached($"{prevV} R", ending.tone)) phonemes.Add($"{prevV} R");
                else if (HasOtoCached($"{prevV} -", ending.tone)) phonemes.Add($"{prevV} -");
            }
            return phonemes;
        }

        private Result ProcessMultipleSyllables(string[] parts, Note[] notes, Note? prevNeighbour) {
            var phonemes = new List<Phoneme>();
            int totalDuration = notes.Sum(n => n.duration);
            int perSyllable = totalDuration / parts.Length;

            string prevV = "-";
            if (prevNeighbour.HasValue) {
                string prevLyric = prevNeighbour.Value.lyric;
                if (!string.IsNullOrEmpty(prevNeighbour.Value.phoneticHint))
                    prevLyric = prevNeighbour.Value.phoneticHint;
                var prevParts = prevLyric.Split(new[] { ' ', ',', '，', '、' },
                    StringSplitOptions.RemoveEmptyEntries);
                if (prevParts.Length > 0) {
                    var parsedPrev = Yue2JaCore.ParseJyutping(prevParts.Last());
                    if (parsedPrev != null && parsedPrev.Length > 0)
                        prevV = Yue2JaCore.ConvertToJapaneseVowel(parsedPrev[parsedPrev.Length - 1]);
                }
            }

            int cursor = 0;
            for (int i = 0; i < parts.Length; i++) {
                var parsed = Yue2JaCore.ParseJyutping(parts[i]);
                if (parsed == null) continue;

                string[] cc = parsed.Length > 1 ? parsed.Take(parsed.Length - 1).ToArray() : new string[0];
                string v = parsed[parsed.Length - 1];
                if (string.IsNullOrEmpty(v)) continue;

                var moras = Yue2JaCore.GenerateMoras(cc, v, fullJyutpingMap);
                if (moras.Count == 0) continue;

                int moraStep = Math.Max(1, perSyllable / moras.Count);
                for (int j = 0; j < moras.Count; j++) {
                    string alias;
                    if (j == 0) {
                        if (disableVcv || string.IsNullOrEmpty(prevV) || prevV == "-")
                            alias = PickAlias(notes[0].tone, $"- {moras[j]}", $"-{moras[j]}", moras[j]);
                        else
                            alias = PickAlias(notes[0].tone, $"{prevV} {moras[j]}", $"{prevV}{moras[j]}", moras[j]);
                    } else {
                        alias = PickAlias(notes[0].tone, moras[j]);
                    }
                    phonemes.Add(new Phoneme { phoneme = alias, position = cursor });
                    cursor += moraStep;
                }
                prevV = Yue2JaCore.ConvertToJapaneseVowel(v);
            }
            return new Result { phonemes = phonemes.ToArray() };
        }

        private string PickAlias(int tone, params string[] candidates) {
            foreach (var c in candidates) {
                if (HasOtoCached(c, tone)) return c;
                foreach (var variant in ExpandAliasVariants(c))
                    if (variant != c && HasOtoCached(variant, tone)) return variant;
            }
            return candidates.Last();
        }

        private IEnumerable<string> ExpandAliasVariants(string alias) {
            if (aliasOverrides.Count == 0) yield break;
            string bare = alias;
            string prefix = "";
            int lastSpace = alias.LastIndexOf(' ');
            if (lastSpace >= 0 && lastSpace < alias.Length - 1) {
                bare = alias.Substring(lastSpace + 1);
                prefix = alias.Substring(0, lastSpace + 1);
            }
            if (aliasOverrides.TryGetValue(bare, out var alts))
                foreach (var alt in alts) yield return prefix + alt;
        }

        private bool HasOtoCached(string alias, int tone) {
            var key = (alias, tone);
            if (!otoCache.TryGetValue(key, out bool result)) {
                result = HasOto(alias, tone);
                otoCache[key] = result;
            }
            return result;
        }
    }
}
