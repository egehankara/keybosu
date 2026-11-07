// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Keyboard.UI
{
    /// <summary>
    /// The resting look of one key: a dark rounded square with its label.
    /// Positioned using fractions of the playfield so it scales with the screen.
    /// </summary>
    public partial class KeyBackground : CompositeDrawable
    {
        public const float CELL_PADDING = 3;

        private readonly SpriteText label;
        private float lastHeight;

        public KeyBackground(KeyDefinition key, KeyboardLayout layout)
        {
            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(1f / layout.Width, 1f / layout.Height);
            Position = new Vector2(key.X / layout.Width, key.Y / layout.Height);
            Padding = new MarginPadding(CELL_PADDING);

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 8,
                BorderThickness = 2,
                BorderColour = ColourInfo.GradientVertical(Colour4.White.Opacity(0.25f), Colour4.White.Opacity(0.08f)),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.Black.Opacity(0.55f),
                    },
                    label = new SpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = key.Label,
                        Colour = Colour4.White.Opacity(0.7f),
                    },
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            // Keep the label proportional to the key as the window is resized.
            if (DrawHeight > 0 && DrawHeight != lastHeight)
            {
                lastHeight = DrawHeight;
                label.Font = OsuFont.Default.With(size: DrawHeight * 0.4f, weight: FontWeight.Bold);
            }
        }
    }
}
