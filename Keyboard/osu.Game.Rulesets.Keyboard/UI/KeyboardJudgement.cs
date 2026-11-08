// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osuTK;

namespace osu.Game.Rulesets.Keyboard.UI
{
    /// <summary>
    /// osu!'s own skinnable judgement (Great, Ok, Meh, Miss - or your skin's hit300 / hit100 / hit50 / hit0 images),
    /// shrunk to fit inside a key and placed under its letter so it never covers the next notes.
    /// </summary>
    public partial class KeyboardJudgement : DrawableJudgement
    {
        /// <summary>How big the judgement is compared to one key.</summary>
        private const float size_in_keys = 0.75f;

        /// <summary>How long it stays fully visible, then how long it takes to fade, in ms. Kept short so the key is free again quickly.</summary>
        private const double hold_duration = 250;

        private const double fade_duration = 200;

        private float layoutWidth = 1;

        /// <summary>The key this judgement belongs to.</summary>
        public KeyboardAction Key { get; private set; }

        public void PlaceOn(KeyboardLayout layout, KeyDefinition key)
        {
            Key = key.Action;
            layoutWidth = layout.Width;

            RelativePositionAxes = Axes.Both;
            // Lower part of the key, below the letter (the letter sits in the middle).
            Position = new Vector2(key.Centre.X / layout.Width, (key.Y + 0.78f) / layout.Height);
        }

        protected override void Update()
        {
            base.Update();

            // DrawableJudgement is 128px; scale it to a fraction of one key at the current window size.
            if (Parent != null && Parent.DrawWidth > 0)
                Scale = new Vector2(Parent.DrawWidth / layoutWidth * size_in_keys / 128f);
        }

        protected override void ApplyHitAnimations()
        {
            base.ApplyHitAnimations();
            fadeOut();
        }

        protected override void ApplyMissAnimations()
        {
            base.ApplyMissAnimations();
            fadeOut();
        }

        private void fadeOut() => this.FadeInFromZero(40).Then().Delay(hold_duration).FadeOut(fade_duration);
    }
}
