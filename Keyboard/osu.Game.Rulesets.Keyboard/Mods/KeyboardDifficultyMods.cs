// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Keyboard.Beatmaps;
using osu.Game.Rulesets.Keyboard.Localisation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Keyboard.Mods
{
    public class KeyboardModNoFail : ModNoFail
    {
    }

    /// <summary>
    /// Wider timing windows, slower filling squares, less health lost and extra lives, like osu!standard's Easy.
    /// osu!'s shared Easy halves approach rate and health drain; the timing windows (accuracy) are up to each ruleset.
    /// </summary>
    public class KeyboardModEasy : ModEasyWithExtraLives, IApplicableToBeatmapConverter
    {
        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter) => ((KeyboardBeatmapConverter)beatmapConverter).DifficultyMods.Add(this);

        public const float RATIO = 0.5f;

        public override LocalisableString Description => KeyboardStrings.ModEasyDescription;

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            base.ApplyToDifficulty(difficulty);

            difficulty.OverallDifficulty *= RATIO;
        }
    }

    /// <summary>
    /// Narrower timing windows, faster filling squares and more health lost, like osu!standard's Hard Rock (without the
    /// flip: that is Mirror here). osu!'s shared Hard Rock only raises health drain; the rest is up to each ruleset.
    /// </summary>
    public class KeyboardModHardRock : ModHardRock, IApplicableToBeatmapConverter
    {
        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter) => ((KeyboardBeatmapConverter)beatmapConverter).DifficultyMods.Add(this);

        public const float RATIO = 1.4f;

        public override void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            base.ApplyToDifficulty(difficulty);

            difficulty.OverallDifficulty = Math.Min(difficulty.OverallDifficulty * RATIO, 10.0f);
            difficulty.ApproachRate = Math.Min(difficulty.ApproachRate * RATIO, 10.0f);
        }
    }

    public class KeyboardModSuddenDeath : ModSuddenDeath
    {
    }

    public class KeyboardModPerfect : ModPerfect
    {
    }

    /// <summary>
    /// Sets the map's difficulty by hand. Besides health drain and accuracy (the timing windows), the approach rate
    /// sets how fast the squares fill. Circle size is left out: keys have a fixed size.
    /// </summary>
    public class KeyboardModDifficultyAdjust : ModDifficultyAdjust, IApplicableToBeatmapConverter
    {
        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter) => ((KeyboardBeatmapConverter)beatmapConverter).DifficultyMods.Add(this);

        [SettingSource("Approach Rate", "How fast the squares fill.", LAST_SETTING_ORDER + 1, SettingControlType = typeof(DifficultyAdjustSettingsControl))]
        public DifficultyBindable ApproachRate { get; } = new DifficultyBindable
        {
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 10,
            ExtendedMinValue = -10,
            ExtendedMaxValue = 11,
            ReadCurrentFromDifficulty = diff => diff.ApproachRate,
        };

        public override IEnumerable<(LocalisableString setting, LocalisableString value)> SettingDescription
        {
            get
            {
                if (!ApproachRate.IsDefault)
                    yield return ("Approach rate", $"{ApproachRate.Value:N1}");

                foreach (var setting in base.SettingDescription)
                    yield return setting;
            }
        }

        protected override void ApplySettings(BeatmapDifficulty difficulty)
        {
            base.ApplySettings(difficulty);

            if (ApproachRate.Value != null)
                difficulty.ApproachRate = ApproachRate.Value.Value;
        }
    }
}
