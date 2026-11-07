// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Input.Bindings;
using osuTK;

namespace osu.Game.Rulesets.Keyboard
{
    /// <summary>What is printed on the keys: picked in settings.</summary>
    public enum KeyboardLayoutType
    {
        [Description("English (QWERTY)")]
        EnglishQwerty,

        [Description("Turkish Q")]
        TurkishQ,

        [Description("Turkish F")]
        TurkishF,
    }

    /// <summary>The physical shape of the keyboard: picked in settings.</summary>
    public enum KeyboardType
    {
        [Description("ANSI (US) - flat Enter")]
        Ansi,

        [Description("ISO (Turkish / European) - tall Enter")]
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

        /// <summary>
        /// Whether beatmap conversion may place notes on this key. Only keys that exist on every keyboard type are
        /// playable; the extra keys (ANSI backslash, ISO key left of Z) are drawn dimmed and never get notes.
        /// </summary>
        public bool Playable { get; }

        public Vector2 Centre => new Vector2(X + 0.5f, Y + 0.5f);

        public KeyDefinition(KeyboardAction action, InputKey key, string label, float x, float y, bool playable = true)
        {
            Action = action;
            Key = key;
            Label = label;
            X = x;
            Y = y;
            Playable = playable;
        }
    }

    /// <summary>A key shape that is only drawn (Enter, and the ISO home-row key osu!framework cannot read). It has no action.</summary>
    public readonly struct DecorativeKey
    {
        public string Label { get; }

        public float X { get; }

        public float Y { get; }

        public float Width { get; }

        public float Height { get; }

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
            Width = Math.Max(Keys.Max(k => k.X + 1), Decorations.Max(d => d.X + d.Width));
            Height = Math.Max(Keys.Max(k => k.Y + 1), Decorations.Max(d => d.Y + d.Height));
        }

        public KeyDefinition this[KeyboardAction action] => byAction[action];

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
                // Backslash sits at the end of the top row; Enter is one wide flat key at the end of the home row.
                keys.Add(new KeyDefinition(KeyboardAction.BackSlash, InputKey.BackSlash, extra.ansiBackslash, 12f, 0, playable: false));
                decorations.Add(new DecorativeKey("Enter", 11.25f, 1, 1.75f, 1));
            }
            else
            {
                // The ISO key left of Z can be read; the extra key at the end of the home row cannot (osu!framework
                // does not map that scancode), so it is only drawn. Enter is one tall key spanning two rows.
                keys.Add(new KeyDefinition(KeyboardAction.NonUSBackSlash, InputKey.NonUSBackSlash, extra.isoLeftOfZ, shift + 0.75f - 1f, 2, playable: false));
                decorations.Add(new DecorativeKey(extra.isoHomeRowEnd, shift + 11.25f, 1, 1, 1));
                decorations.Add(new DecorativeKey("Enter", shift + 12.25f, 0, 1, 2));
            }

            return new KeyboardLayout(layoutType, keyboardType, keys, decorations);
        }
    }
}
