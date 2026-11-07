// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Keyboard.Objects
{
    public class KeyboardHitObject : HitObject
    {
        /// <summary>The key that must be pressed when this object's square is full.</summary>
        public KeyboardAction Key { get; set; }

        /// <summary>How long before <see cref="HitObject.StartTime"/> the square starts filling.</summary>
        public double TimePreempt { get; set; } = 800;

        public override Judgement CreateJudgement() => new Judgement();

        protected override HitWindows CreateHitWindows() => new KeyboardHitWindows();

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            // Same approach-rate curve as osu!standard, so converted maps keep their intended reading speed.
            TimePreempt = IBeatmapDifficultyInfo.DifficultyRange(difficulty.ApproachRate, 1800, 1200, 450);
        }
    }
}
