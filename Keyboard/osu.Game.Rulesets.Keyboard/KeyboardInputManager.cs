// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Keyboard
{
    public partial class KeyboardInputManager : RulesetInputManager<KeyboardAction>
    {
        public KeyboardInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }
    }

    /// <summary>
    /// One action per physical key. Actions are positions on the keyboard, not characters,
    /// so a different layout only changes where they are drawn and what they are labelled.
    /// </summary>
    public enum KeyboardAction
    {
        // Top letter row.
        [Description("Q")]
        Q,

        [Description("W")]
        W,

        [Description("E")]
        E,

        [Description("R")]
        R,

        [Description("T")]
        T,

        [Description("Y")]
        Y,

        [Description("U")]
        U,

        [Description("I")]
        I,

        [Description("O")]
        O,

        [Description("P")]
        P,

        [Description("[")]
        BracketLeft,

        [Description("]")]
        BracketRight,

        // Home row.
        [Description("A")]
        A,

        [Description("S")]
        S,

        [Description("D")]
        D,

        [Description("F")]
        F,

        [Description("G")]
        G,

        [Description("H")]
        H,

        [Description("J")]
        J,

        [Description("K")]
        K,

        [Description("L")]
        L,

        [Description(";")]
        Semicolon,

        [Description("'")]
        Quote,

        // Bottom row.
        [Description("Z")]
        Z,

        [Description("X")]
        X,

        [Description("C")]
        C,

        [Description("V")]
        V,

        [Description("B")]
        B,

        [Description("N")]
        N,

        [Description("M")]
        M,

        [Description(",")]
        Comma,

        [Description(".")]
        Period,

        [Description("/")]
        Slash,
    }
}
