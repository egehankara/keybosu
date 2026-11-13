// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Localisation;
using osu.Game.Rulesets.Keyboard.Objects;

namespace osu.Game.Rulesets.Keyboard.Beatmaps
{
    public class KeyboardBeatmap : Beatmap<KeyboardHitObject>
    {
        // Shown at song select. Counts what is actually played: one slider is usually several notes here.
        public override IEnumerable<BeatmapStatistic> GetStatistics()
        {
            int spinners = HitObjects.Count(h => h is KeyboardSpinner);
            int notes = HitObjects.Count - spinners;

            return new[]
            {
                new BeatmapStatistic
                {
                    Name = BeatmapStatisticStrings.Notes,
                    Content = notes.ToString(),
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Circles),
                    BarDisplayLength = notes > 0 ? 1 : 0,
                },
                new BeatmapStatistic
                {
                    Name = BeatmapStatisticStrings.Spinners,
                    Content = spinners.ToString(),
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Spinners),
                    BarDisplayLength = Math.Min(spinners / 10f, 1),
                },
            };
        }
    }
}
