// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Keyboard.Objects
{
    /// <summary>
    /// Made from osu! spinners (and similar long objects): hold Space for as much of its length as you can.
    /// </summary>
    public class KeyboardSpinner : KeyboardHitObject, IHasDuration
    {
        /// <summary>Share of the spinner that must be held for each result.</summary>
        public const double GREAT_RATIO = 0.9;

        public const double OK_RATIO = 0.6;
        public const double MEH_RATIO = 0.3;

        public KeyboardSpinner()
        {
            Key = KeyboardAction.Space;
        }

        public double Duration { get; set; }

        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public override Judgement CreateJudgement() => new KeyboardSpinnerJudgement();

        // No timing window: the result only depends on how long Space was held.
        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        /// <summary>The result for having held Space for <paramref name="ratio"/> (0 to 1) of the spinner.</summary>
        public static HitResult ResultFor(double ratio)
        {
            if (ratio >= GREAT_RATIO) return HitResult.Great;
            if (ratio >= OK_RATIO) return HitResult.Ok;
            if (ratio >= MEH_RATIO) return HitResult.Meh;

            return HitResult.Miss;
        }
    }

    public class KeyboardSpinnerJudgement : Judgement
    {
        public override HitResult MaxResult => HitResult.Great;
    }
}
