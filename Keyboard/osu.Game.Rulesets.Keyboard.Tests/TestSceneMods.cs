// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Keyboard.Mods;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Keyboard.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Framework.Testing;
using osu.Game.Rulesets.Keyboard.UI;
using osu.Game.Tests.Visual;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    /// <summary>A map plays from start to end with each mod on (and autoplay pressing the keys).</summary>
    [TestFixture]
    public partial class TestSceneMods : PlayerTestScene
    {
        private readonly List<JudgementResult> results = new List<JudgementResult>();

        protected override bool HasCustomSteps => true;

        // The test player adds NoFail unless failing is allowed. Allowed here, so Sudden Death and Perfect really apply.
        protected override bool AllowFail => true;

        protected override Ruleset CreatePlayerRuleset() => new KeyboardRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap<KeyboardHitObject>
        {
            BeatmapInfo = new BeatmapInfo { Ruleset = ruleset, Difficulty = new BeatmapDifficulty() },
            HitObjects =
            {
                new KeyboardHitObject { Key = KeyboardAction.A, StartTime = 1000 },
                new KeyboardHitObject { Key = KeyboardAction.S, StartTime = 2500 },
                new KeyboardHitObject { Key = KeyboardAction.J, StartTime = 3000 },
                new KeyboardSpinner { StartTime = 3500, Duration = 1000 },
                new KeyboardHitObject { Key = KeyboardAction.Slash, StartTime = 5000 },
            },
        };

        private static readonly Mod[] all_mods = Enum.GetValues<ModType>().SelectMany(t => new KeyboardRuleset().GetModsFor(t)).ToArray();

        private static IEnumerable<string> modsToPlay => all_mods.Select(m => m.Acronym).Where(a => a != "AT" && a != "CN");

        private void loadWith(params Mod[] mods)
        {
            AddStep($"load player with {string.Join(", ", mods.Select(m => m.Acronym))}", () => LoadPlayer(mods));
            AddUntilStep("player loaded", () => Player.IsLoaded && Player.Alpha == 1);
            AddStep("track judgements", () =>
            {
                results.Clear();
                Player.ScoreProcessor.NewJudgement += r => results.Add(r);
            });
        }

        private void checkEverythingHit()
        {
            AddUntilStep("everything judged", () => results.Count == 5);
            AddAssert("all hit", () => results.All(r => r.IsHit));
            AddAssert("not failed", () => !Player.GameplayState.HasFailed);
        }

        [TestCaseSource(nameof(modsToPlay))]
        public void TestPlaysThroughWith(string acronym)
        {
            var mod = all_mods.Single(m => m.Acronym == acronym).DeepClone();
            loadWith(mod, new KeyboardModAutoplay());
            AddAssert($"{acronym} is on", () => Player.GameplayState.Mods.Any(m => m.Acronym == acronym));
            checkEverythingHit();
        }

        [Test]
        public void TestCinemaPlaysByItself()
        {
            loadWith(new KeyboardModCinema());
            checkEverythingHit();
        }

        [TestCase(null, 33)]
        [TestCase("LH", 15)]
        [TestCase("RH", 18)]
        [TestCase("HM", 8)]
        public void TestKeysWithoutNotesAreDim(string? acronym, int keysInUse)
        {
            var mods = new List<Mod> { new KeyboardModAutoplay() };
            if (acronym != null)
                mods.Add(all_mods.Single(m => m.Acronym == acronym).DeepClone());

            loadWith(mods.ToArray());

            IEnumerable<KeyBackground> keys() => Player.ChildrenOfType<KeyBackground>().Where(k => k.Action != null && k.Action != KeyboardAction.Space);

            AddAssert($"{keysInUse} letter keys lit", () => keys().Count(k => !k.IsDimmed), () => Is.EqualTo(keysInUse));
            AddAssert("lit keys are the ones notes can land on", () => keys().Where(k => !k.IsDimmed).All(k =>
            {
                var playfield = Player.ChildrenOfType<KeyboardPlayfield>().Single();
                return playfield.IsInUse(playfield.Layout[k.Action!.Value]);
            }));
            AddAssert("dim keys are see-through", () => keys().Where(k => k.IsDimmed).Select(k => k.Alpha), () => Is.All.EqualTo(0.35f));
            AddAssert("Space lit", () => Player.ChildrenOfType<KeyBackground>().Single(k => k.Action == KeyboardAction.Space).IsDimmed, () => Is.False);
            AddAssert("no press highlight on dim keys", () => Player.ChildrenOfType<KeyPressHighlight>().Count(), () => Is.EqualTo(keysInUse + 1));
        }

        [TestCase("DT", 1.5)]
        [TestCase("NC", 1.5)]
        [TestCase("HT", 0.75)]
        [TestCase("DC", 0.75)]
        public void TestSpeed(string acronym, double rate)
        {
            loadWith(all_mods.Single(m => m.Acronym == acronym).DeepClone(), new KeyboardModAutoplay());
            AddAssert($"runs at {rate}x", () => ModUtils.CalculateRateWithMods(Player.GameplayState.Mods) == rate);
        }

        [Test]
        public void TestHiddenHidesNotesBeforeTheyAreDue()
        {
            loadWith(new KeyboardModHidden(), new KeyboardModAutoplay());

            DrawableKeyboardHitObject drawableAt(double time) => Player.DrawableRuleset.Playfield.AllHitObjects
                                                                       .OfType<DrawableKeyboardHitObject>()
                                                                       .Single(d => d.HitObject.StartTime == time);

            // Headless frames can be a few hundred ms apart, so each look is taken in the first frame inside its time range.
            float? alphaIn(double time, double from, double to)
            {
                double now = Player.GameplayClockContainer.CurrentTime;
                return now >= from && now < to ? drawableAt(time).Alpha : null;
            }

            float? first = null, secondBefore = null, secondAfter = null;

            // The first note is left visible, like osu!'s Hidden.
            AddUntilStep("look at the first note just before it is due", () => (first = alphaIn(1000, 700, 1000)) != null);
            AddAssert("first note visible", () => first, () => Is.EqualTo(1));

            var second = new KeyboardHitObject { StartTime = 2500 };
            AddStep("work out when the second note fades", () => second = drawableAt(2500).HitObject);
            AddUntilStep("look at the second note before it fades", () => (secondBefore = alphaIn(2500, 1300, KeyboardModHidden.FadeOutTimes(second).start)) != null);
            AddAssert("second note visible", () => secondBefore, () => Is.EqualTo(1));
            AddUntilStep("look at the second note after it faded", () => (secondAfter = alphaIn(2500, KeyboardModHidden.FadeOutTimes(second).end, 2500)) != null);
            AddAssert("second note gone before it is due", () => secondAfter, () => Is.LessThan(0.01f));
            checkEverythingHit();
        }
    }
}
