// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Keyboard.Beatmaps;
using osu.Game.Rulesets.Keyboard.Localisation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Keyboard.Mods
{
    public class KeyboardModOriginalDensity : Mod, IApplicableToBeatmapConverter
    {
        public override string Name => "Original Density";

        public override string Acronym => "OG";

        public override LocalisableString Description => KeyboardStrings.ModOriginalDensityDescription;

        public override ModType Type => ModType.DifficultyIncrease;

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
            => ((KeyboardBeatmapConverter)beatmapConverter).OriginalDensity = true;
    }
}
