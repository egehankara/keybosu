// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Keyboard.Objects
{
    /// <summary>
    /// Timing windows (half-widths in ms around the note time). They tighten as the map's OD rises,
    /// using the same shape of curve as osu!mania. Change these numbers to make the mode stricter or more forgiving.
    /// </summary>
    public class KeyboardHitWindows : HitWindows
    {
        // Start out at OD 5 until SetDifficulty is called.
        private double perfect = 19.4;
        private double great = 49;
        private double good = 82;
        private double ok = 112;
        private double meh = 136;
        private double miss = 173;

        public override bool IsHitResultAllowed(HitResult result)
        {
            switch (result)
            {
                case HitResult.Perfect:
                case HitResult.Great:
                case HitResult.Good:
                case HitResult.Ok:
                case HitResult.Meh:
                case HitResult.Miss:
                    return true;
            }

            return false;
        }

        public override void SetDifficulty(double difficulty)
        {
            perfect = IBeatmapDifficultyInfo.DifficultyRange(difficulty, 22.4, 19.4, 13.9);
            great = IBeatmapDifficultyInfo.DifficultyRange(difficulty, 64, 49, 34);
            good = IBeatmapDifficultyInfo.DifficultyRange(difficulty, 97, 82, 67);
            ok = IBeatmapDifficultyInfo.DifficultyRange(difficulty, 127, 112, 97);
            meh = IBeatmapDifficultyInfo.DifficultyRange(difficulty, 151, 136, 121);
            miss = IBeatmapDifficultyInfo.DifficultyRange(difficulty, 188, 173, 158);
        }

        public override double WindowFor(HitResult result)
        {
            switch (result)
            {
                case HitResult.Perfect:
                    return perfect;

                case HitResult.Great:
                    return great;

                case HitResult.Good:
                    return good;

                case HitResult.Ok:
                    return ok;

                case HitResult.Meh:
                    return meh;

                case HitResult.Miss:
                    return miss;

                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }
    }
}
