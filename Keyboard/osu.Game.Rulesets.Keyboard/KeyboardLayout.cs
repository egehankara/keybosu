// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Input.Bindings;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Keyboard.Localisation;
using osuTK;

namespace osu.Game.Rulesets.Keyboard
{
    /// <summary>
    /// What is printed on the keys: picked in settings. Names are written in their own language (like a language picker),
    /// so they need no translation. One entry per distinct keyboard: languages that share a keyboard share an entry
    /// (English QWERTY also covers Indonesian, Filipino, Malay, Dutch, Polish, Lithuanian, Latvian, Vietnamese and Chinese;
    /// Spanish covers Catalan).
    /// </summary>
    /// <remarks>New layouts go at the end; existing entries are never reordered.</remarks>
    public enum KeyboardLayoutType
    {
        [Description("English (QWERTY)")]
        EnglishQwerty,

        [Description("Türkçe Q")]
        TurkishQ,

        [Description("Türkçe F")]
        TurkishF,

        [Description("Deutsch (QWERTZ)")]
        German,

        [Description("Français (AZERTY)")]
        French,

        [Description("Español (España)")]
        Spanish,

        [Description("Русский (ЙЦУКЕН)")]
        Russian,

        [Description("English (Dvorak)")]
        Dvorak,

        [Description("English (Colemak)")]
        Colemak,

        [Description("日本語 (JIS)")]
        Japanese,

        [Description("한국어 (두벌식)")]
        Korean,

        [Description("Беларуская")]
        Belarusian,

        [Description("Українська")]
        Ukrainian,

        [Description("Български (БДС)")]
        Bulgarian,

        [Description("Српски (ћирилица)")]
        SerbianCyrillic,

        [Description("Hrvatski / Slovenščina / Srpski (latinica)")]
        Croatian,

        [Description("Čeština (QWERTZ)")]
        Czech,

        [Description("Slovenčina (QWERTZ)")]
        Slovak,

        [Description("Magyar (QWERTZ)")]
        Hungarian,

        [Description("Română")]
        Romanian,

        [Description("Italiano")]
        Italian,

        [Description("Português (Portugal)")]
        Portuguese,

        [Description("Português (Brasil, ABNT2)")]
        BrazilianPortuguese,

        [Description("Svenska / Suomi")]
        SwedishFinnish,

        [Description("Dansk")]
        Danish,

        [Description("Norsk")]
        Norwegian,

        [Description("Ελληνικά")]
        Greek,

        [Description("ไทย (เกษมณี)")]
        Thai,

        [Description("Español (Latinoamérica)")]
        SpanishLatinAmerica,
    }

    /// <summary>The physical shape of the keyboard: picked in settings.</summary>
    public enum KeyboardType
    {
        [LocalisableDescription(typeof(KeyboardStrings), nameof(KeyboardStrings.TypeAnsi))]
        Ansi,

        [LocalisableDescription(typeof(KeyboardStrings), nameof(KeyboardStrings.TypeIso))]
        Iso,
    }

    /// <summary>
    /// A single playable-looking key on the on-screen keyboard. Positions are in "key units" (one key is 1x1).
    /// </summary>
    public readonly struct KeyDefinition
    {
        public KeyboardAction Action { get; }

        public InputKey Key { get; }

        /// <summary>What is printed on the key in the layout this definition came from.</summary>
        public string Label { get; }

        /// <summary>Left edge, in key units.</summary>
        public float X { get; }

        /// <summary>Top edge, in key units (equal to the row index).</summary>
        public float Y { get; }

        /// <summary>Width in key units (1 for every key except Space).</summary>
        public float Width { get; }

        /// <summary>The label as shown on screen; words like "Space" follow the game's language.</summary>
        public LocalisableString DisplayLabel => Action == KeyboardAction.Space ? KeyboardStrings.KeySpace : Label;

        /// <summary>
        /// Whether beatmap conversion may place notes on this key. Only keys that exist on every keyboard type are
        /// playable; the extra keys (ANSI backslash, ISO key left of Z) are drawn dimmed and never get notes.
        /// </summary>
        public bool Playable { get; }

        public Vector2 Centre => new Vector2(X + Width / 2, Y + 0.5f);

        public KeyDefinition(KeyboardAction action, InputKey key, string label, float x, float y, bool playable = true, float width = 1)
        {
            Action = action;
            Key = key;
            Label = label;
            X = x;
            Y = y;
            Playable = playable;
            Width = width;
        }
    }

