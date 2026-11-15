// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Keyboard.Beatmaps;
using osu.Game.Rulesets.Keyboard.Mods;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Beatmaps;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    [TestFixture]
    public class TestMods
    {
        private static readonly KeyboardRuleset ruleset = new KeyboardRuleset();

        private static string[] acronymsFor(ModType type) => ruleset.GetModsFor(type).Select(m => m.Acronym).ToArray();

        [Test]
        public void TestModsAreGroupedLikeOsu()
        {
            Assert.That(acronymsFor(ModType.DifficultyReduction), Is.EqualTo(new[] { "EZ", "NF", "HT", "DC" }));
            Assert.That(acronymsFor(ModType.DifficultyIncrease), Is.EqualTo(new[] { "HR", "SD", "PF", "DT", "NC", "HD", "OG" }));
            Assert.That(acronymsFor(ModType.Conversion), Is.EqualTo(new[] { "DA", "MR", "LH", "RH", "HM" }));
            Assert.That(acronymsFor(ModType.Automation), Is.EqualTo(new[] { "AT", "CN" }));
            Assert.That(acronymsFor(ModType.Fun), Is.EqualTo(new[] { "WU", "WD", "MU" }));
        }

        [Test]
        public void TestEveryModIsInItsOwnCategoryAndUnique()
        {
            var all = Enum.GetValues<ModType>().SelectMany(type => ruleset.GetModsFor(type).Select(mod => (type, mod))).ToList();

            foreach (var (type, mod) in all)
                Assert.That(mod.Type, Is.EqualTo(type), mod.Acronym);

            Assert.That(all.Select(m => m.mod.Acronym), Is.Unique);
            Assert.That(all.Select(m => m.mod.Description.ToString()), Has.None.Empty);
        }

        [Test]
        public void TestOwnModsShowTheirAcronym()
        {
            Assert.That(new KeyboardModLeftHand().Icon, Is.Null);
            Assert.That(new KeyboardModRightHand().Icon, Is.Null);
            Assert.That(new KeyboardModHomeRow().Icon, Is.Null);
            Assert.That(new KeyboardModOriginalDensity().Icon, Is.Null);
        }

        [TestCase(typeof(KeyboardModDoubleTime), typeof(KeyboardModNightcore))]
        [TestCase(typeof(KeyboardModHalfTime), typeof(KeyboardModDaycore))]
        [TestCase(typeof(KeyboardModDoubleTime), typeof(KeyboardModHalfTime))]
        [TestCase(typeof(KeyboardModEasy), typeof(KeyboardModHardRock))]
        [TestCase(typeof(KeyboardModSuddenDeath), typeof(KeyboardModPerfect))]
        [TestCase(typeof(KeyboardModNoFail), typeof(KeyboardModSuddenDeath))]
        [TestCase(typeof(KeyboardModAutoplay), typeof(KeyboardModCinema))]
        [TestCase(typeof(KeyboardModWindUp), typeof(KeyboardModWindDown))]
        [TestCase(typeof(KeyboardModWindUp), typeof(KeyboardModDoubleTime))]
        [TestCase(typeof(KeyboardModLeftHand), typeof(KeyboardModRightHand))]
        [TestCase(typeof(KeyboardModDifficultyAdjust), typeof(KeyboardModEasy))]
        [TestCase(typeof(KeyboardModHomeRow), typeof(KeyboardModLeftHand))]
        [TestCase(typeof(KeyboardModRightHand), typeof(KeyboardModHomeRow))]
        public void TestModsThatCannotBeTogether(Type first, Type second)
            => Assert.That(ModUtils.CheckCompatibleSet(new[] { (Mod)Activator.CreateInstance(first)!, (Mod)Activator.CreateInstance(second)! }), Is.False);

        [Test]
        public void TestModsThatCanBeTogether()
        {
            Assert.That(ModUtils.CheckCompatibleSet(new Mod[]
            {
                new KeyboardModHardRock(), new KeyboardModHidden(), new KeyboardModNightcore(), new KeyboardModSuddenDeath(),
                new KeyboardModMirror(), new KeyboardModLeftHand(), new KeyboardModOriginalDensity(),
            }), Is.True);
        }

        // ---- Conversion mods ----

        private static Beatmap<KeyboardHitObject> convert(Action<KeyboardBeatmapConverter> options, IEnumerable<HitObject> objects)
        {
            var source = new Beatmap<HitObject>();
            source.HitObjects.AddRange(objects);
            var converter = new KeyboardBeatmapConverter(source, ruleset);
            options(converter);
            return (Beatmap<KeyboardHitObject>)converter.Convert();
        }

        /// <summary>Notes half a second apart (never thinned out), all over the playfield.</summary>
        private static IEnumerable<HitObject> spreadOut(int count, float maxX = 512)
            => Enumerable.Range(0, count).Select(i => new FakeCircle
            {
                StartTime = 1000 + i * 500,
                Position = new Vector2((float)(KeyboardBeatmapConverter.RandomFor(i, 7) * maxX), (float)(KeyboardBeatmapConverter.RandomFor(i, 8) * 384)),
            });

        [Test]
        public void TestMirrorPutsLeftNotesOnTheRight()
        {
            var normal = convert(_ => { }, spreadOut(100, maxX: 140));
            var mirrored = convert(c => c.Mirror = true, spreadOut(100, maxX: 140));

            Assert.That(normal.HitObjects.Select(h => KeyboardLayout.HandFor(h.Key)), Is.All.EqualTo(KeyboardHand.Left));
            Assert.That(mirrored.HitObjects.Select(h => KeyboardLayout.HandFor(h.Key)), Is.All.EqualTo(KeyboardHand.Right));
            Assert.That(mirrored.HitObjects.Select(h => h.StartTime), Is.EqualTo(normal.HitObjects.Select(h => h.StartTime)));
        }

        [TestCase(KeyboardHand.Left, 13)]
        [TestCase(KeyboardHand.Right, 15)]
        public void TestOneHandUsesOnlyThatHalf(KeyboardHand hand, int minKeys)
        {
            var objects = spreadOut(400).Append(new FakeSpinner { StartTime = 1000 + 400 * 500, Duration = 2000 }).ToList();
            var beatmap = convert(c => c.OneHand = hand, objects);
            var notes = beatmap.HitObjects.Where(h => h is not KeyboardSpinner).ToList();

            Assert.That(notes.Select(h => KeyboardLayout.HandFor(h.Key)), Is.All.EqualTo(hand));
            // The whole map is spread over that half, not crowded onto a few keys.
            Assert.That(notes.Select(h => h.Key).Distinct().Count(), Is.GreaterThanOrEqualTo(minKeys));
            Assert.That(beatmap.HitObjects.OfType<KeyboardSpinner>().Single().Key, Is.EqualTo(KeyboardAction.Space));
        }

        [Test]
        public void TestHomeRowUsesOnlyTheEightHomeKeys()
        {
            var objects = spreadOut(400).Append(new FakeSpinner { StartTime = 1000 + 400 * 500, Duration = 2000 }).ToList();
            var beatmap = convert(c => c.HomeRowOnly = true, objects);
            var notes = beatmap.HitObjects.Where(h => h is not KeyboardSpinner).ToList();

            Assert.That(notes.Select(h => h.Key), Is.All.Matches<KeyboardAction>(k => KeyboardLayout.HOME_ROW.Contains(k)));
            // All eight are used, the little-finger keys (normally edge keys) too, and none is left nearly empty.
            var perKey = notes.GroupBy(h => h.Key).ToDictionary(g => g.Key, g => g.Count());
            Assert.That(perKey.Keys, Is.EquivalentTo(KeyboardLayout.HOME_ROW));
            Assert.That(perKey.Values.Min(), Is.GreaterThan(notes.Count / 8 / 3), string.Join(", ", perKey.Select(p => $"{p.Key}={p.Value}")));
            Assert.That(beatmap.HitObjects.OfType<KeyboardSpinner>().Single().Key, Is.EqualTo(KeyboardAction.Space));
        }

        [Test]
        public void TestHomeRowKeepsLeftNotesOnTheLeft()
        {
            // When the nearest keys are still busy with earlier notes, a note moves to the nearest free one, which near the
            // middle can be across the gap (J rather than A). Almost all of them stay on their own side.
            var hands = convert(c => c.HomeRowOnly = true, spreadOut(100, maxX: 200)).HitObjects.Select(h => KeyboardLayout.HandFor(h.Key)).ToList();
            Assert.That(hands.Count(h => h == KeyboardHand.Left), Is.GreaterThanOrEqualTo(hands.Count * 9 / 10));
        }

        [Test]
        public void TestNormalConversionUsesBothHands()
        {
            var hands = convert(_ => { }, spreadOut(200)).HitObjects.Select(h => KeyboardLayout.HandFor(h.Key)).ToList();
            Assert.That(hands, Does.Contain(KeyboardHand.Left).And.Contain(KeyboardHand.Right));
        }

        [Test]
        public void TestConvertedNotesOnlyUseKeysThatAreLit()
        {
            foreach (var (hand, homeRow) in new (KeyboardHand?, bool)[] { (null, false), (KeyboardHand.Left, false), (KeyboardHand.Right, false), (null, true) })
            {
                var keys = convert(c => { c.OneHand = hand; c.HomeRowOnly = homeRow; }, spreadOut(300)).HitObjects.Select(h => h.Key);
                Assert.That(keys, Is.All.Matches<KeyboardAction>(k => KeyboardLayout.IsUsed(k, hand, homeRow)), $"{hand} {homeRow}");
            }
        }

        [Test]
        public void TestHandsSplitTheKeyboard()
        {
            var keys = KeyboardLayout.Default.Keys.Where(k => k.Playable && k.Action != KeyboardAction.Space).ToList();
            Assert.That(keys.Count(k => KeyboardLayout.HandFor(k.Action) == KeyboardHand.Left), Is.EqualTo(15));
            Assert.That(keys.Count(k => KeyboardLayout.HandFor(k.Action) == KeyboardHand.Right), Is.EqualTo(18));
        }

        // ---- Through osu!'s own loading (the way a map is really played) ----

        /// <summary>A dense stream (200 ms apart) on the left of an osu!standard map, loaded the way the game loads it.</summary>
        private static List<KeyboardHitObject> play(float approachRate, params Mod[] mods)
        {
            var source = new Beatmap<HitObject>
            {
                BeatmapInfo = { Ruleset = new RulesetInfo { ShortName = "osu", OnlineID = 0 }, Difficulty = new BeatmapDifficulty { ApproachRate = approachRate } },
            };
            source.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 400 });
            source.HitObjects.AddRange(spreadOut(300, maxX: 140).Select((h, i) =>
            {
                h.StartTime = 1000 + i * 200;
                return h;
            }));

            return new TestWorkingBeatmap(source).GetPlayableBeatmap(ruleset.RulesetInfo, mods).HitObjects.Cast<KeyboardHitObject>().ToList();
        }

        [Test]
        public void TestConversionModsWorkWhenPlaying()
        {
            Assert.That(play(9).Select(h => KeyboardLayout.HandFor(h.Key)), Is.All.EqualTo(KeyboardHand.Left));
            Assert.That(play(9, new KeyboardModMirror()).Select(h => KeyboardLayout.HandFor(h.Key)), Is.All.EqualTo(KeyboardHand.Right));
            Assert.That(play(9, new KeyboardModRightHand()).Select(h => KeyboardLayout.HandFor(h.Key)), Is.All.EqualTo(KeyboardHand.Right));
            Assert.That(play(9, new KeyboardModHomeRow()).Select(h => h.Key), Is.All.Matches<KeyboardAction>(k => KeyboardLayout.HOME_ROW.Contains(k)));
        }

        /// <summary>
        /// osu! changes the approach rate only after conversion, but conversion must already use it: otherwise slow squares
        /// (low approach rate) arrive on keys still busy with the previous note and appear part-way full. Home Row has only
        /// eight keys for a dense stream.
        /// </summary>
        [TestCase(9f)]
        [TestCase(7f)]
        public void TestNoNoteAppearsLateWithMods(float approachRate)
        {
            var slowest = new KeyboardModDifficultyAdjust();
            slowest.ApproachRate.Value = 0;

            foreach (var mods in new[]
                     {
                         Array.Empty<Mod>(),
                         new Mod[] { new KeyboardModEasy() },
                         new Mod[] { new KeyboardModHardRock() },
                         new Mod[] { slowest },
                         new Mod[] { new KeyboardModHomeRow() },
                         new Mod[] { new KeyboardModEasy(), new KeyboardModHomeRow() },
                         new Mod[] { new KeyboardModLeftHand() },
                     })
            {
                var notes = play(approachRate, mods);
                string name = string.Join("+", mods.Select(m => m.Acronym));

                Assert.That(notes, Has.Count.EqualTo(300), name);
                Assert.That(notes.Where(h => h.AppearOffset < h.FillDuration), Is.Empty, $"{name}: notes appearing part-way full");
            }
        }

        // ---- Difficulty mods ----

        private static double greatWindowWith(Mod? mod)
        {
            var difficulty = new BeatmapDifficulty { OverallDifficulty = 6 };
            (mod as IApplicableToDifficulty)?.ApplyToDifficulty(difficulty);

            var windows = new KeyboardHitWindows();
            windows.SetDifficulty(difficulty.OverallDifficulty);
            return windows.WindowFor(HitResult.Great);
        }

        [Test]
        public void TestEasyAndHardRockChangeTheApproachRate()
        {
            var easy = new BeatmapDifficulty { ApproachRate = 6 };
            new KeyboardModEasy().ApplyToDifficulty(easy);
            var hardRock = new BeatmapDifficulty { ApproachRate = 6 };
            new KeyboardModHardRock().ApplyToDifficulty(hardRock);

            Assert.That(easy.ApproachRate, Is.EqualTo(3));
            Assert.That(hardRock.ApproachRate, Is.EqualTo(8.4f));
        }

        [Test]
        public void TestEasyAndHardRockChangeTheTimingWindows()
        {
            Assert.That(greatWindowWith(new KeyboardModEasy()), Is.GreaterThan(greatWindowWith(null)));
            Assert.That(greatWindowWith(new KeyboardModHardRock()), Is.LessThan(greatWindowWith(null)));
        }

        [Test]
        public void TestDifficultyAdjustSetsTheApproachRate()
        {
            var mod = new KeyboardModDifficultyAdjust();
            mod.ApproachRate.Value = 9;
            mod.OverallDifficulty.Value = 2;

            var difficulty = new BeatmapDifficulty { ApproachRate = 5, OverallDifficulty = 5 };
            mod.ApplyToDifficulty(difficulty);

            Assert.That(difficulty.ApproachRate, Is.EqualTo(9));
            Assert.That(difficulty.OverallDifficulty, Is.EqualTo(2));
            Assert.That(mod.SettingDescription.Select(s => s.setting.ToString()), Does.Contain("Approach rate"));
        }

        // ---- Hidden ----

        [Test]
        public void TestHiddenLeavesTheFirstNoteVisibleNotASpinner()
        {
            var hidden = new TestHidden();
            Assert.That(hidden.CountsAsFirst(new KeyboardSpinner()), Is.False);
            Assert.That(hidden.CountsAsFirst(new KeyboardHitObject()), Is.True);
        }

        private class TestHidden : KeyboardModHidden
        {
            public bool CountsAsFirst(HitObject hitObject) => IsFirstAdjustableObject(hitObject);
        }

        [Test]
        public void TestHiddenFadesNotesBeforeTheyAreDue()
        {
            var note = new KeyboardHitObject { StartTime = 5000, TimePreempt = 1000 };
            var (start, end) = KeyboardModHidden.FadeOutTimes(note);

            Assert.That(start, Is.EqualTo(4400));
            Assert.That(end, Is.EqualTo(4700));
        }

        [Test]
        public void TestHiddenStillShowsALateNoteForAMoment()
        {
            // The key was busy until shortly before: this note only appears 250 ms before it is due.
            var note = new KeyboardHitObject { StartTime = 5000, TimePreempt = 1000, PreviousOnSameKey = 5000 - 250 - KeyboardHitObject.SAME_KEY_CLEARANCE };
            var (start, end) = KeyboardModHidden.FadeOutTimes(note);

            Assert.That(start, Is.EqualTo(4750));
            Assert.That(end, Is.EqualTo(4900));
        }

        private class FakeCircle : HitObject, IHasPosition
        {
            public Vector2 Position { get; set; }
            public float X { get => Position.X; set => Position = new Vector2(value, Y); }
            public float Y { get => Position.Y; set => Position = new Vector2(X, value); }
        }

        private class FakeSpinner : HitObject, IHasDuration
        {
            public double Duration { get; set; }
            public double EndTime { get => StartTime + Duration; set => Duration = value - StartTime; }
        }
    }
}
