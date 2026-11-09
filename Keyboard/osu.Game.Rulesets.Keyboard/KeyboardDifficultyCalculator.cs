// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Keyboard
{
    public class KeyboardDifficultyCalculator : DifficultyCalculator
    {
        private readonly IWorkingBeatmap workingBeatmap;

        public KeyboardDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
            workingBeatmap = beatmap;
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills)
        {
            var objects = beatmap.HitObjects;

            if (objects.Count < 2)
                return new DifficultyAttributes(mods, 0);

            // Rate mods (DoubleTime, HalfTime) squeeze or stretch the map in real time, so they change the rating.
            double rate = ModUtils.CalculateRateWithMods(mods);

            // Song select sorts by the map's own stored rating and rulesets cannot change that. Showing that same rating keeps
            // the list in the order of the numbers on it.
            double storedRating = workingBeatmap.BeatmapInfo.StarRating;

            if (storedRating >= 0)
            {
                return new DifficultyAttributes(mods, storedRating * Math.Pow(rate, 0.9))
                {
                    MaxCombo = objects.Count,
                };
            }

            // Not worked out by osu! yet: fall back to note density (notes per second).
            double seconds = Math.Max(1, (objects[^1].StartTime - objects[0].StartTime) / 1000 / rate);
            double notesPerSecond = objects.Count / seconds;

            return new DifficultyAttributes(mods, Math.Pow(notesPerSecond, 0.9) * 0.5)
            {
                MaxCombo = objects.Count,
            };
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, Mod[] mods) => Enumerable.Empty<DifficultyHitObject>();

        protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods) => Array.Empty<Skill>();
    }
}
