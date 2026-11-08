// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    /// <summary>
    /// Autoplay must hit every note and hold every spinner to the end.
    /// </summary>
    [TestFixture]
    public partial class TestSceneAutoplay : PlayerTestScene
    {
        private readonly List<JudgementResult> results = new List<JudgementResult>();

        protected override bool Autoplay => true;

        protected override bool HasCustomSteps => true;

        protected override Ruleset CreatePlayerRuleset() => new KeyboardRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap<KeyboardHitObject>
        {
            BeatmapInfo = new BeatmapInfo { Ruleset = ruleset, Difficulty = new BeatmapDifficulty() },
            HitObjects =
            {
                new KeyboardHitObject { Key = KeyboardAction.A, StartTime = 1000 },
                new KeyboardHitObject { Key = KeyboardAction.S, StartTime = 1300 },
                // Two keys at the same moment.
                new KeyboardHitObject { Key = KeyboardAction.J, StartTime = 1600 },
                new KeyboardHitObject { Key = KeyboardAction.K, StartTime = 1600 },
                new KeyboardSpinner { StartTime = 2000, Duration = 1500 },
                new KeyboardHitObject { Key = KeyboardAction.Slash, StartTime = 3800 },
            },
        };

        [Test]
        public void TestAutoplayHitsEverything()
        {
            CreateTest();
            AddStep("track judgements", () =>
            {
                results.Clear();
                Player.ScoreProcessor.NewJudgement += r => results.Add(r);
            });
            AddUntilStep("everything judged", () => results.Count == 6);
            AddStep("nothing missed, spinner Great", () =>
            {
                string summary = string.Join(" | ", results.Select(r => $"{((KeyboardHitObject)r.HitObject).Key}: {r.Type}"));
                Assert.That(results.All(r => r.IsHit), Is.True, summary);
                Assert.That(results.Single(r => r.HitObject is KeyboardSpinner).Type, Is.EqualTo(HitResult.Great), summary);
            });
        }
    }
}
