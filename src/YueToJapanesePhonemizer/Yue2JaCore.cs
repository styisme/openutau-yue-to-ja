// =====================================================================
// Yue2JaCore - 纯逻辑与数据表（粤拼 Jyutping → 日语假名）
// 不依赖 OpenUtau 运行时。
//
// v1.0.0 粤语版
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenUtau.Plugin.Builtin {

    internal static class Yue2JaCore {

        private static readonly Regex DateRegex = new Regex(
            @"\d{4}[-/]\d{1,2}[-/]\d{1,2}", RegexOptions.Compiled);
        private static readonly Regex TimeRegex = new Regex(
            @"\d{1,2}:\d{2}(?::\d{2})?", RegexOptions.Compiled);

        internal static readonly Dictionary<string, string> RomajiToKana = new() {
            { "a", "あ" }, { "i", "い" }, { "u", "う" }, { "e", "え" }, { "o", "お" },
            { "n", "ん" }, { "N", "ん" }, { "yi", "い" },
            { "ba", "ば" }, { "bi", "び" }, { "bu", "ぶ" }, { "be", "べ" }, { "bo", "ぼ" },
            { "bya", "びゃ" }, { "byu", "びゅ" }, { "byo", "びょ" },
            { "pa", "ぱ" }, { "pi", "ぴ" }, { "pu", "ぷ" }, { "pe", "ぺ" }, { "po", "ぽ" },
            { "pya", "ぴゃ" }, { "pyu", "ぴゅ" }, { "pyo", "ぴょ" },
            { "ma", "ま" }, { "mi", "み" }, { "mu", "む" }, { "me", "め" }, { "mo", "も" },
            { "mya", "みゃ" }, { "myu", "みゅ" }, { "myo", "みょ" },
            { "fa", "ふぁ" }, { "fi", "ふぃ" }, { "fu", "ふ" }, { "fe", "ふぇ" }, { "fo", "ふぉ" },
            { "ha", "は" }, { "hi", "ひ" }, { "hu", "ふ" }, { "he", "へ" }, { "ho", "ほ" },
            { "hya", "ひゃ" }, { "hyu", "ひゅ" }, { "hyo", "ひょ" },
            { "da", "だ" }, { "di", "でぃ" }, { "du", "どぅ" }, { "de", "で" }, { "do", "ど" },
            { "dya", "じゃ" }, { "dyu", "じゅ" }, { "dyo", "じょ" },
            { "ta", "た" }, { "ti", "てぃ" }, { "tu", "とぅ" }, { "te", "て" }, { "to", "と" },
            { "tya", "ちゃ" }, { "tyu", "ちゅ" }, { "tyo", "ちょ" },
            { "na", "な" }, { "ni", "に" }, { "nu", "ぬ" }, { "ne", "ね" }, { "no", "の" },
            { "nya", "にゃ" }, { "nyu", "にゅ" }, { "nyo", "にょ" },
            { "ra", "ら" }, { "ri", "り" }, { "ru", "る" }, { "re", "れ" }, { "ro", "ろ" },
            { "rya", "りゃ" }, { "ryu", "りゅ" }, { "ryo", "りょ" },
            { "ga", "が" }, { "gi", "ぎ" }, { "gu", "ぐ" }, { "ge", "げ" }, { "go", "ご" },
            { "gya", "ぎゃ" }, { "gyu", "ぎゅ" }, { "gyo", "ぎょ" },
            { "ka", "か" }, { "ki", "き" }, { "ku", "く" }, { "ke", "け" }, { "ko", "こ" },
            { "kya", "きゃ" }, { "kyu", "きゅ" }, { "kyo", "きょ" },
            { "za", "ざ" }, { "zi", "じ" }, { "zu", "ず" }, { "ze", "ぜ" }, { "zo", "ぞ" },
            { "ja", "じゃ" }, { "ji", "じ" }, { "ju", "じゅ" }, { "je", "じぇ" }, { "jo", "じょ" },
            { "sa", "さ" }, { "si", "し" }, { "su", "す" }, { "se", "せ" }, { "so", "そ" },
            { "sha", "しゃ" }, { "shi", "し" }, { "shu", "しゅ" }, { "she", "しぇ" }, { "sho", "しょ" },
            { "tsa", "つぁ" }, { "tsi", "つぃ" }, { "tsu", "つ" }, { "tse", "つぇ" }, { "tso", "つぉ" },
            { "cha", "ちゃ" }, { "chi", "ち" }, { "chu", "ちゅ" }, { "che", "ちぇ" }, { "cho", "ちょ" },
            { "ya", "や" }, { "yu", "ゆ" }, { "ye", "いぇ" }, { "yo", "よ" },
            { "wa", "わ" }, { "wi", "うぃ" }, { "wu", "う" }, { "we", "うぇ" }, { "wo", "うぉ" },
            { "va", "ヴぁ" }, { "vi", "ヴぃ" }, { "vu", "ヴ" }, { "ve", "ヴぇ" }, { "vo", "ヴぉ" },
        };

        internal static readonly string[] Initials = {
            "ng", "gw", "kw",
            "b", "p", "m", "f", "d", "t", "n", "l",
            "g", "k", "h", "w", "z", "c", "s", "j"
        };

        internal static readonly string[] Finals = {
            // 3+ 字符
            "aang", "oeng",
            "aai", "aau", "aam", "aan", "aap", "aat", "aak",
            "eoi", "eon", "eot", "oek",
            "ing", "ung", "yun", "yut",
            "em", "en", "eng", "ep", "et", "ek",
            // 2 字符
            "aa", "ai", "au", "ei", "eu", "ou", "oi", "iu", "ui",
            "im", "in", "ip", "it", "ik",
            "on", "ong", "ot", "ok",
            "un", "ut", "uk",
            "oe", "eo", "yu", "ng",
            "am", "an", "ang", "ap", "at", "ak",
            // 1 字符
            "a", "e", "i", "o", "u", "m"
        };

        internal static readonly Dictionary<string, string> InitialMap = new() {
            { "b", "b" }, { "p", "p" }, { "m", "m" }, { "f", "h" },
            { "d", "d" }, { "t", "t" }, { "n", "n" }, { "l", "r" },
            { "g", "g" }, { "k", "k" }, { "h", "h" }, { "ng", "g" },
            { "gw", "g" }, { "kw", "k" }, { "w", "w" },
            { "z", "j" }, { "c", "ch" }, { "s", "sh" }, { "j", "y" },
        };

        internal static readonly HashSet<string> LightToneChars = new();

        internal static readonly Dictionary<string, string[]> DefaultFullJyutpingMap = new();

        internal static readonly Dictionary<string, string> DefaultPolyphoneMap = new() {
            { "ngan haang", "ngan hang" },
            { "coeng daai", "zoeng daai" },
        };

        internal static readonly HashSet<char> Punctuation = new() {
            '，', '。', '！', '？', '、', '；', '：', '“', '”', '‘', '’',
            '（', '）', '《', '》', '【', '】', '…', '—', '～', '·',
            ',', '.', '!', '?', ';', ':', '"', '\'', '(', ')',
            '[', ']', '{', '}', '<', '>', '/', '\\', '|', '`',
        };

        internal static readonly HashSet<char> LongMarks = new() { '~', '～', '—', 'ー' };

        internal static readonly string[] DigitJyutping = {
            "ling", "jat", "ji", "saam", "sei", "ng", "luk", "cat", "baat", "gau"
        };

        // ------------------------------------------------------------
        // 文本处理
        // ------------------------------------------------------------
        internal static string ReplacePunctuation(string s) {
            var sb = new StringBuilder(s.Length);
            foreach (char c in s) {
                if (Punctuation.Contains(c)) sb.Append(' ');
                else sb.Append(c);
            }
            return sb.ToString().Trim();
        }

        internal static string ReplaceDigits(string s) {
            if (!s.Any(char.IsDigit)) return s;
            var sb = new StringBuilder();
            int i = 0;
            while (i < s.Length) {
                var dm = DateRegex.Match(s, i);
                if (dm.Success && dm.Index == i) {
                    sb.Append(' ').Append(ReadDate(dm)).Append(' ');
                    i += dm.Length;
                    continue;
                }
                var tm = TimeRegex.Match(s, i);
                if (tm.Success && tm.Index == i) {
                    sb.Append(' ').Append(ReadTime(tm)).Append(' ');
                    i += tm.Length;
                    continue;
                }
                if (char.IsDigit(s[i])) {
                    sb.Append(' ').Append(ReadNumberChunk(s, ref i)).Append(' ');
                    continue;
                }
                sb.Append(s[i]);
                i++;
            }
            var result = Regex.Replace(sb.ToString(), @"\s+", " ");
            return result.Trim();
        }

        private static string ReadNumberChunk(string s, ref int i) {
            int start = i;
            while (i < s.Length && (char.IsDigit(s[i]) || s[i] == ',')) i++;
            bool hasDot = false;
            if (i + 1 < s.Length && s[i] == '.' && char.IsDigit(s[i + 1])) {
                hasDot = true; i++;
                while (i < s.Length && char.IsDigit(s[i])) i++;
            }
            string numStr = s.Substring(start, i - start).Replace(",", "");
            bool isPercent = false;
            if (i < s.Length && s[i] == '%') { isPercent = true; i++; }

            if (hasDot) {
                var parts = numStr.Split('.');
                string intPart = long.TryParse(parts[0], out var ip) ? ReadCardinal(ip) : ReadDigitByDigit(parts[0]);
                string decPart = ReadDigitByDigit(parts[1]);
                string result = intPart + " dim " + decPart;
                if (isPercent) result = "baak fan zi " + result;
                return result;
            }
            if (isPercent) {
                if (long.TryParse(numStr, out var pct)) return "baak fan zi " + ReadCardinal(pct);
                return "baak fan zi " + ReadDigitByDigit(numStr);
            }
            return ConvertDigitsString(numStr);
        }

        private static string ReadDate(Match m) {
            string full = m.Value;
            char sep = full.Contains('-') ? '-' : '/';
            var parts = full.Split(sep);
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);
            return ReadYear(year) + " nin " +
                   ReadCardinal(month) + " jyut " +
                   ReadCardinal(day) + " jat";
        }

        private static string ReadTime(Match m) {
            var parts = m.Value.Split(':');
            int hour = int.Parse(parts[0]);
            int minute = int.Parse(parts[1]);
            string result = ReadCardinal(hour) + " dim";
            if (minute > 0) result += " " + ReadCardinal(minute) + " fan";
            if (parts.Length > 2) {
                int second = int.Parse(parts[2]);
                if (second > 0) result += " " + ReadCardinal(second) + " miu";
            }
            return result;
        }

        private static string ReadYear(int year) => ReadDigitByDigit(year.ToString());

        private static string ReadCardinal(long num) {
            if (num == 0) return "ling";
            if (num < 0) return "fu " + ReadCardinal(-num);
            if (num > 999999999999L) return ReadDigitByDigit(num.ToString());

            var parts = new List<string>();
            if (num >= 100000000) {
                long jik = num / 100000000;
                parts.Add(ReadCardinal(jik) + " jik");
                num %= 100000000;
                if (num > 0 && num < 10000000) parts.Add("ling");
            }
            if (num >= 10000) {
                long maan = num / 10000;
                parts.Add(ReadCardinal(maan) + " maan");
                num %= 10000;
                if (num > 0 && num < 1000) parts.Add("ling");
            }
            if (num >= 1000) {
                long cin = num / 1000;
                parts.Add(DigitJyutping[cin] + " cin");
                num %= 1000;
                if (num > 0 && num < 100) parts.Add("ling");
            }
            if (num >= 100) {
                long baak = num / 100;
                parts.Add(DigitJyutping[baak] + " baak");
                num %= 100;
                if (num > 0 && num < 10) parts.Add("ling");
            }
            if (num >= 10) {
                long sap = num / 10;
                if (sap > 1) parts.Add(DigitJyutping[sap]);
                parts.Add("sap");
                num %= 10;
            }
            if (num > 0) parts.Add(DigitJyutping[num]);
            return string.Join(" ", parts);
        }

        private static string ReadDigitByDigit(string digits) =>
            string.Join(" ", digits.Where(char.IsDigit).Select(c => DigitJyutping[c - '0']));

        private static string ConvertDigitsString(string digits) =>
            ConvertDigitSequence(digits.Where(char.IsDigit).Select(c => c - '0').ToList());

        internal static string ConvertDigitSequence(List<int> digits) {
            if (digits.Count == 0) return "";
            if (digits.Count == 1) return DigitJyutping[digits[0]];
            long num = 0;
            foreach (var d in digits) num = num * 10 + d;
            if (num == 0) return string.Join(" ", digits.Select(d => DigitJyutping[d]));
            if (num == 10) return "sap";
            if (num < 20) return "sap " + DigitJyutping[num - 10];
            if (num < 100) {
                int t = (int)(num / 10);
                int o = (int)(num % 10);
                return o == 0 ? DigitJyutping[t] + " sap"
                              : DigitJyutping[t] + " sap " + DigitJyutping[o];
            }
            return string.Join(" ", digits.Select(d => DigitJyutping[d]));
        }

        internal static string StripLongMarks(string s) =>
            new string(s.Where(c => !LongMarks.Contains(c)).ToArray()).Trim();

        internal static string StripDigits(string s) =>
            new string(s.Where(c => !char.IsDigit(c)).ToArray());

        internal static bool ContainsJapaneseKana(string s) {
            foreach (char c in s) {
                if ((c >= 0x3040 && c <= 0x309F) ||
                    (c >= 0x30A0 && c <= 0x30FF) ||
                    (c >= 0xFF66 && c <= 0xFF9D)) return true;
            }
            return false;
        }

        // ------------------------------------------------------------
        // 粤拼解析
        // ------------------------------------------------------------
        internal static string[] ParseJyutping(string jyutping) {
            jyutping = new string(jyutping.Where(c => c >= 'a' && c <= 'z').ToArray()).ToLowerInvariant();
            if (string.IsNullOrEmpty(jyutping)) return null;

            if (jyutping == "m" || jyutping == "ng") return new[] { jyutping };
            if (Finals.Contains(jyutping)) return new[] { jyutping };

            foreach (var init in Initials.OrderByDescending(s => s.Length)) {
                if (jyutping.StartsWith(init)) {
                    string remaining = jyutping.Substring(init.Length);
                    if (Finals.Contains(remaining))
                        return new[] { init, remaining };
                }
            }
            return null;
        }

        // ------------------------------------------------------------
        // 假名转换
        // ------------------------------------------------------------
        internal static List<string> GenerateMoras(string[] cc, string final,
            Dictionary<string, string[]> fullMap) {
            var moras = new List<string>();
            string rawInitial = (cc != null && cc.Length > 0) ? cc[0] : "";
            string full = rawInitial + final;

            if (fullMap.TryGetValue(full, out var mapped)) {
                foreach (var m in mapped) moras.Add(ToKana(m));
                return moras;
            }

            string cons = "";
            if (cc != null && cc.Length > 0)
                cons = InitialMap.TryGetValue(cc[0], out var jc) ? jc : cc[0];

            var vowelSeq = GetJapaneseVowelSequence(final);
            if (vowelSeq.Count == 0) return moras;

            if (!string.IsNullOrEmpty(cons)) {
                string firstV = vowelSeq[0];
                vowelSeq.RemoveAt(0);
                if (firstV == "N") {
                    moras.Add(ToKana(cons + "u"));
                    moras.Add("ん");
                } else {
                    moras.Add(ToKana(cons + firstV));
                }
            }
            foreach (var v in vowelSeq) {
                if (v == "N") moras.Add("ん");
                else moras.Add(ToKana(v));
            }
            return moras;
        }

        internal static List<string> GetJapaneseVowelSequence(string final) {
            switch (final) {
                // 单元音
                case "aa": return new List<string> { "a" };
                case "a":  return new List<string> { "a" };
                case "e":  return new List<string> { "e" };
                case "i":  return new List<string> { "i" };
                case "o":  return new List<string> { "o" };
                case "u":  return new List<string> { "u" };
                case "yu": return new List<string> { "yu" };
                case "oe": return new List<string> { "e" };
                case "eo": return new List<string> { "o" };
                // 复元音
                case "aai": return new List<string> { "a", "i" };
                case "aau": return new List<string> { "a", "u" };
                case "ai":  return new List<string> { "a", "i" };
                case "au":  return new List<string> { "a", "u" };
                case "ei":  return new List<string> { "e", "i" };
                case "eu":  return new List<string> { "e", "u" };
                case "eoi": return new List<string> { "e", "o", "i" };
                case "eot": return new List<string> { "e", "o" };
                case "ou":  return new List<string> { "o", "u" };
                case "oi":  return new List<string> { "o", "i" };
                case "iu":  return new List<string> { "i", "u" };
                case "ui":  return new List<string> { "u", "i" };
                // 鼻音韵尾
                case "aam": case "am": case "em":
                case "aan": case "an": case "en":
                case "aang": case "ang": case "eng":
                    return new List<string> { "a", "N" };
                case "im": case "in": case "ing":
                    return new List<string> { "i", "N" };
                case "on": case "ong":
                    return new List<string> { "o", "N" };
                case "un": case "ung":
                    return new List<string> { "u", "N" };
                case "eon":
                    return new List<string> { "e", "o", "N" };
                case "oeng":
                    return new List<string> { "e", "N" };
                case "yun":
                    return new List<string> { "yu", "N" };
                case "m": case "ng":
                    return new List<string> { "N" };
                // 入声韵尾（日语无塞音尾，丢弃）
                case "aap": case "ap":
                case "aat": case "at":
                case "aak": case "ak":
                    return new List<string> { "a" };
                case "ip": case "it": case "ik":
                    return new List<string> { "i" };
                case "ot": case "ok":
                    return new List<string> { "o" };
                case "ut": case "uk":
                    return new List<string> { "u" };
                case "oek":
                    return new List<string> { "e" };
                case "yut":
                    return new List<string> { "yu" };
                case "ep": case "et": case "ek":
                    return new List<string> { "e" };
                default:
                    return new List<string> { final };
            }
        }

        internal static string ToKana(string romaji) {
            if (string.IsNullOrEmpty(romaji)) return romaji;
            if (RomajiToKana.TryGetValue(romaji, out var kana)) return kana;
            return romaji;
        }

        internal static string ConvertToJapaneseVowel(string jyutpingFinal) {
            if (string.IsNullOrEmpty(jyutpingFinal)) return "-";
            if (jyutpingFinal == "-" || jyutpingFinal == "R") return jyutpingFinal;
            var seq = GetJapaneseVowelSequence(jyutpingFinal);
            if (seq.Count > 0) {
                var last = seq.Last();
                if (last == "N") return "n";
                return last.Last().ToString();
            }
            return jyutpingFinal.Last().ToString();
        }

        internal static string GetLastJapaneseVowel(string mora) {
            if (string.IsNullOrEmpty(mora)) return "-";
            if (mora.EndsWith("ん")) return "n";
            if (mora.EndsWith("ゃ")) return "a";
            if (mora.EndsWith("ゅ")) return "u";
            if (mora.EndsWith("ょ")) return "o";
            char last = mora.Last();
            if ("あかさたなはまやらわがざだばぱ".Contains(last)) return "a";
            if ("いきしちにひみりぎじぢびぴ".Contains(last)) return "i";
            if ("うくすつぬふむゆるぐずづぶぷ".Contains(last)) return "u";
            if ("えけせてねへめれげぜでべぺ".Contains(last)) return "e";
            if ("おこそとのほもよろごぞどぼぽ".Contains(last)) return "o";
            return "-";
        }
    }
}