    /// <summary>A key shape that is only drawn (the ISO home-row key, which osu!framework cannot read). It has no action.</summary>
    public readonly struct DecorativeKey
    {
        public string Label { get; }

        public float X { get; }

        public float Y { get; }

        public float Width { get; }

        public float Height { get; }

        public LocalisableString DisplayLabel => Label;

        public DecorativeKey(string label, float x, float y, float width, float height)
        {
            Label = label;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    /// <summary>
    /// Describes where every key sits on screen and what is printed on it, for one combination of
    /// <see cref="KeyboardLayoutType"/> (labels) and <see cref="KeyboardType"/> (physical shape).
    /// The 33 letter and punctuation keys are identical across all combinations, so beatmap conversion
    /// never depends on what the player picked.
    /// </summary>
    public class KeyboardLayout
    {
        public string Name { get; }

        public KeyboardLayoutType LayoutType { get; }

        public KeyboardType KeyboardType { get; }

        /// <summary>Keys that can be pressed (including the dimmed extra key of this keyboard type).</summary>
        public IReadOnlyList<KeyDefinition> Keys { get; }

        /// <summary>Shapes that are only drawn.</summary>
        public IReadOnlyList<DecorativeKey> Decorations { get; }

        /// <summary>Total width in key units.</summary>
        public float Width { get; }

        /// <summary>Total height in key units.</summary>
        public float Height { get; }

        private readonly Dictionary<KeyboardAction, KeyDefinition> byAction;

        private KeyboardLayout(KeyboardLayoutType layoutType, KeyboardType keyboardType, IEnumerable<KeyDefinition> keys, IEnumerable<DecorativeKey> decorations)
        {
            LayoutType = layoutType;
            KeyboardType = keyboardType;
            Name = $"{layoutType} / {keyboardType}";
            Keys = keys.ToList();
            Decorations = decorations.ToList();
            byAction = Keys.ToDictionary(k => k.Action);
            Width = Math.Max(Keys.Max(k => k.X + k.Width), Decorations.Select(d => d.X + d.Width).DefaultIfEmpty(0).Max());
            Height = Math.Max(Keys.Max(k => k.Y + 1), Decorations.Select(d => d.Y + d.Height).DefaultIfEmpty(0).Max());
        }

        public KeyDefinition this[KeyboardAction action] => byAction[action];

        /// <summary>The Space bar starts this far into the X key (it just touches X, like a real keyboard)...</summary>
        public const float SPACE_START_INTO_X = 0.7f;

        /// <summary>...and ends this far into the comma key (a bit more than half of it).</summary>
        public const float SPACE_END_INTO_COMMA = 0.6f;

        /// <summary>
        /// The order layouts are listed in settings: most used first. Estimated from how many internet users live where each
        /// layout is the usual one (English QWERTY also covers China, India, Indonesia, the Philippines, Poland, the Netherlands...);
        /// enthusiast layouts last. Only the display order - the saved setting does not depend on it.
        /// </summary>
        public static readonly IReadOnlyList<KeyboardLayoutType> BY_POPULARITY = new[]
        {
            KeyboardLayoutType.EnglishQwerty,
            KeyboardLayoutType.SpanishLatinAmerica,
            KeyboardLayoutType.BrazilianPortuguese,
            KeyboardLayoutType.Russian,
            KeyboardLayoutType.Japanese,
            KeyboardLayoutType.German,
            KeyboardLayoutType.French,
            KeyboardLayoutType.TurkishQ,
            KeyboardLayoutType.Thai,
            KeyboardLayoutType.Italian,
            KeyboardLayoutType.Korean,
            KeyboardLayoutType.Spanish,
            KeyboardLayoutType.Ukrainian,
            KeyboardLayoutType.Romanian,
            KeyboardLayoutType.SwedishFinnish,
            KeyboardLayoutType.Czech,
            KeyboardLayoutType.Hungarian,
            KeyboardLayoutType.Greek,
            KeyboardLayoutType.Portuguese,
            KeyboardLayoutType.Croatian,
            KeyboardLayoutType.SerbianCyrillic,
            KeyboardLayoutType.Danish,
            KeyboardLayoutType.Norwegian,
            KeyboardLayoutType.Bulgarian,
            KeyboardLayoutType.Slovak,
            KeyboardLayoutType.Belarusian,
            KeyboardLayoutType.TurkishF,
            KeyboardLayoutType.Dvorak,
            KeyboardLayoutType.Colemak,
        };

        /// <summary>
        /// The eight keys fingers rest on, by physical position (so it is right in every layout,
        /// e.g. "Ş" on Turkish Q or "U İ E A" on Turkish F).
        /// </summary>
        public static readonly IReadOnlyCollection<KeyboardAction> HOME_ROW = new HashSet<KeyboardAction>
        {
            KeyboardAction.A, KeyboardAction.S, KeyboardAction.D, KeyboardAction.F,
            KeyboardAction.J, KeyboardAction.K, KeyboardAction.L, KeyboardAction.Semicolon,
        };

        /// <summary>The two index-finger keys that have a bump on a real keyboard.</summary>
        public static readonly IReadOnlyCollection<KeyboardAction> INDEX_FINGER_BUMPS = new HashSet<KeyboardAction>
        {
            KeyboardAction.F, KeyboardAction.J,
        };

        public bool Contains(KeyboardAction action) => byAction.ContainsKey(action);

        /// <summary>
        /// Use when only the identity of the 33 shared keys matters (conversion). Positions of those keys are
        /// stable enough for that; for drawing, always use the layout chosen in settings.
        /// </summary>
        public static KeyboardLayout Default => Create(KeyboardLayoutType.EnglishQwerty, KeyboardType.Ansi);

        /// <summary>Every key that can ever be bound, on any keyboard type. Used for the default key bindings.</summary>
        public static IEnumerable<KeyDefinition> AllBindableKeys
            => Default.Keys.Concat(Create(KeyboardLayoutType.EnglishQwerty, KeyboardType.Iso).Keys).GroupBy(k => k.Action).Select(g => g.First());

        private static readonly ConcurrentDictionary<(KeyboardLayoutType, KeyboardType), KeyboardLayout> cache
            = new ConcurrentDictionary<(KeyboardLayoutType, KeyboardType), KeyboardLayout>();

        public static KeyboardLayout Create(KeyboardLayoutType layoutType, KeyboardType keyboardType)
            => cache.GetOrAdd((layoutType, keyboardType), k => build(k.Item1, k.Item2));

        // The 33 keys shared by every keyboard, row by row, left to right. Row offsets mimic a real keyboard's stagger.
        private static readonly (float offset, (KeyboardAction action, InputKey key)[] keys)[] rows =
        {
            (0f, new[]
            {
                (KeyboardAction.Q, InputKey.Q),
                (KeyboardAction.W, InputKey.W),
                (KeyboardAction.E, InputKey.E),
                (KeyboardAction.R, InputKey.R),
                (KeyboardAction.T, InputKey.T),
                (KeyboardAction.Y, InputKey.Y),
                (KeyboardAction.U, InputKey.U),
                (KeyboardAction.I, InputKey.I),
                (KeyboardAction.O, InputKey.O),
                (KeyboardAction.P, InputKey.P),
                (KeyboardAction.BracketLeft, InputKey.BracketLeft),
                (KeyboardAction.BracketRight, InputKey.BracketRight),
            }),
            (0.25f, new[]
            {
                (KeyboardAction.A, InputKey.A),
                (KeyboardAction.S, InputKey.S),
                (KeyboardAction.D, InputKey.D),
                (KeyboardAction.F, InputKey.F),
                (KeyboardAction.G, InputKey.G),
                (KeyboardAction.H, InputKey.H),
                (KeyboardAction.J, InputKey.J),
                (KeyboardAction.K, InputKey.K),
                (KeyboardAction.L, InputKey.L),
                (KeyboardAction.Semicolon, InputKey.Semicolon),
                (KeyboardAction.Quote, InputKey.Quote),
            }),
            (0.75f, new[]
            {
                (KeyboardAction.Z, InputKey.Z),
                (KeyboardAction.X, InputKey.X),
                (KeyboardAction.C, InputKey.C),
                (KeyboardAction.V, InputKey.V),
                (KeyboardAction.B, InputKey.B),
                (KeyboardAction.N, InputKey.N),
                (KeyboardAction.M, InputKey.M),
                (KeyboardAction.Comma, InputKey.Comma),
                (KeyboardAction.Period, InputKey.Period),
                (KeyboardAction.Slash, InputKey.Slash),
            }),
        };

        // What is printed on the 33 shared keys, in the same order as above (12 + 11 + 10).
        private static string[] labelsFor(KeyboardLayoutType type)
        {
            switch (type)
            {
                case KeyboardLayoutType.TurkishQ:
                    return new[] { "Q W E R T Y U I O P Ğ Ü", "A S D F G H J K L Ş İ", "Z X C V B N M Ö Ç ." };

                case KeyboardLayoutType.TurkishF:
                    return new[] { "F G Ğ I O D R N H P Q W", "U İ E A Ü T K M L Y Ş", "J Ö V C Ç Z S B . ," };

                case KeyboardLayoutType.German:
                    return new[] { "Q W E R T Z U I O P Ü +", "A S D F G H J K L Ö Ä", "Y X C V B N M , . -" };

                case KeyboardLayoutType.French:
                    return new[] { "A Z E R T Y U I O P ^ $", "Q S D F G H J K L M Ù", "W X C V B N , ; : !" };

                case KeyboardLayoutType.Spanish:
                    return new[] { "Q W E R T Y U I O P ` +", "A S D F G H J K L Ñ ´", "Z X C V B N M , . -" };

                case KeyboardLayoutType.Russian:
                    return new[] { "Й Ц У К Е Н Г Ш Щ З Х Ъ", "Ф Ы В А П Р О Л Д Ж Э", "Я Ч С М И Т Ь Б Ю ." };

                case KeyboardLayoutType.Dvorak:
                    return new[] { "' , . P Y F G C R L / =", "A O E U I D H T N S -", "; Q J K X B M W V Z" };

                case KeyboardLayoutType.Colemak:
                    return new[] { "Q W F P G J L U Y ; [ ]", "A R S T D H N E I O '", "Z X C V B K M , . /" };

                case KeyboardLayoutType.Japanese:
                    return new[] { "Q W E R T Y U I O P @ [", "A S D F G H J K L ; :", "Z X C V B N M , . /" };

                case KeyboardLayoutType.Korean:
                    return new[] { "ㅂ ㅈ ㄷ ㄱ ㅅ ㅛ ㅕ ㅑ ㅐ ㅔ [ ]", "ㅁ ㄴ ㅇ ㄹ ㅎ ㅗ ㅓ ㅏ ㅣ ; '", "ㅋ ㅌ ㅊ ㅍ ㅠ ㅜ ㅡ , . /" };

                case KeyboardLayoutType.Belarusian:
                    return new[] { "Й Ц У К Е Н Г Ш Ў З Х '", "Ф Ы В А П Р О Л Д Ж Э", "Я Ч С М І Т Ь Б Ю ." };

                case KeyboardLayoutType.Ukrainian:
                    return new[] { "Й Ц У К Е Н Г Ш Щ З Х Ї", "Ф І В А П Р О Л Д Ж Є", "Я Ч С М И Т Ь Б Ю ." };

                case KeyboardLayoutType.Bulgarian:
                    return new[] { ", У Е И Ш Щ К С Д З Ц ;", "Ь Я А О Ж Г Т Н В М Ч", "Ю Й Ъ Э Ф Х П Р Л Б" };

                case KeyboardLayoutType.SerbianCyrillic:
                    return new[] { "Љ Њ Е Р Т З У И О П Ш Ђ", "А С Д Ф Г Х Ј К Л Ч Ћ", "Ѕ Џ Ц В Б Н М , . -" };

                case KeyboardLayoutType.Croatian:
                    return new[] { "Q W E R T Z U I O P Š Đ", "A S D F G H J K L Č Ć", "Y X C V B N M , . -" };

                case KeyboardLayoutType.Czech:
                    return new[] { "Q W E R T Z U I O P Ú )", "A S D F G H J K L Ů §", "Y X C V B N M , . -" };

                case KeyboardLayoutType.Slovak:
                    return new[] { "Q W E R T Z U I O P Ú Ä", "A S D F G H J K L Ô §", "Y X C V B N M , . -" };

                case KeyboardLayoutType.Hungarian:
                    return new[] { "Q W E R T Z U I O P Ő Ú", "A S D F G H J K L É Á", "Y X C V B N M , . -" };

                case KeyboardLayoutType.Romanian:
                    return new[] { "Q W E R T Y U I O P Ă Î", "A S D F G H J K L Ș Ț", "Z X C V B N M , . /" };

                case KeyboardLayoutType.Italian:
                    return new[] { "Q W E R T Y U I O P È +", "A S D F G H J K L Ò À", "Z X C V B N M , . -" };

                case KeyboardLayoutType.Portuguese:
                    return new[] { "Q W E R T Y U I O P + ´", "A S D F G H J K L Ç º", "Z X C V B N M , . -" };

                case KeyboardLayoutType.BrazilianPortuguese:
                    return new[] { "Q W E R T Y U I O P ´ [", "A S D F G H J K L Ç ~", "Z X C V B N M , . ;" };

                case KeyboardLayoutType.SwedishFinnish:
                    return new[] { "Q W E R T Y U I O P Å ¨", "A S D F G H J K L Ö Ä", "Z X C V B N M , . -" };

                case KeyboardLayoutType.Danish:
                    return new[] { "Q W E R T Y U I O P Å ¨", "A S D F G H J K L Æ Ø", "Z X C V B N M , . -" };

                case KeyboardLayoutType.Norwegian:
                    return new[] { "Q W E R T Y U I O P Å ¨", "A S D F G H J K L Ø Æ", "Z X C V B N M , . -" };

                case KeyboardLayoutType.Greek:
                    return new[] { "; ς Ε Ρ Τ Υ Θ Ι Ο Π [ ]", "Α Σ Δ Φ Γ Η Ξ Κ Λ ΄ '", "Ζ Χ Ψ Ω Β Ν Μ , . /" };

                case KeyboardLayoutType.Thai:
                    return new[] { "ๆ ไ ำ พ ะ ั ี ร น ย บ ล", "ฟ ห ก ด เ ้ ่ า ส ว ง", "ผ ป แ อ ิ ื ท ม ใ ฝ" };

                case KeyboardLayoutType.SpanishLatinAmerica:
                    return new[] { "Q W E R T Y U I O P ´ +", "A S D F G H J K L Ñ {", "Z X C V B N M , . -" };

                default:
                    return new[] { "Q W E R T Y U I O P [ ]", "A S D F G H J K L ; '", "Z X C V B N M , . /" };
            }
        }

        // What is printed on the extra keys: (ANSI backslash key, ISO key left of Z, ISO extra key at the end of the home row).
        private static (string ansiBackslash, string isoLeftOfZ, string isoHomeRowEnd) extraLabelsFor(KeyboardLayoutType type)
        {
            switch (type)
            {
                case KeyboardLayoutType.TurkishQ:
                    return (",", "<", ",");

                case KeyboardLayoutType.TurkishF:
                    return ("X", "<", "X");

                case KeyboardLayoutType.German:
                    return ("#", "<", "#");

                case KeyboardLayoutType.French:
                    return ("*", "<", "*");

                case KeyboardLayoutType.Spanish:
                    return ("Ç", "<", "Ç");

                case KeyboardLayoutType.Russian:
                    return ("\\", "\\", "\\");

                case KeyboardLayoutType.Japanese:
                    return ("]", "\\", "]");

                case KeyboardLayoutType.Korean:
                    return ("₩", "\\", "₩");

                case KeyboardLayoutType.Belarusian:
                    return ("\\", "\\", "\\");

                case KeyboardLayoutType.Ukrainian:
                    return ("\\", "Ґ", "\\");

                case KeyboardLayoutType.Bulgarian:
                    return ("\\", "\\", "\\");

                case KeyboardLayoutType.SerbianCyrillic:
                    return ("Ж", "<", "Ж");

                case KeyboardLayoutType.Croatian:
                    return ("Ž", "<", "Ž");

                case KeyboardLayoutType.Czech:
                    return ("¨", "\\", "¨");

                case KeyboardLayoutType.Slovak:
                    return ("Ň", "&", "Ň");

                case KeyboardLayoutType.Hungarian:
                    return ("Ű", "Í", "Ű");

                case KeyboardLayoutType.Romanian:
                    return ("Â", "\\", "Â");

                case KeyboardLayoutType.Italian:
                    return ("Ù", "<", "Ù");

                case KeyboardLayoutType.Portuguese:
                    return ("~", "<", "~");

                case KeyboardLayoutType.BrazilianPortuguese:
                    return ("]", "\\", "]");

                case KeyboardLayoutType.SwedishFinnish:
                    return ("'", "<", "'");

                case KeyboardLayoutType.Danish:
                    return ("'", "<", "'");

                case KeyboardLayoutType.Norwegian:
                    return ("'", "<", "'");

                case KeyboardLayoutType.Greek:
                    return ("\\", "<", "\\");

                case KeyboardLayoutType.Thai:
                    return ("ฃ", "ฃ", "ฃ");

                case KeyboardLayoutType.SpanishLatinAmerica:
                    return ("}", "<", "}");

                default:
                    return ("\\", "\\", "#");
            }
        }

        private static KeyboardLayout build(KeyboardLayoutType layoutType, KeyboardType keyboardType)
        {
            // ISO has a key left of Z, which pushes everything right by a quarter key so no coordinate is negative.
            float shift = keyboardType == KeyboardType.Iso ? 0.25f : 0f;

            var keys = new List<KeyDefinition>();
            var decorations = new List<DecorativeKey>();
            string[] labels = labelsFor(layoutType);
            var extra = extraLabelsFor(layoutType);

            for (int y = 0; y < rows.Length; y++)
            {
                string[] rowLabels = labels[y].Split(' ');

                if (rowLabels.Length != rows[y].keys.Length)
                    throw new InvalidOperationException($"Layout '{layoutType}' row {y} has {rowLabels.Length} labels but the keyboard has {rows[y].keys.Length} keys.");

                for (int i = 0; i < rowLabels.Length; i++)
                {
                    var (action, key) = rows[y].keys[i];
                    keys.Add(new KeyDefinition(action, key, rowLabels[i], shift + rows[y].offset + i, y));
                }
            }

            if (keyboardType == KeyboardType.Ansi)
            {
                // Backslash sits at the end of the top row. (Enter is not drawn: it never gets notes and only took up room.)
                keys.Add(new KeyDefinition(KeyboardAction.BackSlash, InputKey.BackSlash, extra.ansiBackslash, 12f, 0, playable: false));
            }
            else
            {
                // The ISO key left of Z can be read; the extra key at the end of the home row cannot (osu!framework
                // does not map that scancode), so it is only drawn. (Enter is not drawn.)
                keys.Add(new KeyDefinition(KeyboardAction.NonUSBackSlash, InputKey.NonUSBackSlash, extra.isoLeftOfZ, shift + 0.75f - 1f, 2, playable: false));
                decorations.Add(new DecorativeKey(extra.isoHomeRowEnd, shift + 11.25f, 1, 1, 1));
            }

            // Space bar under the bottom row, like a real one: from just inside X to a bit past the middle of the comma key.
            // Spinners are held here.
            float spaceStart = keys.Single(k => k.Action == KeyboardAction.X).X + SPACE_START_INTO_X;
            float spaceEnd = keys.Single(k => k.Action == KeyboardAction.Comma).X + SPACE_END_INTO_COMMA;
            keys.Add(new KeyDefinition(KeyboardAction.Space, InputKey.Space, "Space", spaceStart, 3, width: spaceEnd - spaceStart));

            return new KeyboardLayout(layoutType, keyboardType, keys, decorations);
        }
    }
}
