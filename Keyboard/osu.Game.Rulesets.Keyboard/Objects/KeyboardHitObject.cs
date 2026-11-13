// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Keyboard.Objects
{
    public class KeyboardHitObject : HitObject, IHasComboInformation
    {
        // Combo information, copied from the original map. lazer uses it to pick each note's colour from the
        // beatmap's colours (if "Beatmap colours" is on) or from the skin's skin.ini (Combo1, Combo2, ...).
        public bool NewCombo { get; set; }

        public int ComboOffset { get; set; }

        public Bindable<int> IndexInCurrentComboBindable { get; } = new Bindable<int>();

        public int IndexInCurrentCombo
        {
            get => IndexInCurrentComboBindable.Value;
            set => IndexInCurrentComboBindable.Value = value;
        }

        public Bindable<int> ComboIndexBindable { get; } = new Bindable<int>();

        public int ComboIndex
        {
            get => ComboIndexBindable.Value;
            set => ComboIndexBindable.Value = value;
        }

        public Bindable<int> ComboIndexWithOffsetsBindable { get; } = new Bindable<int>();

        public int ComboIndexWithOffsets
        {
            get => ComboIndexWithOffsetsBindable.Value;
            set => ComboIndexWithOffsetsBindable.Value = value;
        }

        public Bindable<bool> LastInComboBindable { get; } = new Bindable<bool>();

        public bool LastInCombo
        {
            get => LastInComboBindable.Value;
            set => LastInComboBindable.Value = value;
        }

        /// <summary>The key that must be pressed when this object's square is full.</summary>
        public KeyboardAction Key { get; set; }

        /// <summary>How long before <see cref="HitObject.StartTime"/> the square starts filling, from the map's approach rate.</summary>
        public double TimePreempt { get; set; } = 800;

        /// <summary>Keeps the speed osu!standard would fill this note at, instead of the slower default.</summary>
        public bool OriginalApproach { get; set; }

        private const double max_eased_approach = 1400;

        /// <summary>
        /// After a note on a key, how long that key needs before the next note on it may show up:
        /// covers the late hit window and the hit animation, so the key is completely empty again.
        /// </summary>
        public const double SAME_KEY_CLEARANCE = 200;

        /// <summary>How long the square takes to grow from empty to full: always the map's approach rate (<see cref="TimePreempt"/>).</summary>
        public double FillDuration => TimePreempt;

        /// <summary>When the previous note on the same key ends (set during conversion), or -infinity.</summary>
        public double PreviousOnSameKey { get; set; } = double.NegativeInfinity;

        /// <summary>
        /// How long before <see cref="HitObject.StartTime"/> the note actually appears. Normally the whole <see cref="FillDuration"/>;
        /// shorter if the key is still busy with the previous note. The square always grows at the same speed, so a note
        /// that appears late simply starts part-way full instead of growing faster.
        /// </summary>
        public double AppearOffset => Math.Max(50, Math.Min(FillDuration, StartTime - (PreviousOnSameKey + SAME_KEY_CLEARANCE)));

        /// <summary>How full the square already is when it appears (0 = empty, 1 = full).</summary>
        public float InitialFill => (float)Math.Clamp(1 - AppearOffset / FillDuration, 0, 1);

        public override Judgement CreateJudgement() => new KeyboardNoteJudgement();

        protected override HitWindows CreateHitWindows() => new KeyboardHitWindows();

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimePreempt = ApproachTime(difficulty.ApproachRate, OriginalApproach);
        }

        /// <summary>
        /// How long a note takes to fill, in ms. Finding a key among 33 takes longer than moving a cursor, so quick approach
        /// times are doubled, up to <see cref="max_eased_approach"/>. Maps that are already slower than that stay as they are.
        /// </summary>
        public static double ApproachTime(double approachRate, bool original)
        {
            // Same approach-rate curve as osu!standard.
            double time = IBeatmapDifficultyInfo.DifficultyRange(approachRate, 1800, 1200, 450);

            return original || time >= max_eased_approach ? time : Math.Min(time * 2, max_eased_approach);
        }
    }

    /// <summary>Notes are judged Great / Ok / Meh / Miss; Great is the best.</summary>
    public class KeyboardNoteJudgement : Judgement
    {
        public override HitResult MaxResult => HitResult.Great;
    }
}
