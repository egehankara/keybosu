// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Rulesets.Keyboard.UI
{
    /// <summary>
    /// Lights up a key while it is held. Sits above the hit objects and never swallows input.
    /// </summary>
    public partial class KeyPressHighlight : CompositeDrawable, IKeyBindingHandler<KeyboardAction>
    {
        private readonly KeyboardAction action;
        private readonly Container highlight;

        public KeyPressHighlight(KeyDefinition key, KeyboardLayout layout)
        {
            action = key.Action;

            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(key.Width / layout.Width, 1f / layout.Height);
            Position = new Vector2(key.X / layout.Width, key.Y / layout.Height);
            Padding = new MarginPadding(KeyBackground.CELL_PADDING);

            InternalChild = highlight = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 8,
                BorderThickness = 3,
                BorderColour = Colour4.White,
                Alpha = 0,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.White.Opacity(0.18f),
                }
            };
        }

        public bool OnPressed(KeyBindingPressEvent<KeyboardAction> e)
        {
            if (e.Action == action)
                highlight.FadeTo(1, 40);

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<KeyboardAction> e)
        {
            if (e.Action == action)
                highlight.FadeOut(200);
        }
    }
}
