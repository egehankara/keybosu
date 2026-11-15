// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Keyboard.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Keyboard.Objects.Drawables
{
    /// <summary>
    /// A square that grows inside its key. Press the key at the moment it fills the cell.
    /// </summary>
    public partial class DrawableKeyboardHitObject : DrawableHitObject<KeyboardHitObject>, IKeyBindingHandler<KeyboardAction>
    {
        private Container fill = null!;

        /// <summary>The coloured face of the square.</summary>
        private Box face = null!;

        /// <summary>White over the square, brightest at the exact moment to press.</summary>
        private Box glow = null!;

        /// <summary>How long before the note is due the square starts to brighten, in ms.</summary>
        public const double GLOW_LEAD_IN = 250;

        /// <summary>How bright the square gets at the exact moment (white over its colour).</summary>
        public const float GLOW_PEAK = 0.75f;

        public const float FILL_ALPHA = 0.85f;

        /// <summary>How see-through the square is once it is too late for anything but a Meh.</summary>
        public const float LATE_ALPHA = 0.35f;

        public float GlowAlpha => glow.Alpha;

        public float FillAlpha => fill.Alpha;

        protected override double InitialLifetimeOffset => HitObject.AppearOffset;

        public DrawableKeyboardHitObject(KeyboardHitObject hitObject)
            : base(hitObject)
        {
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(KeyboardPlayfield playfield)
        {
            // Where this note lives depends on the keyboard shape chosen in settings, which the playfield knows.
            var layout = playfield.Layout;
            var key = layout[HitObject.Key];

            // Absolute positions in the playfield's fixed keyboard space (not fractions), so effects that move notes
            // by pixels - like lazer's fail animation - move them a sensible distance.
            Size = new Vector2(KeyboardPlayfield.KEY_SIZE);
            Position = key.Centre * KeyboardPlayfield.KEY_SIZE;

            AddInternal(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(KeyBackground.CELL_PADDING),
                Child = fill = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    CornerRadius = 8,
                    Alpha = FILL_ALPHA,
                    Children = new Drawable[]
                    {
                        face = new Box { RelativeSizeAxes = Axes.Both },
                        glow = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.White,
                            Alpha = 0,
                        },
                    },
                }
            });
        }

        /// <summary>The colour the square is drawn in.</summary>
        public Colour4 NoteColour => face.Colour.TopLeft.SRGB;

        protected override void Update()
        {
            base.Update();

            // lazer keeps AccentColour set to the combo colour: from the map if "Beatmap colours" is on, otherwise from
            // the skin's skin.ini, and updates it when the skin changes. Read it every frame until the note is judged
            // (after that the hit / miss animation owns the colour).
            if (!Judged)
                face.Colour = AccentColour.Value;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            this.FadeInFromZero(150);
            // Constant growth speed: a note that has to appear late starts part-way full.
            fill.ScaleTo(Math.Max(0.02f, HitObject.InitialFill)).ScaleTo(1f, HitObject.AppearOffset);

            // Show the exact moment with the square itself. "Full" is hard to judge by size alone (the last few percent of
            // growth barely show), so the square also brightens towards the moment, faster and faster, and is brightest
            // exactly when the note is due. It can be read ahead, which matters: reacting to a signal at the moment itself
            // would always be late. Once the Great window is over the glow is gone and the square fades, down to
            // see-through by the end of the Ok window: a press now is late.
            var times = HighlightTimes(HitObject);

            fill.FadeTo(FILL_ALPHA);
            glow.FadeOut();

            using (BeginAbsoluteSequence(times.glowStart))
                glow.FadeTo(GLOW_PEAK, times.due - times.glowStart, Easing.In);

            using (BeginAbsoluteSequence(times.due))
                glow.FadeOut(times.greatEnd - times.due, Easing.Out);

            using (BeginAbsoluteSequence(times.greatEnd))
                fill.FadeTo(LATE_ALPHA, times.okEnd - times.greatEnd);
        }

        /// <summary>
        /// When the square starts to brighten, when it is brightest (the note is due), when the glow is gone (the Great
        /// window ends) and when the square has faded (the Ok window ends).
        /// </summary>
        public static (double glowStart, double due, double greatEnd, double okEnd) HighlightTimes(KeyboardHitObject note)
        {
            double due = note.StartTime;
            double great = note.HitWindows.WindowFor(HitResult.Great);
            double ok = note.HitWindows.WindowFor(HitResult.Ok);

            return (due - Math.Min(GLOW_LEAD_IN, note.AppearOffset), due, due + great, due + ok);
        }

        public bool OnPressed(KeyBindingPressEvent<KeyboardAction> e)
        {
            if (e.Action != HitObject.Key || Judged)
                return false;

            return UpdateResult(true);
        }

        public void OnReleased(KeyBindingReleaseEvent<KeyboardAction> e)
        {
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyMinResult();

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);

            if (result == HitResult.None)
                return;

            ApplyResult(result);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);

            switch (state)
            {
                case ArmedState.Hit:
                    this.ScaleTo(1.25f, 250, Easing.OutQuint);
                    this.FadeOut(250, Easing.OutQuint).Expire();
                    break;

                case ArmedState.Miss:
                    // A missed note keeps its own colour: turning red said nothing on a red note, and on any other
                    // colour it looked like a new red note for a moment. It shrinks away instead (a hit note grows),
                    // and the miss itself is shown by the judgement under the letter.
                    glow.FadeOut(100);
                    this.ScaleTo(0.8f, 400, Easing.OutQuint);
                    this.FadeOut(400, Easing.InQuint).Expire();
                    break;
            }
        }
    }
}
