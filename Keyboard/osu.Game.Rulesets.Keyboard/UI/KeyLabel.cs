// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Keyboard.UI
{
    /// <summary>
    /// The letter printed on one key. Drawn in its own layer above the notes, so it always stays visible while a
    /// coloured square fills the key. A soft shadow keeps it readable on bright combo colours.
    /// </summary>
    public partial class KeyLabel : CompositeDrawable
    {
        /// <summary>The key this label belongs to, or null for shapes that are only drawn.</summary>
        public KeyboardAction? Action { get; }

        /// <summary>Whether the label is shown (hidden on the Space bar while a spinner covers it).</summary>
        public bool IsLabelVisible { get; private set; } = true;

        private readonly SpriteText label;
        private float lastUnit;

        public KeyLabel(KeyDefinition key, KeyboardLayout layout, bool highlightHomeRow = false)
            : this(key.DisplayLabel, key.X, key.Y, key.Width, 1, layout, !key.Playable, key.Action,
                highlightHomeRow && KeyboardLayout.HOME_ROW.Contains(key.Action))
        {
        }

        public KeyLabel(DecorativeKey key, KeyboardLayout layout)
            : this(key.DisplayLabel, key.X, key.Y, key.Width, key.Height, layout, true, null, false)
        {
        }

        private KeyLabel(LocalisableString text, float x, float y, float width, float height, KeyboardLayout layout, bool dim,
                         KeyboardAction? action, bool homeRow)
        {
            Action = action;

            // Keys that notes never land on are drawn dim, together with their face.
            if (dim)
                Alpha = 0.35f;

            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(width / layout.Width, height / layout.Height);
            Position = new Vector2(x / layout.Width, y / layout.Height);
            Padding = new MarginPadding(KeyBackground.CELL_PADDING);

            InternalChild = label = new SpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = text,
                Colour = Colour4.White.Opacity(homeRow ? 1f : 0.8f),
                Shadow = true,
                ShadowColour = Colour4.Black.Opacity(0.5f),
            };
        }

        public void SetLabelVisible(bool visible)
        {
            if (visible == IsLabelVisible)
                return;

            IsLabelVisible = visible;
            label.FadeTo(visible ? 1 : 0, 150, Easing.OutQuint);
        }

        protected override void Update()
        {
            base.Update();

            // Keep the label proportional to the key as the window is resized. Wide keys (Space) use their height.
            float unit = Math.Min(DrawWidth, DrawHeight);

            if (unit > 0 && unit != lastUnit)
            {
                lastUnit = unit;

                // Words like "Space" need a smaller font than single characters to fit.
                float scale = label.Text.ToString().Length > 1 ? 0.28f : 0.4f;
                label.Font = OsuFont.Default.With(size: unit * scale, weight: FontWeight.Bold);
            }
        }
    }
}
