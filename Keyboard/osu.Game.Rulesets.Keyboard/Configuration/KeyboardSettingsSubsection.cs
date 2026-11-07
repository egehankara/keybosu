// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Keyboard.Localisation;

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
                    LabelText = KeyboardStrings.KeyboardLayout,
                    TooltipText = KeyboardStrings.KeyboardLayoutTooltip,
                    Current = config.GetBindable<KeyboardLayoutType>(KeyboardRulesetSetting.Layout),
                },
                new SettingsEnumDropdown<KeyboardType>
                {
                    LabelText = KeyboardStrings.KeyboardType,
                    TooltipText = KeyboardStrings.KeyboardTypeTooltip,
                    Current = config.GetBindable<KeyboardType>(KeyboardRulesetSetting.KeyboardType),
                },
                new SettingsCheckbox
                {
                    LabelText = KeyboardStrings.HighlightHomeRow,
                    TooltipText = KeyboardStrings.HighlightHomeRowTooltip,
                    Current = config.GetBindable<bool>(KeyboardRulesetSetting.HighlightHomeRow),
                },
                new SettingsCheckbox
                {
                    LabelText = KeyboardStrings.ShowJudgements,
                    TooltipText = KeyboardStrings.ShowJudgementsTooltip,
                    Current = config.GetBindable<bool>(KeyboardRulesetSetting.ShowJudgements),
                },
                new SettingsEnumDropdown<KeyboardCentring>
                {
                    LabelText = KeyboardStrings.ScreenCentre,
                    TooltipText = KeyboardStrings.ScreenCentreTooltip,
                    Current = config.GetBindable<KeyboardCentring>(KeyboardRulesetSetting.Centring),
                },
            };
        }
    }
}
