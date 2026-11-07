// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Keyboard.Configuration;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Keyboard.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    /// <summary>
    /// With autoplay (which always hits Perfect) checks that the judgement shows up small, inside the hit key, under its letter.
    /// </summary>
    [TestFixture]
    public partial class TestSceneJudgementDisplay : PlayerTestScene
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
                new KeyboardHitObject { Key = KeyboardAction.H, StartTime = 1000 },
                new KeyboardHitObject { Key = KeyboardAction.Slash, StartTime = 1500 },
            },
        };

        private void play(bool showJudgements)
        {
            CreateTest(() => AddStep($"show judgements: {showJudgements}", () =>
            {
                var config = (KeyboardRulesetConfigManager)RulesetConfigs.GetConfigFor(new KeyboardRuleset())!;
                config.SetValue(KeyboardRulesetSetting.ShowJudgements, showJudgements);
                config.SetValue(KeyboardRulesetSetting.Layout, KeyboardLayoutType.TurkishQ);
            }));
            AddStep("track judgements", () =>
            {
                results.Clear();
                Player.ScoreProcessor.NewJudgement += r => results.Add(r);
            });
        }

        private IEnumerable<KeyboardJudgement> judgements => Player.DrawableRuleset.Playfield.ChildrenOfType<KeyboardJudgement>().Where(j => j.IsPresent);

        [Test]
        public void TestJudgementSitsUnderTheLetter()
        {
            play(true);

            AddUntilStep("judgement for H on screen", () => judgements.Any(j => j.Key == KeyboardAction.H && j.Alpha > 0.5f));
            AddStep("it is Perfect, inside H, below its centre and smaller than the key", () =>
            {
                var judgement = judgements.First(j => j.Key == KeyboardAction.H);
                var key = Player.DrawableRuleset.Playfield.ChildrenOfType<KeyBackground>().Single(k => k.Action == KeyboardAction.H).ScreenSpaceDrawQuad;
                var centre = judgement.ScreenSpaceDrawQuad.Centre;

                Assert.That(judgement.Result!.Type, Is.EqualTo(HitResult.Perfect));
                Assert.That(centre.X, Is.InRange(key.TopLeft.X, key.TopRight.X), "not inside the key horizontally");
                Assert.That(centre.Y, Is.InRange(key.Centre.Y, key.BottomLeft.Y), "not in the lower half of the key (under the letter)");
                Assert.That(judgement.ScreenSpaceDrawQuad.Width, Is.LessThan(key.Width), "bigger than the key");
            });
            AddUntilStep("it fades away again", () => !judgements.Any(j => j.Key == KeyboardAction.H));
        }

        [Test]
        public void TestNothingShownWhenTurnedOff()
        {
            play(false);

            AddUntilStep("both notes judged", () => results.Count == 2);
            AddAssert("no judgement drawn", () => !Player.DrawableRuleset.Playfield.ChildrenOfType<KeyboardJudgement>().Any(j => j.IsPresent));
            AddAssert("notes were still hit", () => results.All(r => r.IsHit));
        }
    }
}
