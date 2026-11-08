// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.ClicksPerSecond;

namespace osu.Game.Rulesets.Keyboard
{
    // Cached so spinners can ask whether Space is already held when they appear.
    [Cached]
    public partial class KeyboardInputManager : RulesetInputManager<KeyboardAction>, ICanAttachHUDPieces
    {
        public KeyboardInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }

        // The keyboard on screen already shows every key. osu!'s key overlay would only add almost fifty counters
        // named "B1", "B2", ... on top of the combo, so it gets nothing to count.
        void ICanAttachHUDPieces.Attach(InputCountController inputCountController)
        {
        }

        void ICanAttachHUDPieces.Attach(ClicksPerSecondController controller) => Attach(controller);
    }

    /// <summary>
    /// One action per physical key. Actions are positions on the keyboard, not characters,
    /// so a different layout only changes where they are drawn and what they are labelled.
    /// </summary>
    public enum KeyboardAction
    {
        // IMPORTANT: lazer stores key bindings by these numbers. Never renumber or reorder existing members,
        // or saved bindings silently point at the wrong keys. Add new members with a new, unused number.

        // Top letter row.
        [Description("Q")]
        Q = 0,

        [Description("W")]
        W = 1,

        [Description("E")]
        E = 2,

        [Description("R")]
        R = 3,

        [Description("T")]
        T = 4,

        [Description("Y")]
        Y = 5,

        [Description("U")]
        U = 6,

        [Description("I")]
        I = 7,

        [Description("O")]
        O = 8,

        [Description("P")]
        P = 9,

        [Description("[")]
        BracketLeft = 10,

        [Description("]")]
        BracketRight = 11,

        // Home row.
        [Description("A")]
        A = 12,

        [Description("S")]
        S = 13,

        [Description("D")]
        D = 14,

        [Description("F")]
        F = 15,

        [Description("G")]
        G = 16,

        [Description("H")]
        H = 17,

        [Description("J")]
        J = 18,

        [Description("K")]
        K = 19,

        [Description("L")]
        L = 20,

        [Description(";")]
        Semicolon = 21,

        [Description("'")]
        Quote = 22,

        // Extra keys that only some keyboards have. Numbers are deliberately far from the letter keys.
        [Description("Backslash (ANSI keyboards)")]
        BackSlash = 40,

        [Description("Extra key left of Z (ISO keyboards)")]
        NonUSBackSlash = 41,

        // Held down for the whole length of a spinner.
        [Description("Space (spinners)")]
        Space = 42,

        // Bottom row.
        [Description("Z")]
        Z = 23,

        [Description("X")]
        X = 24,

        [Description("C")]
        C = 25,

        [Description("V")]
        V = 26,

        [Description("B")]
        B = 27,

        [Description("N")]
        N = 28,

        [Description("M")]
        M = 29,

        [Description(",")]
        Comma = 30,

        [Description(".")]
        Period = 31,

        [Description("/")]
        Slash = 32,
    }
}
