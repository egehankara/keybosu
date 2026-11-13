// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Keyboard.Configuration;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    /// <summary>Loads the real settings section and checks it is built from osu!'s current (form-style) settings controls.</summary>
    public partial class TestSceneSettings : OsuTestScene
    {
        // The settings overlay provides this in the game; the new form controls take their colours from it.
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private KeyboardSettingsSubsection section = null!;

        [Test]
        public void TestSettingsUseTheCurrentDesign()
        {
            AddStep("show settings", () => Child = section = new KeyboardSettingsSubsection(new KeyboardRuleset()));
            AddUntilStep("loaded", () => section.IsLoaded);
            AddAssert("five settings in osu!'s current settings style", () => section.ChildrenOfType<SettingsItemV2>().Count() == 5);
            AddAssert("feedback button is there and leads to the project", () =>
                section.ChildrenOfType<SettingsButtonV2>().Single().Enabled.Value && KeyboardRuleset.PROJECT_URL == "https://github.com/egehankara/keybosu");
            AddAssert("layouts are listed most used first", () =>
                section.ChildrenOfType<osu.Game.Graphics.UserInterfaceV2.FormEnumDropdown<KeyboardLayoutType>>().Single().Items.SequenceEqual(KeyboardLayout.BY_POPULARITY));
            AddAssert("no old-style settings left", () => !section.ChildrenOfType<SettingsCheckbox>().Any() && !section.ChildrenOfType<SettingsDropdown<KeyboardLayoutType>>().Any());
        }
    }
}
