// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Keyboard.Beatmaps;
using osu.Game.Rulesets.Keyboard.Localisation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Keyboard.Mods
{
    /// <summary>Flips the map left to right: notes from the left of the map land on the right of the keyboard.</summary>
    public class KeyboardModMirror : ModMirror, IApplicableToBeatmapConverter
    {
        public override LocalisableString Description => KeyboardStrings.ModMirrorDescription;

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
            => ((KeyboardBeatmapConverter)beatmapConverter).Mirror = true;
    }

    /// <summary>
    /// Every note goes to one hand's half of the keyboard (the whole map squeezed onto it). Spinners stay on Space.
    /// No icon, so the mod shows its acronym (LH / RH), like Original Density.
    /// </summary>
    public abstract class KeyboardModOneHand : Mod, IApplicableToBeatmapConverter
    {
        public abstract KeyboardHand Hand { get; }

        public override ModType Type => ModType.Conversion;

        // Only one hand can be picked, and the home row already has keys for both hands.
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(KeyboardModOneHand)).Append(typeof(KeyboardModHomeRow)).ToArray();

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
            => ((KeyboardBeatmapConverter)beatmapConverter).OneHand = Hand;
    }

    /// <summary>Every note goes to the home row: the eight keys the fingers rest on (A S D F and J K L ; on QWERTY).</summary>
    public class KeyboardModHomeRow : Mod, IApplicableToBeatmapConverter
    {
        public override string Name => "Home Row";

        // HR is Hard Rock.
        public override string Acronym => "HM";

        public override LocalisableString Description => KeyboardStrings.ModHomeRowDescription;

        public override ModType Type => ModType.Conversion;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(KeyboardModOneHand)).ToArray();

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
            => ((KeyboardBeatmapConverter)beatmapConverter).HomeRowOnly = true;
    }

    public class KeyboardModLeftHand : KeyboardModOneHand
    {
        public override string Name => "Left Hand";

        public override string Acronym => "LH";

        public override LocalisableString Description => KeyboardStrings.ModLeftHandDescription;

        public override KeyboardHand Hand => KeyboardHand.Left;
    }

    public class KeyboardModRightHand : KeyboardModOneHand
    {
        public override string Name => "Right Hand";

        public override string Acronym => "RH";

        public override LocalisableString Description => KeyboardStrings.ModRightHandDescription;

        public override KeyboardHand Hand => KeyboardHand.Right;
    }
}
