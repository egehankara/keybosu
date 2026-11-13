// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
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
    /// Sits on the Space bar. Before the spinner starts, a circle grows out from the middle until it fills the bar
    /// (like a note's square); during the spinner a brighter circle grows as Space is held. Both use the combo colour.
    /// </summary>
    public partial class DrawableKeyboardSpinner : DrawableHitObject<KeyboardHitObject>, IKeyBindingHandler<KeyboardAction>
    {
        // FastCircle draws a circle without masking of its own. A masked Circle inside the rounded bar would make
        // osu!framework ignore the bar's rounded corners, and the colour showed outside them.
        private FastCircle approach = null!;
        private FastCircle progress = null!;

        /// <summary>A circle this wide (the bar's diagonal) covers the whole bar, corners included.</summary>
        private float fullDiameter;
        private SpriteText hint = null!;

        /// <summary>Milliseconds of the spinner during which Space was held.</summary>
        public double HeldTime { get; private set; }

        /// <summary>Share of the spinner held so far, 0 to 1; the bright circle's size follows it.</summary>
        public float HeldShare => spinner.Duration > 0 ? (float)Math.Clamp(HeldTime / spinner.Duration, 0, 1) : 0;

        /// <summary>Diameter of the bright progress circle, for tests.</summary>
        public float ProgressDiameter => progress.Size.X;

        /// <summary>The diameter at which a circle covers the whole bar, for tests.</summary>
        public float FullDiameter => fullDiameter;

        /// <summary>The colour of the growing circle before the spinner starts, for tests.</summary>
        public Colour4 ApproachColour => approach.Colour.TopLeft.SRGB;

        [Resolved]
        private KeyboardInputManager inputManager { get; set; } = null!;

        private KeyboardSpinner spinner => (KeyboardSpinner)HitObject;

        protected override double InitialLifetimeOffset => HitObject.AppearOffset;

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

            // Absolute positions in the playfield's fixed keyboard space (see DrawableKeyboardHitObject).
            Size = new Vector2(key.Width, 1) * KeyboardPlayfield.KEY_SIZE;
            Position = key.Centre * KeyboardPlayfield.KEY_SIZE;
            fullDiameter = (Size - new Vector2(2 * KeyBackground.CELL_PADDING)).Length;

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
                        // Before the spinner: a soft circle grows out from the middle until it fills the bar.
                        approach = new FastCircle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0.5f,
                        },
                        // During the spinner: a bright circle grows out from the middle as Space is held.
                        progress = new FastCircle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = Vector2.Zero,
                            Alpha = 0.9f,
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
            approach.ResizeTo(Math.Max(0.02f, HitObject.InitialFill) * fullDiameter).ResizeTo(fullDiameter, HitObject.AppearOffset);
        }

        protected override void Update()
        {
            base.Update();

            if (Judged)
                return;

            // Rewound to before the spinner (seeking back in autoplay or a replay): start counting again from nothing.
            if (Time.Current < HitObject.StartTime)
                HeldTime = 0;

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

            progress.Size = new Vector2(HeldShare * fullDiameter);

            // Same colours as the notes: the combo colour from the beatmap ("Beatmap colours" on) or the skin's skin.ini.
            approach.Colour = AccentColour.Value;
            progress.Colour = AccentColour.Value.Lighten(0.6f);
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
