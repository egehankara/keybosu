// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Keyboard.Beatmaps;
using osu.Game.Rulesets.Keyboard.Configuration;
using osu.Game.Rulesets.Keyboard.Mods;
using osu.Game.Rulesets.Keyboard.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Keyboard
{
    public partial class KeyboardRuleset : Ruleset
    {
        // The name shown in the game, exactly as written and never translated. ShortName below stays "keyboard":
        // lazer stores settings, key bindings and scores under it, so changing that would reset them.
        public override string Description => "keybosu!";

        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) =>
            new DrawableKeyboardRuleset(this, beatmap, mods);

        // Works out combo numbers from NewCombo, which is what the combo colours are picked by.
        public override IBeatmapProcessor CreateBeatmapProcessor(IBeatmap beatmap) => new BeatmapProcessor(beatmap);

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) =>
            new KeyboardBeatmapConverter(beatmap, this);

        public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) =>
            new KeyboardDifficultyCalculator(RulesetInfo, beatmap);

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[] { new KeyboardModNoFail(), new KeyboardModHalfTime() };

                case ModType.DifficultyIncrease:
                    return new Mod[] { new KeyboardModDoubleTime() };

                case ModType.Automation:
                    return new Mod[] { new KeyboardModAutoplay() };

                default:
                    return Array.Empty<Mod>();
            }
        }

        public override string ShortName => "keyboard";

        // Each action is bound to its own physical key, taken from the layout (including keys only some keyboards have).
        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0)
            => KeyboardLayout.AllBindableKeys.Select(k => new KeyBinding(k.Key, k.Action));

        public override IRulesetConfigManager CreateConfig(SettingsStore? settings) => new KeyboardRulesetConfigManager(settings, RulesetInfo);

        public override RulesetSettingsSubsection CreateSettings() => new KeyboardSettingsSubsection(this);

        public override Drawable CreateIcon() => new Icon();

        /// <summary>
        /// The ruleset icon, matching osu!'s own ruleset icons: a thick white ring with a solid white shape inside -
        /// here a key (a square with soft corners). Drawn at a fixed size; lazer scales it to wherever it is shown.
        /// </summary>
        public partial class Icon : CompositeDrawable
        {
            public const float SIZE = 100;

            /// <summary>Ring thickness as a share of the icon's size, measured from osu!'s own ruleset icons.</summary>
            public const float RING_THICKNESS = 0.13f;

            /// <summary>Size of the key inside, as a share of the icon's size.</summary>
            public const float KEY_SIZE = 0.4f;

            public Icon()
            {
                Size = new Vector2(SIZE);

                InternalChildren = new Drawable[]
                {
                    // Outer ring
                    new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        BorderThickness = SIZE * RING_THICKNESS,
                        BorderColour = Color4.White,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true,
                        },
                    },
                    // The key: a solid white square with soft corners
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(SIZE * KEY_SIZE),
                        Masking = true,
                        CornerRadius = SIZE * KEY_SIZE * 0.22f,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                        },
                    },
                };
            }
        }

        // Leave this line intact. It will bake the correct version into the ruleset on each build/release.
        public override string RulesetAPIVersionSupported => CURRENT_RULESET_API_VERSION;
    }
}
