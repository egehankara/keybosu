// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;

namespace osu.Game.Rulesets.Keyboard.Configuration
{
    /// <summary>The "Keyboard" section shown under Settings > Ruleset.</summary>
    public partial class KeyboardSettingsSubsection : RulesetSettingsSubsection
    {
        public KeyboardSettingsSubsection(KeyboardRuleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (KeyboardRulesetConfigManager)Config;

            Children = new Drawable[]
            {
                new SettingsEnumDropdown<KeyboardLayoutType>
                {
                    LabelText = "Keyboard layout",
                    TooltipText = "What is printed on the keys. Applies the next time a map starts.",
                    Current = config.GetBindable<KeyboardLayoutType>(KeyboardRulesetSetting.Layout),
                },
                new SettingsEnumDropdown<KeyboardType>
                {
                    LabelText = "Keyboard type",
                    TooltipText = "The shape of your keyboard: flat Enter (ANSI) or tall Enter with an extra key left of Z (ISO). Applies the next time a map starts.",
                    Current = config.GetBindable<KeyboardType>(KeyboardRulesetSetting.KeyboardType),
                },
            };
        }
    }
}
