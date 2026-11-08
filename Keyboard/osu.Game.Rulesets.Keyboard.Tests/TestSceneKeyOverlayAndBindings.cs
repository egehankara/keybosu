// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings.Sections.Input;
using osu.Game.Rulesets.Keyboard.UI;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    [TestFixture]
    public partial class TestSceneKeyOverlayAndBindings : PlayerTestScene
    {
        protected override bool HasCustomSteps => true;

        protected override Ruleset CreatePlayerRuleset() => new KeyboardRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap<KeyboardHitObject>
        {
            BeatmapInfo = new BeatmapInfo { Ruleset = ruleset, Difficulty = new BeatmapDifficulty() },
            HitObjects = { new KeyboardHitObject { Key = KeyboardAction.A, StartTime = 1000 } },
        };

        [Test]
        public void TestKeyOverlayHasNothingToShow()
        {
            CreateTest();
            AddUntilStep("player loaded", () => Player.IsLoaded);
            AddAssert("no key counters", () => !Player.HUDOverlay.InputCountController.Triggers.Any());
        }

        [Test]
        public void TestKeyBindingsPanelShowsTheWarningFirst()
        {
            KeyBindingPanel panel = null!;

            AddStep("show key bindings", () => Child = panel = new KeyBindingPanel { State = { Value = Visibility.Visible } });
            AddUntilStep("warning comes first in the list", () =>
            {
                var warning = panel.ChildrenOfType<KeyBindingsWarning>().SingleOrDefault();

                // By layout position: the list only finishes loading what is added to it once it is scrolled into view.
                return warning?.Parent is FlowContainer<Drawable> list
                       && list.Children.OfType<KeyBindingRow>().All(r => list.GetLayoutPosition(warning) < list.GetLayoutPosition(r));
            });
        }

        [Test]
        public void TestBindingsAreStillRegistered()
        {
            var ruleset = new KeyboardRuleset();

            // osu! only stores the defaults of the listed variants: emptying this to hide the list would leave new installs without keys.
            Assert.That(ruleset.GameplayVariants, Does.Contain(0));
            Assert.That(ruleset.GetDefaultKeyBindings(0), Is.Not.Empty);
        }
    }
}
