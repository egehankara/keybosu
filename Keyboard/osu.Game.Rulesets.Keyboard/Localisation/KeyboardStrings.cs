// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Keyboard.Localisation
{
    /// <summary>
    /// Every piece of text the ruleset shows. The language follows osu!lazer's own language setting and updates live.
    /// Translations live in <c>Resources/Localisation/translations.ini</c> (one [section] per language, <c>key = text</c>);
    /// anything missing falls back to English.
    /// </summary>
    public static class KeyboardStrings
    {
        // Settings
        public static LocalisableString KeyboardLayout => get(nameof(KeyboardLayout));
        public static LocalisableString KeyboardLayoutTooltip => tooltip(nameof(KeyboardLayoutTooltip));
        public static LocalisableString KeyboardType => get(nameof(KeyboardType));
        public static LocalisableString KeyboardTypeTooltip => tooltip(nameof(KeyboardTypeTooltip));
        public static LocalisableString TypeAnsi => get(nameof(TypeAnsi));
        public static LocalisableString TypeIso => get(nameof(TypeIso));
        public static LocalisableString HighlightHomeRow => get(nameof(HighlightHomeRow));
        public static LocalisableString HighlightHomeRowTooltip => tooltip(nameof(HighlightHomeRowTooltip));
        public static LocalisableString ScreenCentre => get(nameof(ScreenCentre));
        public static LocalisableString ScreenCentreTooltip => tooltip(nameof(ScreenCentreTooltip));
        public static LocalisableString CentreWholeKeyboard => get(nameof(CentreWholeKeyboard));
        public static LocalisableString CentreSpaceBar => get(nameof(CentreSpaceBar));
        public static LocalisableString CentreBetweenIndexFingers => get(nameof(CentreBetweenIndexFingers));
        public static LocalisableString ShowJudgements => get(nameof(ShowJudgements));
        public static LocalisableString ShowJudgementsTooltip => tooltip(nameof(ShowJudgementsTooltip));

        /// <summary>Appended to every setting's tooltip.</summary>
        public static LocalisableString AppliesNextMap => get(nameof(AppliesNextMap));

        public static LocalisableString Feedback => get(nameof(Feedback));

        // On the playfield
        public static LocalisableString KeySpace => get(nameof(KeySpace));
        public static LocalisableString HoldSpace => get(nameof(HoldSpace));

        // Key bindings panel
        public static LocalisableString KeyBindingsHint => get(nameof(KeyBindingsHint));

        /// <summary>Languages that have a section in the translation file (lower-case culture names, e.g. "tr", "pt-br").</summary>
        public static IEnumerable<string> Languages => translations.Value.Keys;

        /// <summary>Every key English defines; every other language is expected to define the same ones.</summary>
        public static IEnumerable<string> AllKeys => translations.Value[english].Keys;

        /// <summary>The raw translation of a key in one language section, or null. For tests.</summary>
        public static string? RawTranslation(string language, string key)
            => translations.Value.TryGetValue(language, out var texts) && texts.TryGetValue(key, out string? text) ? text : null;

        /// <summary>
        /// The text for <paramref name="key"/> in <paramref name="language"/> (a culture name such as "tr", "pt-BR" or "zh-Hant"),
        /// or English when there is no translation.
        /// </summary>
        public static string Translate(string key, string? language)
        {
            string? section = sectionFor(language);

            if (section != null && translations.Value[section].TryGetValue(key, out string? text))
                return text;

            return translations.Value[english].TryGetValue(key, out string? fallback) ? fallback : key;
        }

        /// <summary>Which section of the translation file serves a culture name, or null if none does (English is then used).</summary>
        public static string? SectionFor(string? language) => sectionFor(language);

        private const string english = "en";

        // Culture names lazer may report that are spelled differently in the file.
        private static readonly Dictionary<string, string> aliases = new Dictionary<string, string>
        {
            ["nb"] = "no",
            ["nn"] = "no",
            ["zh-tw"] = "zh-hant",
            ["zh-hk"] = "zh-hant",
            ["zh-hans"] = "zh",
            ["zh-cn"] = "zh",
        };

        private static string? sectionFor(string? language)
        {
            if (string.IsNullOrEmpty(language))
                return null;

            string name = language.ToLowerInvariant().Replace('_', '-');

            // Most specific first: "pt-br", then "pt".
            foreach (string candidate in new[] { name, name.Split('-')[0] })
            {
                string resolved = aliases.TryGetValue(candidate, out string? alias) ? alias : candidate;

                if (translations.Value.ContainsKey(resolved))
                    return resolved;
            }

            return null;
        }

        private static LocalisableString get(string key) => new LocalisableString(new KeyboardLocalisedString(key));

        private static LocalisableString tooltip(string key) => new LocalisableString(new KeyboardLocalisedString(key, nameof(AppliesNextMap)));

        private static readonly Lazy<Dictionary<string, Dictionary<string, string>>> translations = new Lazy<Dictionary<string, Dictionary<string, string>>>(load);

        private static Dictionary<string, Dictionary<string, string>> load()
        {
            var assembly = typeof(KeyboardStrings).Assembly;
            string resource = assembly.GetManifestResourceNames().Single(n => n.EndsWith("translations.ini", StringComparison.Ordinal));

            using var stream = assembly.GetManifestResourceStream(resource)!;
            using var reader = new StreamReader(stream);

            return Parse(reader.ReadToEnd());
        }

        /// <summary>Reads the translation file format: <c>[language]</c> sections of <c>key = text</c> lines; <c>#</c> starts a comment.</summary>
        public static Dictionary<string, Dictionary<string, string>> Parse(string text)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string>? current = null;

            foreach (string rawLine in text.Split('\n'))
            {
                string line = rawLine.Trim();

                if (line.Length == 0 || line.StartsWith('#'))
                    continue;

                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    current = result[line[1..^1].Trim().ToLowerInvariant()] = new Dictionary<string, string>();
                    continue;
                }

                int equals = line.IndexOf('=');

                if (current == null || equals < 0)
                    throw new FormatException($"Bad line in translations.ini: \"{line}\"");

                current[line[..equals].Trim()] = line[(equals + 1)..].Trim();
            }

            return result;
        }
    }

    /// <summary>
    /// A string that osu!framework re-resolves whenever the game's language changes.
    /// Several keys are joined with a space (used to append "applies next time" to tooltips).
    /// </summary>
    public class KeyboardLocalisedString : ILocalisableStringData
    {
        public IReadOnlyList<string> Keys { get; }

        public KeyboardLocalisedString(params string[] keys)
        {
            Keys = keys;
        }

        public string GetLocalised(LocalisationParameters parameters)
            => string.Join(" ", Keys.Select(k => KeyboardStrings.Translate(k, parameters.Store?.EffectiveCulture.Name)));

        public bool Equals(ILocalisableStringData? other) => other is KeyboardLocalisedString s && s.Keys.SequenceEqual(Keys);

        public override string ToString() => string.Join(" ", Keys.Select(k => KeyboardStrings.Translate(k, null)));
    }
}
