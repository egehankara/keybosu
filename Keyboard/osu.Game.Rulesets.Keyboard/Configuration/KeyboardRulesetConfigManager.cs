// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Configuration;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Keyboard.Localisation;

namespace osu.Game.Rulesets.Keyboard.Configuration
{
    public enum KeyboardRulesetSetting
    {
        Layout,
        KeyboardType,
        HighlightHomeRow,
        Centring,
        ShowJudgements,
    }

    /// <summary>Which point of the keyboard sits exactly in the middle of the screen.</summary>
    public enum KeyboardCentring
    {
        [LocalisableDescription(typeof(KeyboardStrings), nameof(KeyboardStrings.CentreWholeKeyboard))]
        WholeKeyboard,

        [LocalisableDescription(typeof(KeyboardStrings), nameof(KeyboardStrings.CentreSpaceBar))]
        SpaceBar,

        // Shown as "between the index fingers": the boundary between G and H is exactly halfway between F and J,
        // and naming it by fingers is right on every layout (on Turkish F those keys say A and K).
        // The member name stays as it is because the saved setting refers to it.
        [LocalisableDescription(typeof(KeyboardStrings), nameof(KeyboardStrings.CentreBetweenIndexFingers))]
        BetweenGAndH,
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
            SetDefault(KeyboardRulesetSetting.HighlightHomeRow, true);
            SetDefault(KeyboardRulesetSetting.Centring, KeyboardCentring.WholeKeyboard);
            SetDefault(KeyboardRulesetSetting.ShowJudgements, true);
        }
    }
}
