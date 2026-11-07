// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
using osu.Game.Rulesets.Keyboard.Scoring;
using osu.Game.Rulesets.Keyboard.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Keyboard
{
    public partial class KeyboardRuleset : Ruleset
    {
        public override string Description => "Keyboard";

        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) =>
            new DrawableKeyboardRuleset(this, beatmap, mods);

        public override ScoreProcessor CreateScoreProcessor() => new KeyboardScoreProcessor(this);

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

        public override Drawable CreateIcon() => new Icon('K');

        public partial class Icon : CompositeDrawable
        {
            public Icon(char c)
            {
                InternalChildren = new Drawable[]
                {
                    new Circle
                    {
                        Size = new Vector2(20),
                        Colour = Color4.White,
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = c.ToString(),
                        Colour = Color4.Black,
                        Font = OsuFont.Default.With(size: 18)
                    }
                };
            }
        }

        // Leave this line intact. It will bake the correct version into the ruleset on each build/release.
        public override string RulesetAPIVersionSupported => CURRENT_RULESET_API_VERSION;
    }
}
