// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    /// <summary>osu!'s own mod select screen shows every mod in its own column, one panel each.</summary>
    [TestFixture]
    public partial class TestSceneModSelect : OsuManualInputManagerTestScene
    {
        private UserModSelectOverlay overlay = null!;

        [Test]
        public void TestEveryModHasAPanelInItsColumn()
        {
            AddStep("pick keybosu!", () => Ruleset.Value = new KeyboardRuleset().RulesetInfo);
            AddStep("show mod select", () => Child = overlay = new UserModSelectOverlay { State = { Value = Visibility.Visible } });
            AddUntilStep("panels loaded", () => overlay.ChildrenOfType<ModPanel>().Count() >= 20);

            string[] acronymsIn(ModType type) => overlay.ChildrenOfType<ModColumn>().Single(c => c.ModType == type)
                                                        .ChildrenOfType<ModPanel>().Select(p => p.Mod.Acronym).ToArray();

            AddAssert("difficulty reduction", () => acronymsIn(ModType.DifficultyReduction), () => Is.EqualTo(new[] { "EZ", "NF", "HT", "DC" }));
            AddAssert("difficulty increase", () => acronymsIn(ModType.DifficultyIncrease), () => Is.EqualTo(new[] { "HR", "SD", "PF", "DT", "NC", "HD", "OG" }));
            AddAssert("conversion", () => acronymsIn(ModType.Conversion), () => Is.EqualTo(new[] { "DA", "MR", "LH", "RH", "HM" }));
            AddAssert("automation", () => acronymsIn(ModType.Automation), () => Is.EqualTo(new[] { "AT", "CN" }));
            AddAssert("fun", () => acronymsIn(ModType.Fun), () => Is.EqualTo(new[] { "WU", "WD", "MU" }));
        }
    }
}
