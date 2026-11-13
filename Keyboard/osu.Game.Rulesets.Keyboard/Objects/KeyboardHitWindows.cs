// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Keyboard.Objects
{
    /// <summary>
    /// Timing windows (half-widths in ms around the note time): Great, Ok, Meh and Miss - exactly osu!standard's
    /// (osu.Game.Rulesets.Osu.Scoring.OsuHitWindows): same OD curves, same rounding, same fixed 400ms early-press Miss.
    /// Every legacy skin has images for exactly these four results (hit300, hit100, hit50, hit0).
    /// </summary>
    public class KeyboardHitWindows : HitWindows
    {
        /// <summary>Pressing this early (or less) but outside Meh is a Miss, like osu!standard.</summary>
        public const double MISS_WINDOW = 400;

        // Start out at OD 5 until SetDifficulty is called.
        private double great = 49.5;
        private double ok = 99.5;
        private double meh = 149.5;

        public override bool IsHitResultAllowed(HitResult result)
        {
            switch (result)
            {
                case HitResult.Great:
                case HitResult.Ok:
                case HitResult.Meh:
                case HitResult.Miss:
                    return true;
            }

            return false;
        }

        public override void SetDifficulty(double difficulty)
        {
            // Same numbers and rounding as osu!standard: whole milliseconds, minus half a millisecond.
            great = Math.Floor(IBeatmapDifficultyInfo.DifficultyRange(difficulty, 80, 50, 20)) - 0.5;
            ok = Math.Floor(IBeatmapDifficultyInfo.DifficultyRange(difficulty, 140, 100, 60)) - 0.5;
            meh = Math.Floor(IBeatmapDifficultyInfo.DifficultyRange(difficulty, 200, 150, 100)) - 0.5;
        }

        public override double WindowFor(HitResult result)
        {
            switch (result)
            {
                case HitResult.Great:
                    return great;

                case HitResult.Ok:
                    return ok;

                case HitResult.Meh:
                    return meh;

                case HitResult.Miss:
                    return MISS_WINDOW;

                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }
    }
}
