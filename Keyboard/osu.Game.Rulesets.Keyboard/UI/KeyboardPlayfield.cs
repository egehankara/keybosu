// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Keyboard.Configuration;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Keyboard.UI
{
    [Cached]
    public partial class KeyboardPlayfield : Playfield
    {
        /// <summary>
        /// The layout chosen in settings. It is fixed for the whole play: changing the setting applies to the next map.
        /// Hit objects read this to know where their key is.
        /// </summary>
        public KeyboardLayout Layout { get; private set; } = KeyboardLayout.Default;

        [BackgroundDependencyLoader]
        private void load(KeyboardRulesetConfigManager config)
        {
            Layout = KeyboardLayout.Create(
                config.Get<KeyboardLayoutType>(KeyboardRulesetSetting.Layout),
                config.Get<KeyboardType>(KeyboardRulesetSetting.KeyboardType));

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
                            Children = new Drawable[]
                            {
                                // Enter and other shapes that are only drawn, then the real keys.
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ChildrenEnumerable = Layout.Decorations.Select(d => new KeyBackground(d, Layout)),
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ChildrenEnumerable = Layout.Keys.Select(k => new KeyBackground(k, Layout)),
                                },
                            }
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
