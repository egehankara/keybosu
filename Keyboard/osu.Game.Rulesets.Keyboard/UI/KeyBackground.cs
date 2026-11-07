// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
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
        private float lastUnit;

        public KeyBackground(KeyDefinition key, KeyboardLayout layout)
            : this(key.DisplayLabel, key.X, key.Y, key.Width, 1, layout, !key.Playable)
        {
        }

        public KeyBackground(DecorativeKey key, KeyboardLayout layout)
            : this(key.DisplayLabel, key.X, key.Y, key.Width, key.Height, layout, true)
        {
        }

        private KeyBackground(LocalisableString text, float x, float y, float width, float height, KeyboardLayout layout, bool dim)
        {
            // Keys that notes never land on are drawn dim.
            if (dim)
                Alpha = 0.35f;

            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(width / layout.Width, height / layout.Height);
            Position = new Vector2(x / layout.Width, y / layout.Height);
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
                        Text = text,
                        Colour = Colour4.White.Opacity(0.7f),
                    },
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            // Keep the label proportional to the key as the window is resized. Tall or wide keys use their smaller side.
            float unit = Math.Min(DrawWidth, DrawHeight);

            if (unit > 0 && unit != lastUnit)
            {
                lastUnit = unit;

                // Words like "Enter" need a smaller font than single characters to fit.
                float scale = label.Text.ToString().Length > 1 ? 0.28f : 0.4f;
                label.Font = OsuFont.Default.With(size: unit * scale, weight: FontWeight.Bold);
            }
        }
    }
}
