// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Keyboard.Localisation;

namespace osu.Game.Rulesets.Keyboard.Configuration
{
    /// <summary>
    /// The "Keyboard" section shown under Settings > Ruleset. Built the same way as osu!'s own ruleset settings
    /// (SettingsItemV2 around Form controls), so it gets the current settings look: rounded rows, toggle switches,
    /// a "?" hint and a revert-to-default button.
    /// </summary>
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
                new SettingsItemV2(new FormEnumDropdown<KeyboardLayoutType>
                {
                    Items = KeyboardLayout.BY_POPULARITY,
                    Caption = KeyboardStrings.KeyboardLayout,
                    HintText = KeyboardStrings.KeyboardLayoutTooltip,
                    Current = config.GetBindable<KeyboardLayoutType>(KeyboardRulesetSetting.Layout),
                }),
                new SettingsItemV2(new FormEnumDropdown<KeyboardType>
                {
                    Caption = KeyboardStrings.KeyboardType,
                    HintText = KeyboardStrings.KeyboardTypeTooltip,
                    Current = config.GetBindable<KeyboardType>(KeyboardRulesetSetting.KeyboardType),
                }),
                new SettingsItemV2(new FormEnumDropdown<KeyboardCentring>
                {
                    Caption = KeyboardStrings.ScreenCentre,
                    HintText = KeyboardStrings.ScreenCentreTooltip,
                    Current = config.GetBindable<KeyboardCentring>(KeyboardRulesetSetting.Centring),
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = KeyboardStrings.HighlightHomeRow,
                    HintText = KeyboardStrings.HighlightHomeRowTooltip,
                    Current = config.GetBindable<bool>(KeyboardRulesetSetting.HighlightHomeRow),
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = KeyboardStrings.ShowJudgements,
                    HintText = KeyboardStrings.ShowJudgementsTooltip,
                    Current = config.GetBindable<bool>(KeyboardRulesetSetting.ShowJudgements),
                }),
            };
        }
    }
}
