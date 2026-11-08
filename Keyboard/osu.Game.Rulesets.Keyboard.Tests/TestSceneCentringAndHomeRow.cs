// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Keyboard.Configuration;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Keyboard.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    /// <summary>
    /// Loads real plays and measures on screen (in pixels) where the keyboard ends up.
    /// </summary>
    [TestFixture]
    public partial class TestSceneCentringAndHomeRow : PlayerTestScene
    {
        protected override bool HasCustomSteps => true;

        protected override Ruleset CreatePlayerRuleset() => new KeyboardRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap<KeyboardHitObject>
        {
            BeatmapInfo = new BeatmapInfo { Ruleset = ruleset, Difficulty = new BeatmapDifficulty() },
            HitObjects = { new KeyboardHitObject { Key = KeyboardAction.A, StartTime = 1000 } },
        };

        private void play(KeyboardLayoutType layout, KeyboardType type, KeyboardCentring centring, bool homeRow = true)
        {
            CreateTest(() => AddStep($"{layout} / {type} / {centring} / home row {homeRow}", () =>
            {
                var config = (KeyboardRulesetConfigManager)RulesetConfigs.GetConfigFor(new KeyboardRuleset())!;
                config.SetValue(KeyboardRulesetSetting.Layout, layout);
                config.SetValue(KeyboardRulesetSetting.KeyboardType, type);
                config.SetValue(KeyboardRulesetSetting.Centring, centring);
                config.SetValue(KeyboardRulesetSetting.HighlightHomeRow, homeRow);
            }));
            AddUntilStep("keys drawn", () => keys.Any() && keys.All(k => k.IsLoaded && k.DrawWidth > 0));
        }

        private IEnumerable<KeyBackground> keys => Player.DrawableRuleset.Playfield.ChildrenOfType<KeyBackground>();

        private float screenCentreX => Player.ScreenSpaceDrawQuad.Centre.X;

        private static IEnumerable<TestCaseData> combinations()
        {
            foreach (var (layout, type) in new[]
                     {
                         (KeyboardLayoutType.EnglishQwerty, KeyboardType.Ansi),
                         (KeyboardLayoutType.TurkishQ, KeyboardType.Iso),
                         (KeyboardLayoutType.TurkishF, KeyboardType.Ansi),
                     })
            {
                foreach (var centring in Enum.GetValues<KeyboardCentring>())
                    yield return new TestCaseData(layout, type, centring).SetName($"{layout}_{type}_{centring}");
            }
        }

        [TestCaseSource(nameof(combinations))]
        public void TestChosenPointIsInTheMiddleOfTheScreen(KeyboardLayoutType layout, KeyboardType type, KeyboardCentring centring)
        {
            play(layout, type, centring);

            AddStep("measure", () =>
            {
                var all = keys.ToList();
                float pointX;

                switch (centring)
                {
                    case KeyboardCentring.BetweenGAndH:
                        // H's left edge is exactly the boundary between G and H.
                        float hLeft = all.Single(k => k.Action == KeyboardAction.H).ScreenSpaceDrawQuad.TopLeft.X;
                        float gRight = all.Single(k => k.Action == KeyboardAction.G).ScreenSpaceDrawQuad.TopRight.X;
                        Assert.That(Math.Abs(hLeft - gRight), Is.LessThan(0.5f), "G and H should touch");
                        pointX = hLeft;
                        break;

                    case KeyboardCentring.SpaceBar:
                        pointX = all.Single(k => k.Action == KeyboardAction.Space).ScreenSpaceDrawQuad.Centre.X;
                        break;

                    default:
                        pointX = (all.Min(k => k.ScreenSpaceDrawQuad.TopLeft.X) + all.Max(k => k.ScreenSpaceDrawQuad.TopRight.X)) / 2;
                        break;
                }

                Assert.That(pointX, Is.EqualTo(screenCentreX).Within(1f), $"{centring} point is {pointX - screenCentreX:0.0}px off centre");

                // At least the side margin of empty space on both sides (within a pixel).
                var screen = Player.ScreenSpaceDrawQuad;
                float margin = screen.Width * KeyboardPlayfield.SIDE_MARGIN;
                float leftGap = all.Min(k => k.ScreenSpaceDrawQuad.TopLeft.X) - screen.TopLeft.X;
                float rightGap = screen.TopRight.X - all.Max(k => k.ScreenSpaceDrawQuad.TopRight.X);
                Assert.That(leftGap, Is.GreaterThanOrEqualTo(margin - 1), $"only {leftGap:0}px free on the left");
                Assert.That(rightGap, Is.GreaterThanOrEqualTo(margin - 1), $"only {rightGap:0}px free on the right");
            });
        }

        [Test]
        public void TestLettersAreDrawnAboveTheNotes()
        {
            play(KeyboardLayoutType.TurkishQ, KeyboardType.Iso, KeyboardCentring.WholeKeyboard);

            AddAssert("every key and shape has a letter", () =>
            {
                var layout = ((KeyboardPlayfield)Player.DrawableRuleset.Playfield).Layout;
                return Player.DrawableRuleset.Playfield.ChildrenOfType<KeyLabel>().Count() == layout.Keys.Count + layout.Decorations.Count;
            });
            AddAssert("the letter layer comes after (on top of) the notes", () =>
            {
                var notes = Player.DrawableRuleset.Playfield.HitObjectContainer;
                var letters = Player.DrawableRuleset.Playfield.ChildrenOfType<KeyLabel>().First().Parent!;
                var space = (osu.Framework.Graphics.Containers.Container<osu.Framework.Graphics.Drawable>)notes.Parent!;

                return letters.Parent == space && space.IndexOf(letters) > space.IndexOf(notes);
            });
        }

        [Test]
        public void TestHomeRowIsHighlightedWhenOn()
        {
            play(KeyboardLayoutType.TurkishQ, KeyboardType.Iso, KeyboardCentring.WholeKeyboard);

            AddAssert("exactly the eight home row keys are highlighted", () =>
                keys.Where(k => k.IsHomeRowHighlighted).Select(k => k.Action!.Value).OrderBy(a => a).SequenceEqual(KeyboardLayout.HOME_ROW.OrderBy(a => a)));
            AddAssert("bumps on the two index-finger keys only", () =>
                keys.Where(k => k.HasBump).Select(k => k.Action!.Value).OrderBy(a => a).SequenceEqual(new[] { KeyboardAction.F, KeyboardAction.J }));
        }

        [Test]
        public void TestHomeRowIsPlainWhenOff()
        {
            play(KeyboardLayoutType.TurkishQ, KeyboardType.Iso, KeyboardCentring.WholeKeyboard, homeRow: false);

            AddAssert("no key is highlighted", () => !keys.Any(k => k.IsHomeRowHighlighted));
            AddAssert("F and J still have their bumps", () =>
                keys.Where(k => k.HasBump).Select(k => k.Action!.Value).OrderBy(a => a).SequenceEqual(new[] { KeyboardAction.F, KeyboardAction.J }));
        }

        [Test]
        public void TestHomeRowFollowsPhysicalKeysNotLetters()
        {
            // On Turkish Q the key right of L shows "Ş" - it must still be part of the home row.
            var layout = KeyboardLayout.Create(KeyboardLayoutType.TurkishQ, KeyboardType.Iso);
            AddAssert("Ş is a home row key", () => KeyboardLayout.HOME_ROW.Contains(layout.Keys.Single(k => k.Label == "Ş").Action));
        }
    }
}
