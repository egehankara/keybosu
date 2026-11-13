// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace osu.Game.Rulesets.Keyboard.UI
{
    /// <summary>
    /// The face of one key: a dark rounded square (plus the bump on F and J). Drawn below the notes.
    /// The letter is drawn separately by <see cref="KeyLabel"/>, above the notes, so a filling square never hides it.
    /// Positioned using fractions of the keyboard so it scales with the screen.
    /// </summary>
    public partial class KeyBackground : CompositeDrawable
    {
        public const float CELL_PADDING = 3;

        /// <summary>Face colour of highlighted home row keys: a warm light grey, clearly lighter than the other keys.</summary>
        public static readonly Colour4 HOME_ROW_FACE = Colour4.FromHex("5a5650").Opacity(0.9f);

        /// <summary>The key this is drawn for, or null for shapes that are only drawn.</summary>
        public KeyboardAction? Action { get; }

        /// <summary>Whether this key is drawn as a home row key.</summary>
        public bool IsHomeRowHighlighted { get; }

        /// <summary>Whether this key has the index-finger bump (always on F and J, like a real keyboard).</summary>
        public bool HasBump { get; }

        public KeyBackground(KeyDefinition key, KeyboardLayout layout, bool highlightHomeRow = false)
            : this(key.X, key.Y, key.Width, 1, layout, !key.Playable, key.Action,
                highlightHomeRow && KeyboardLayout.HOME_ROW.Contains(key.Action),
                KeyboardLayout.INDEX_FINGER_BUMPS.Contains(key.Action))
        {
        }

        public KeyBackground(DecorativeKey key, KeyboardLayout layout)
            : this(key.X, key.Y, key.Width, key.Height, layout, true, null, false, false)
        {
        }

        private KeyBackground(float x, float y, float width, float height, KeyboardLayout layout, bool dim,
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
                // Home row keys get a lighter, neutral grey face, like the contrasting keycaps on real keyboards.
                // The edge stays the same as every other key: a bright edge looked like a pressed key, and colour is
                // kept for notes, so the home row never looks like either.
                BorderThickness = 2,
                BorderColour = ColourInfo.GradientVertical(Colour4.White.Opacity(0.25f), Colour4.White.Opacity(0.08f)),
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = homeRow ? HOME_ROW_FACE : Colour4.Black.Opacity(0.55f),
                },
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
    }
}
