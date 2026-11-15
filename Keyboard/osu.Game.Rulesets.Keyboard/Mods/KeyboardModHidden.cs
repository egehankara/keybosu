// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Keyboard.Localisation;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Keyboard.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Keyboard.Mods
{
    /// <summary>
    /// Notes fade out while their square is still filling, so they have to be pressed from memory - like osu!'s Hidden,
    /// where circles fade out before they are clicked. The first note stays visible, and spinners are not touched.
    /// </summary>
    public class KeyboardModHidden : ModHidden
    {
        /// <summary>The note starts fading this share of its fill time before it is due...</summary>
        public const double FADE_OUT_START = 0.6;

        /// <summary>...and is gone this share of its fill time before it is due.</summary>
        public const double FADE_OUT_END = 0.3;

        public override LocalisableString Description => KeyboardStrings.ModHiddenDescription;

        public override double ScoreMultiplier => 1.06;

        // Like osu!standard: the note left visible is the first real note, even when the map starts with a spinner.
        protected override bool IsFirstAdjustableObject(HitObject hitObject) => hitObject is not KeyboardSpinner;

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            if (hitObject is not DrawableKeyboardHitObject note)
                return;

            var (start, end) = FadeOutTimes(note.HitObject);

            using (note.BeginAbsoluteSequence(start))
                note.FadeOut(end - start);
        }

        /// <summary>
        /// When a note starts fading out and when it is gone. A note that appears late (its key was busy) is still
        /// shown for a moment, and fades out by the time it is due at the latest.
        /// </summary>
        public static (double start, double end) FadeOutTimes(KeyboardHitObject note)
        {
            double appear = note.StartTime - note.AppearOffset;
            double fade = note.FillDuration * (FADE_OUT_START - FADE_OUT_END);

            double end = Math.Min(note.StartTime, Math.Max(note.StartTime - note.FillDuration * FADE_OUT_END, appear + MIN_VISIBLE));
            double start = Math.Max(appear, end - fade);

            return (start, end);
        }

        /// <summary>A note is on screen at least this long (in ms) before it is gone.</summary>
        public const double MIN_VISIBLE = 150;
    }
}
