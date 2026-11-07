// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Keyboard.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    /// <summary>
    /// Plays a single spinner with real key presses and checks the result depends on how long Space is held.
    /// </summary>
    [TestFixture]
    public partial class TestSceneSpinner : PlayerTestScene
    {
        private const double start = 1500;
        private const double duration = 1500;

        private readonly List<JudgementResult> results = new List<JudgementResult>();

        protected override bool HasCustomSteps => true;

        protected override Ruleset CreatePlayerRuleset() => new KeyboardRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap<KeyboardHitObject>
        {
            BeatmapInfo = new BeatmapInfo { Ruleset = ruleset, Difficulty = new BeatmapDifficulty() },
            HitObjects = { new KeyboardSpinner { StartTime = start, Duration = duration } },
        };

        private void startPlay()
        {
            CreateTest();
            AddStep("track judgements", () =>
            {
                results.Clear();
                Player.ScoreProcessor.NewJudgement += r => results.Add(r);
            });
        }

        [Test]
        public void TestHoldingTheWholeTimeIsGreat()
        {
            startPlay();
            AddStep("hold space before the spinner starts", () => InputManager.PressKey(Key.Space));
            AddUntilStep("spinner judged", () => results.Count == 1);
            AddStep("let go", () => InputManager.ReleaseKey(Key.Space));
            AddAssert("judged Great", () => results[0].Type == HitResult.Great);
        }

        [Test]
        public void TestNotPressingIsAMiss()
        {
            startPlay();
            AddUntilStep("spinner judged", () => results.Count == 1);
            AddAssert("judged Miss", () => results[0].Type == HitResult.Miss);
        }

        [Test]
        public void TestLettingGoPartwayGivesAPartialResult()
        {
            DrawableKeyboardSpinner? drawable = null;
            double heldAtJudgement = 0;

            startPlay();
            AddUntilStep("spinner on screen", () => (drawable = Player.DrawableRuleset.Playfield.AllHitObjects.OfType<DrawableKeyboardSpinner>().FirstOrDefault()) != null);
            AddStep("remember held time when judged", () => Player.ScoreProcessor.NewJudgement += _ => heldAtJudgement = drawable!.HeldTime);
            AddStep("hold space", () => InputManager.PressKey(Key.Space));
            AddUntilStep("part of the spinner done", () => Player.GameplayClockContainer.CurrentTime >= start + duration * 0.3);
            AddStep("let go", () => InputManager.ReleaseKey(Key.Space));
            AddUntilStep("spinner judged", () => results.Count == 1);

            // The test clock is not precise enough to aim for an exact share, so check against what was actually held.
            AddStep("result matches the share held", () =>
            {
                double ratio = heldAtJudgement / duration;
                Assert.That(ratio, Is.InRange(0.05, 0.95), "should have been a partial hold");
                Assert.That(results[0].Type, Is.EqualTo(KeyboardSpinner.ResultFor(ratio)), $"held {ratio:P0}");
                Assert.That(results[0].Type, Is.Not.EqualTo(HitResult.Great), "letting go early must not be Great");
            });
        }

        [Test]
        public void TestResultThresholds()
        {
            AddAssert("95% is Great", () => KeyboardSpinner.ResultFor(0.95) == HitResult.Great);
            AddAssert("70% is Ok", () => KeyboardSpinner.ResultFor(0.70) == HitResult.Ok);
            AddAssert("40% is Meh", () => KeyboardSpinner.ResultFor(0.40) == HitResult.Meh);
            AddAssert("10% is Miss", () => KeyboardSpinner.ResultFor(0.10) == HitResult.Miss);
        }
    }
}
