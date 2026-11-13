// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Keyboard.Objects.Drawables;
using osu.Game.Rulesets.Keyboard.UI;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    [TestFixture]
    public partial class TestSceneColoursAndFail : PlayerTestScene
    {
        protected override bool HasCustomSteps => true;

        protected override bool AllowFail => true;

        protected override Ruleset CreatePlayerRuleset() => new KeyboardRuleset();

        // Three notes: A and S share a combo, D starts a new one.
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap<KeyboardHitObject>
        {
            BeatmapInfo = new BeatmapInfo { Ruleset = ruleset, Difficulty = new BeatmapDifficulty() },
            HitObjects =
            {
                new KeyboardHitObject { Key = KeyboardAction.A, StartTime = 3000, NewCombo = true },
                new KeyboardHitObject { Key = KeyboardAction.S, StartTime = 3100 },
                new KeyboardHitObject { Key = KeyboardAction.D, StartTime = 3200, NewCombo = true },
            },
        };

        private DrawableKeyboardHitObject note(KeyboardAction key) =>
            Player.DrawableRuleset.Playfield.AllHitObjects.OfType<DrawableKeyboardHitObject>().Single(n => n.HitObject.Key == key);

        private void play()
        {
            CreateTest();
            // AllHitObjects also lists notes that have not appeared yet; wait until all three are actually alive.
            AddUntilStep("all three notes on screen", () => Player.DrawableRuleset.Playfield.AllHitObjects.OfType<DrawableKeyboardHitObject>().Count(n => n.IsAlive && n.IsLoaded) == 3);
            AddWaitStep("a couple of frames", 2);
        }

        [Test]
        public void TestNotesUseComboColours()
        {
            play();

            AddStep("each note is drawn in its combo colour", () =>
            {
                foreach (var k in new[] { KeyboardAction.A, KeyboardAction.S, KeyboardAction.D })
                {
                    var n = note(k);
                    Assert.That(n.NoteColour, Is.EqualTo((Colour4)n.AccentColour.Value),
                        $"{k}: drawn {n.NoteColour}, accent {n.AccentColour.Value}, combo index {n.HitObject.ComboIndex}");
                }
            });
            AddAssert("same combo, same colour", () => note(KeyboardAction.A).NoteColour == note(KeyboardAction.S).NoteColour);
            AddAssert("new combo, next colour", () => note(KeyboardAction.D).NoteColour != note(KeyboardAction.A).NoteColour);
        }

        [Test]
        public void TestFailDropsNotesGently()
        {
            Vector2 before = Vector2.Zero;
            float keyHeight = 0;

            play();
            AddStep("remember where A is", () =>
            {
                before = note(KeyboardAction.A).ScreenSpaceDrawQuad.Centre;
                keyHeight = Player.DrawableRuleset.Playfield.ChildrenOfType<KeyBackground>().First(k => k.Action == KeyboardAction.A).ScreenSpaceDrawQuad.Height;
            });
            AddStep("fail", () => Player.HealthProcessor.TriggerFailure());
            AddUntilStep("failed", () => Player.GameplayState.HasFailed);
            AddUntilStep("note starts falling", () => note(KeyboardAction.A).ScreenSpaceDrawQuad.Centre.Y > before.Y + 1);
            AddWaitStep("let it fall for a while", 40);
            AddStep("it fell a few keys, not off into space", () =>
            {
                float fallenKeys = (note(KeyboardAction.A).ScreenSpaceDrawQuad.Centre.Y - before.Y) / keyHeight;
                Assert.That(fallenKeys, Is.InRange(0f, 4.5f), $"fell {fallenKeys:0.0} key heights");
            });
        }
    }
}
