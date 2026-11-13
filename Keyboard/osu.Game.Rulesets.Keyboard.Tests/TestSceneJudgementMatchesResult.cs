// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Keyboard.Configuration;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Keyboard.Objects.Drawables;
using osu.Game.Rulesets.Keyboard.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    /// <summary>
    /// Without pressing anything: the judgement drawn must say Miss (it used to say Perfect for everything).
    /// Also checks notes appear as early as the map's approach rate says.
    /// </summary>
    [TestFixture]
    public partial class TestSceneJudgementMatchesResult : PlayerTestScene
    {
        protected override bool HasCustomSteps => true;

        protected override Ruleset CreatePlayerRuleset() => new KeyboardRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap<KeyboardHitObject>
        {
            BeatmapInfo = new BeatmapInfo { Ruleset = ruleset, Difficulty = new BeatmapDifficulty { ApproachRate = 5 } },
            HitObjects =
            {
                new KeyboardHitObject { Key = KeyboardAction.A, StartTime = 3000 },
                new KeyboardHitObject { Key = KeyboardAction.S, StartTime = 3600 },
            },
        };

        private void play()
        {
            CreateTest(() => AddStep("judgements on", () =>
                ((KeyboardRulesetConfigManager)RulesetConfigs.GetConfigFor(new KeyboardRuleset())!).SetValue(KeyboardRulesetSetting.ShowJudgements, true)));
        }

        [Test]
        public void TestMissIsDrawnAsMiss()
        {
            play();

            AddUntilStep("a judgement is on screen", () => Player.DrawableRuleset.Playfield.ChildrenOfType<KeyboardJudgement>().Any(j => j.IsPresent));
            AddStep("it says MISS", () =>
            {
                var judgement = Player.DrawableRuleset.Playfield.ChildrenOfType<KeyboardJudgement>().First(j => j.IsPresent);
                Assert.That(judgement.Result!.Type, Is.EqualTo(HitResult.Miss));
                Assert.That(judgement.ChildrenOfType<SpriteText>().Single().Text.ToString(), Is.EqualTo("MISS"));
            });
        }

        [Test]
        public void TestNotesAppearAsEarlyAsTheMapsApproachRateSays()
        {
            play();

            AddUntilStep("notes created", () => Player.DrawableRuleset.Playfield.AllHitObjects.OfType<DrawableKeyboardHitObject>().Any());
            AddStep("AR 5: the first note appears 1400ms early", () =>
            {
                var note = Player.DrawableRuleset.Playfield.AllHitObjects.OfType<DrawableKeyboardHitObject>().First(n => n.HitObject.StartTime == 3000);
                Assert.That(note.HitObject.StartTime - note.LifetimeStart, Is.EqualTo(1400).Within(1));
            });
        }
    }
}
