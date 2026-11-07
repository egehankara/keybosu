// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets.Keyboard.Configuration
{
    public enum KeyboardRulesetSetting
    {
        Layout,
        KeyboardType,
    }

    /// <summary>Persistent, per-user settings for the keyboard ruleset.</summary>
    public class KeyboardRulesetConfigManager : RulesetConfigManager<KeyboardRulesetSetting>
    {
        public KeyboardRulesetConfigManager(SettingsStore? store, RulesetInfo ruleset, int? variant = null)
            : base(store!, ruleset, variant)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            SetDefault(KeyboardRulesetSetting.Layout, KeyboardLayoutType.EnglishQwerty);
            SetDefault(KeyboardRulesetSetting.KeyboardType, KeyboardType.Ansi);
        }
    }
}
