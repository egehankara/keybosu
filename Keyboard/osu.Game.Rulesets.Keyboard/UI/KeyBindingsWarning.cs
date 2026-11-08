// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings.Sections.Input;
using osu.Game.Rulesets.Keyboard.Localisation;

namespace osu.Game.Rulesets.Keyboard.UI
{
    /// <summary>A notice above this ruleset's list in osu!'s key bindings panel.</summary>
    public partial class KeyBindingsWarning : CompositeDrawable
    {
        private static readonly Colour4 colour = Colour4.FromHex("ffcc22");

        public KeyBindingsWarning()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Margin = new MarginPadding { Bottom = 5 };

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = SettingsPanel.CONTENT_PADDING, // lines the box up with the rows below it
                Child = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Masking = true,
                    CornerRadius = 5,
                    BorderThickness = 2,
                    BorderColour = colour,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colour,
                            Alpha = 0.15f,
                        },
                        new OsuTextFlowContainer(s =>
                        {
                            s.Font = OsuFont.GetFont(size: 13, weight: FontWeight.SemiBold);
                            s.Colour = colour;
                        })
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(10),
                            Text = KeyboardStrings.KeyBindingsHint,
                        },
                    },
                },
            };
        }

        /// <summary>
        /// osu! offers rulesets no way to add anything to the key bindings panel, but it does ask them for the icon shown in
        /// that panel's sidebar. So the icon looks for the panel it sits in; everywhere else this returns null.
        /// </summary>
        public static KeyBindingPanel? FindPanelOf(Drawable icon)
        {
            CompositeDrawable? parent = icon.Parent;

            while (parent != null && parent is not KeyBindingPanel)
                parent = parent.Parent;

            return parent as KeyBindingPanel;
        }

        /// <summary>
        /// Adds the warning above the ruleset's list. Returns false while the list is not ready yet, so the caller can try again.
        /// </summary>
        public static bool TryAddTo(KeyBindingPanel panel, string rulesetShortName)
        {
            var firstRow = panel.ChildrenOfType<VariantBindingsSubsection>()
                                .FirstOrDefault(s => s.Ruleset.ShortName == rulesetShortName)?
                                .ChildrenOfType<KeyBindingRow>()
                                .FirstOrDefault();

            if (firstRow?.Parent is not FlowContainer<Drawable> list || !list.IsLoaded)
                return false;

            list.Insert(-1, new KeyBindingsWarning());
            return true;
        }
    }
}
