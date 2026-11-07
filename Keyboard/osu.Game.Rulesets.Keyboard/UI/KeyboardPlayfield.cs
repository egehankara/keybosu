// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Keyboard.UI
{
    [Cached]
    public partial class KeyboardPlayfield : Playfield
    {
        public KeyboardLayout Layout { get; } = KeyboardLayout.Qwerty;

        [BackgroundDependencyLoader]
        private void load()
        {
            HitObjectContainer.RelativeSizeAxes = Axes.Both;

            // Everything lives in one box that keeps the keyboard's aspect ratio, and all children
            // are positioned in fractions of it. That way the whole thing scales to any window.
            AddRangeInternal(new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    FillAspectRatio = Layout.Width / Layout.Height,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            ChildrenEnumerable = Layout.Keys.Select(k => new KeyBackground(k, Layout)),
                        },
                        HitObjectContainer,
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            ChildrenEnumerable = Layout.Keys.Select(k => new KeyPressHighlight(k, Layout)),
                        },
                    }
                }
            });
        }
    }
}
