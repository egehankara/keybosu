// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        protected override double InitialLifetimeOffset => HitObject.TimePreempt;

        public DrawableKeyboardHitObject(KeyboardHitObject hitObject)
            : base(hitObject)
        {
            var layout = KeyboardLayout.Qwerty;
            var key = layout[hitObject.Key];

            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(1f / layout.Width, 1f / layout.Height);
            Origin = Anchor.Centre;
            Position = new Vector2(key.Centre.X / layout.Width, key.Centre.Y / layout.Height);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var key = KeyboardLayout.Qwerty[HitObject.Key];

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
                    // Colour follows the key's horizontal position, so the keyboard reads as a rainbow.
                    Colour = Colour4.FromHSV(key.X / KeyboardLayout.Qwerty.Width, 0.55f, 1f),
                    Alpha = 0.85f,
                    Child = new Box { RelativeSizeAxes = Axes.Both },
                }
            });
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            this.FadeInFromZero(150);
            fill.ScaleTo(0.02f).ScaleTo(1f, HitObject.TimePreempt);
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
                    fill.FadeColour(Color4.Red, 200);
                    this.FadeOut(400, Easing.InQuint).Expire();
                    break;
            }
        }
    }
}
