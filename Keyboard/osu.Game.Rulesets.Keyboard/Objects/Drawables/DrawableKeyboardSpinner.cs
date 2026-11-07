// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Keyboard.Localisation;
using osu.Game.Rulesets.Keyboard.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Keyboard.Objects.Drawables
{
    /// <summary>
    /// Sits on the Space bar. It fills up before the spinner starts (like a note), then a bar shows
    /// how much of the spinner Space has been held for.
    /// </summary>
    public partial class DrawableKeyboardSpinner : DrawableHitObject<KeyboardHitObject>, IKeyBindingHandler<KeyboardAction>
    {
        private Container approach = null!;
        private Box progress = null!;
        private SpriteText hint = null!;

        /// <summary>Milliseconds of the spinner during which Space was held.</summary>
        public double HeldTime { get; private set; }

        [Resolved]
        private KeyboardInputManager inputManager { get; set; } = null!;

        private KeyboardSpinner spinner => (KeyboardSpinner)HitObject;

        protected override double InitialLifetimeOffset => HitObject.TimePreempt;

        public DrawableKeyboardSpinner(KeyboardSpinner spinner)
            : base(spinner)
        {
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(KeyboardPlayfield playfield)
        {
            var layout = playfield.Layout;
            var key = layout[KeyboardAction.Space];

            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(key.Width / layout.Width, 1f / layout.Height);
            Position = new Vector2(key.Centre.X / layout.Width, key.Centre.Y / layout.Height);

            AddInternal(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(KeyBackground.CELL_PADDING),
                Child = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 8,
                    Children = new Drawable[]
                    {
                        approach = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = Colour4.FromHex("8866ee"),
                            Alpha = 0.5f,
                            Child = new Box { RelativeSizeAxes = Axes.Both },
                        },
                        progress = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0,
                            Colour = Colour4.FromHex("aaffcc"),
                            Alpha = 0.85f,
                        },
                        hint = new SpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = KeyboardStrings.HoldSpace,
                            Font = OsuFont.Default.With(size: 22, weight: FontWeight.Bold),
                            Colour = Color4.White,
                        },
                    }
                }
            });
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            this.FadeInFromZero(150);
            approach.ScaleTo(new Vector2(0.02f, 1)).ScaleTo(Vector2.One, HitObject.TimePreempt);
        }

        protected override void Update()
        {
            base.Update();

            if (Judged)
                return;

            // Read the held state directly, so holding Space before the spinner appears still counts.
            bool holding = inputManager.KeyBindingContainer.PressedActions.Contains(KeyboardAction.Space);

            if (holding && Time.Elapsed > 0)
            {
                double now = Time.Current;
                double previous = now - Time.Elapsed;
                double overlap = Math.Min(now, spinner.EndTime) - Math.Max(previous, HitObject.StartTime);

                if (overlap > 0)
                    HeldTime += overlap;
            }

            progress.Width = spinner.Duration > 0 ? (float)Math.Clamp(HeldTime / spinner.Duration, 0, 1) : 0;
            hint.Alpha = holding ? 0.5f : 1f;
        }

        public bool OnPressed(KeyBindingPressEvent<KeyboardAction> e)
        {
            // Swallow Space while the spinner is on screen, so it never reaches "skip intro".
            return e.Action == KeyboardAction.Space && !Judged;
        }

        public void OnReleased(KeyBindingReleaseEvent<KeyboardAction> e)
        {
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            // timeOffset is measured from the end of the spinner.
            if (timeOffset < 0)
                return;

            double ratio = spinner.Duration > 0 ? HeldTime / spinner.Duration : 1;
            ApplyResult(KeyboardSpinner.ResultFor(ratio));
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);

            switch (state)
            {
                case ArmedState.Hit:
                    this.FadeOut(300, Easing.OutQuint).Expire();
                    break;

                case ArmedState.Miss:
                    progress.FadeColour(Color4.Red, 200);
                    this.FadeOut(400, Easing.InQuint).Expire();
                    break;
            }
        }
    }
}
