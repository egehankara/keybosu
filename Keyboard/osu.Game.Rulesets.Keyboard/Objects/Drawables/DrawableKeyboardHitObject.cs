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
                    Alpha = 0.85f,
                    Child = new Box { RelativeSizeAxes = Axes.Both },
                }
            });
        }

        /// <summary>The colour the square is drawn in.</summary>
        public Colour4 NoteColour => fill.Colour.TopLeft.SRGB;

        protected override void Update()
        {
            base.Update();

            // lazer keeps AccentColour set to the combo colour: from the map if "Beatmap colours" is on, otherwise from
            // the skin's skin.ini, and updates it when the skin changes. Read it every frame until the note is judged
            // (after that the hit / miss animation owns the colour).
            if (!Judged)
                fill.Colour = AccentColour.Value;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            this.FadeInFromZero(150);
            // Constant growth speed: a note that has to appear late starts part-way full.
            fill.ScaleTo(Math.Max(0.02f, HitObject.InitialFill)).ScaleTo(1f, HitObject.AppearOffset);
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
