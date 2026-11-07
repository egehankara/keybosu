// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.Bindings;
using osuTK;

namespace osu.Game.Rulesets.Keyboard
{
    /// <summary>
    /// A single key on the on-screen keyboard. Positions are in "key units" (one key is 1x1).
    /// </summary>
    public readonly struct KeyDefinition
    {
        public KeyboardAction Action { get; }

        public InputKey Key { get; }

        public string Label { get; }

        /// <summary>Left edge, in key units.</summary>
        public float X { get; }

        /// <summary>Top edge, in key units (equal to the row index).</summary>
        public float Y { get; }

        public Vector2 Centre => new Vector2(X + 0.5f, Y + 0.5f);

        public KeyDefinition(KeyboardAction action, InputKey key, string label, float x, float y)
        {
            Action = action;
            Key = key;
            Label = label;
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// Describes where every key sits on screen. This is deliberately plain data so that
    /// further layouts can be added later and chosen from settings.
    /// </summary>
    public class KeyboardLayout
    {
        public string Name { get; }

        public IReadOnlyList<KeyDefinition> Keys { get; }

        /// <summary>Total width in key units.</summary>
        public float Width { get; }

        /// <summary>Total height in key units.</summary>
        public float Height { get; }

        private readonly Dictionary<KeyboardAction, KeyDefinition> byAction;

        public KeyboardLayout(string name, IEnumerable<KeyDefinition> keys)
        {
            Name = name;
            Keys = keys.ToList();
            byAction = Keys.ToDictionary(k => k.Action);
            Width = Keys.Max(k => k.X + 1);
            Height = Keys.Max(k => k.Y + 1);
        }

        public KeyDefinition this[KeyboardAction action] => byAction[action];

        /// <summary>Standard English (US) QWERTY: letters plus the punctuation keys beside them.</summary>
        public static KeyboardLayout Qwerty { get; } = createQwerty();

        private static KeyboardLayout createQwerty()
        {
            var keys = new List<KeyDefinition>();

            void addRow(int y, float offset, params (KeyboardAction action, InputKey key, string label)[] row)
            {
                for (int i = 0; i < row.Length; i++)
                    keys.Add(new KeyDefinition(row[i].action, row[i].key, row[i].label, offset + i, y));
            }

            addRow(0, 0f,
                (KeyboardAction.Q, InputKey.Q, "Q"),
                (KeyboardAction.W, InputKey.W, "W"),
                (KeyboardAction.E, InputKey.E, "E"),
                (KeyboardAction.R, InputKey.R, "R"),
                (KeyboardAction.T, InputKey.T, "T"),
                (KeyboardAction.Y, InputKey.Y, "Y"),
                (KeyboardAction.U, InputKey.U, "U"),
                (KeyboardAction.I, InputKey.I, "I"),
                (KeyboardAction.O, InputKey.O, "O"),
                (KeyboardAction.P, InputKey.P, "P"),
                (KeyboardAction.BracketLeft, InputKey.BracketLeft, "["),
                (KeyboardAction.BracketRight, InputKey.BracketRight, "]"));

            addRow(1, 0.25f,
                (KeyboardAction.A, InputKey.A, "A"),
                (KeyboardAction.S, InputKey.S, "S"),
                (KeyboardAction.D, InputKey.D, "D"),
                (KeyboardAction.F, InputKey.F, "F"),
                (KeyboardAction.G, InputKey.G, "G"),
                (KeyboardAction.H, InputKey.H, "H"),
                (KeyboardAction.J, InputKey.J, "J"),
                (KeyboardAction.K, InputKey.K, "K"),
                (KeyboardAction.L, InputKey.L, "L"),
                (KeyboardAction.Semicolon, InputKey.Semicolon, ";"),
                (KeyboardAction.Quote, InputKey.Quote, "'"));

            addRow(2, 0.75f,
                (KeyboardAction.Z, InputKey.Z, "Z"),
                (KeyboardAction.X, InputKey.X, "X"),
                (KeyboardAction.C, InputKey.C, "C"),
                (KeyboardAction.V, InputKey.V, "V"),
                (KeyboardAction.B, InputKey.B, "B"),
                (KeyboardAction.N, InputKey.N, "N"),
                (KeyboardAction.M, InputKey.M, "M"),
                (KeyboardAction.Comma, InputKey.Comma, ","),
                (KeyboardAction.Period, InputKey.Period, "."),
                (KeyboardAction.Slash, InputKey.Slash, "/"));

            return new KeyboardLayout("QWERTY (US)", keys);
        }
    }
}
