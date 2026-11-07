// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
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

        /// <summary>The key this is drawn for, or null for shapes that are only drawn (Enter).</summary>
        public KeyboardAction? Action { get; }

        /// <summary>Whether this key is drawn as a home row key.</summary>
        public bool IsHomeRowHighlighted { get; }

        /// <summary>Whether this key has the index-finger bump.</summary>
        public bool HasBump { get; }

        private readonly SpriteText label;
        private float lastUnit;

        public KeyBackground(KeyDefinition key, KeyboardLayout layout, bool highlightHomeRow = false)
            : this(key.DisplayLabel, key.X, key.Y, key.Width, 1, layout, !key.Playable, key.Action,
                highlightHomeRow && KeyboardLayout.HOME_ROW.Contains(key.Action),
                highlightHomeRow && KeyboardLayout.INDEX_FINGER_BUMPS.Contains(key.Action))
        {
        }

        public KeyBackground(DecorativeKey key, KeyboardLayout layout)
            : this(key.DisplayLabel, key.X, key.Y, key.Width, key.Height, layout, true, null, false, false)
        {
        }

        private KeyBackground(LocalisableString text, float x, float y, float width, float height, KeyboardLayout layout, bool dim,
                              KeyboardAction? action, bool homeRow, bool bump)
        {
            Action = action;
            IsHomeRowHighlighted = homeRow;
            HasBump = bump;

            // Keys that notes never land on are drawn dim.
            if (dim)
                Alpha = 0.35f;

            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(width / layout.Width, height / layout.Height);
            Position = new Vector2(x / layout.Width, y / layout.Height);
            Padding = new MarginPadding(CELL_PADDING);

            var face = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 8,
                BorderThickness = homeRow ? 3 : 2,
                // Home row keys get a bright solid edge and a lighter face so fingers can find them at a glance.
                BorderColour = homeRow
                    ? ColourInfo.GradientVertical(Colour4.White.Opacity(0.85f), Colour4.White.Opacity(0.5f))
                    : ColourInfo.GradientVertical(Colour4.White.Opacity(0.25f), Colour4.White.Opacity(0.08f)),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = homeRow ? Colour4.FromHex("26304a").Opacity(0.85f) : Colour4.Black.Opacity(0.55f),
                    },
                    label = new SpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = text,
                        Colour = Colour4.White.Opacity(homeRow ? 1f : 0.7f),
                    },
                }
            };

            if (bump)
            {
                // The little ridge real keyboards have on F and J.
                face.Add(new Circle
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Y,
                    Size = new Vector2(0.32f, 0.06f),
                    Y = -0.12f,
                    Colour = Colour4.White.Opacity(0.9f),
                });
            }

            InternalChild = face;
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
