// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Localisation;
using osu.Game.Rulesets.Keyboard.Beatmaps;
using osu.Game.Rulesets.Keyboard.Localisation;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    [TestFixture]
    public class TestConversionAndLanguages
    {
        private static Beatmap<KeyboardHitObject> convert(params HitObject[] objects)
        {
            var source = new Beatmap<HitObject>();
            source.HitObjects.AddRange(objects);
            return (Beatmap<KeyboardHitObject>)new KeyboardBeatmapConverter(source, new KeyboardRuleset()).Convert();
        }

        private static double[] noteTimes(Beatmap<KeyboardHitObject> beatmap) => beatmap.HitObjects.Select(h => h.StartTime).ToArray();

        // The default beat is 1000 ms, so anything under 1450 ms is short.
        [TestCase(500, 0, new double[] { 1000 }, TestName = "A 1/2 beat slider is one press")]
        [TestCase(1000, 0, new double[] { 1000 }, TestName = "A one beat slider is one press")]
        [TestCase(1500, 0, new double[] { 1000, 2500 }, TestName = "A 1.5 beat slider is two presses")]
        [TestCase(6000, 2, new double[] { 1000, 3000, 5000, 7000 }, TestName = "Reverse slider with long passes gets a press on every turn")]
        [TestCase(3200, 3, new double[] { 1000, 4200 }, TestName = "Short passes get no press on the turns")]
        [TestCase(1200, 1, new double[] { 1000 }, TestName = "Short reverse slider is one press")]
        public void TestSliderPresses(double duration, int repeats, double[] expected)
        {
            var result = convert(new FakeSlider { StartTime = 1000, Duration = duration, RepeatCount = repeats });

            Assert.That(noteTimes(result), Is.EqualTo(expected).Within(0.001));
        }

        [Test]
        public void TestVeryShortDependsOnTheMapsTempo()
        {
            var source = new Beatmap<HitObject>();
            source.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 2000 });
            source.HitObjects.Add(new FakeSlider { StartTime = 1000, Duration = 2800 });

            var result = (Beatmap<KeyboardHitObject>)new KeyboardBeatmapConverter(source, new KeyboardRuleset()).Convert();

            Assert.That(noteTimes(result), Is.EqualTo(new double[] { 1000 }));
        }

        [Test]
        public void TestResultsScreenOnlyListsResultsThatExist()
        {
            var shown = new KeyboardRuleset().GetHitResultsForDisplay().Select(r => r.result);

            Assert.That(shown, Is.EquivalentTo(new[] { HitResult.Great, HitResult.Ok, HitResult.Meh, HitResult.Miss }));
        }

        [Test]
        public void TestCircleSizeIsHidden()
        {
            var acronyms = new KeyboardRuleset().GetBeatmapAttributesForDisplay(new BeatmapInfo(), Array.Empty<Mod>()).Select(a => a.Acronym);

            Assert.That(acronyms, Is.EqualTo(new[] { "AR", "OD", "HP" }));
        }

        [Test]
        public void TestSongSelectCountsNotesAndSpinners()
        {
            var result = convert(
                new FakeCircle { StartTime = 500 },
                new FakeSlider { StartTime = 1000, Duration = 2000 },
                new FakeSpinner { StartTime = 4000, Duration = 1000 });

            var statistics = result.GetStatistics().ToList();

            Assert.That(statistics.Select(s => s.Name), Is.EqualTo(new[] { BeatmapStatisticStrings.Notes, BeatmapStatisticStrings.Spinners }));
            Assert.That(statistics.Select(s => s.Content), Is.EqualTo(new[] { "3", "1" }));
        }

        [Test]
        public void TestSpinnerBecomesASpaceHold()
        {
            var result = convert(new FakeSpinner { StartTime = 1000, Duration = 2500 });
            var spinner = result.HitObjects.Single() as KeyboardSpinner;

            Assert.That(spinner, Is.Not.Null);
            Assert.That(spinner!.Key, Is.EqualTo(KeyboardAction.Space));
            Assert.That(spinner.StartTime, Is.EqualTo(1000));
            Assert.That(spinner.EndTime, Is.EqualTo(3500));
        }

        [Test]
        public void TestComboStartsAreCopied()
        {
            var result = convert(
                new FakeCircle { StartTime = 1000, Position = new Vector2(10, 10), NewCombo = true, ComboOffset = 2 },
                new FakeCircle { StartTime = 1500, Position = new Vector2(300, 200) },
                new FakeCircle { StartTime = 2000, Position = new Vector2(500, 380), NewCombo = true });

            Assert.That(result.HitObjects.Select(h => h.NewCombo), Is.EqualTo(new[] { true, false, true }));
            Assert.That(result.HitObjects[0].ComboOffset, Is.EqualTo(2));

            new KeyboardRuleset().CreateBeatmapProcessor(result).PreProcess();
            Assert.That(result.HitObjects[0].ComboIndex, Is.EqualTo(result.HitObjects[1].ComboIndex), "same combo");
            Assert.That(result.HitObjects[2].ComboIndex, Is.GreaterThan(result.HitObjects[1].ComboIndex), "new combo");
        }

        [Test]
        public void TestTopOfTheScreenGoesToTheTopRowAndBottomToTheBottomRow()
        {
            // Notes 3s apart: more than the same-key gap, so only their position decides the key.
            var layout = KeyboardLayout.Default;
            float rowOf(KeyboardHitObject h) => layout[h.Key].Y;

            var top = convert(Enumerable.Range(0, 40).Select(i => (HitObject)new FakeCircle { StartTime = i * 3000, Position = new Vector2(i * 12, 5) }).ToArray());
            var bottom = convert(Enumerable.Range(0, 40).Select(i => (HitObject)new FakeCircle { StartTime = i * 3000, Position = new Vector2(i * 12, 380) }).ToArray());

            Assert.That(top.HitObjects.All(h => rowOf(h) == 0), Is.True, "top of the osu! playfield -> top letter row");
            Assert.That(bottom.HitObjects.All(h => rowOf(h) == 2), Is.True,
                "bottom of the osu! playfield -> bottom letter row; got: " + string.Join(" ", bottom.HitObjects.Select(h => $"{h.StartTime / 1000}:{h.Key}")));
        }

        [Test]
        public void TestNotesSpreadEvenlyOverTheThreeRows()
        {
            var rng = new Random(7);
            var result = convert(Enumerable.Range(0, 3000).Select(i => (HitObject)new FakeCircle
            {
                StartTime = i * 3000, // far apart (more than the same-key gap), so only the position decides the key
                Position = new Vector2(rng.Next(0, 513), rng.Next(0, 385)),
            }).ToArray());

            var perRow = result.HitObjects.GroupBy(h => KeyboardLayout.Default[h.Key].Y).ToDictionary(g => g.Key, g => g.Count());

            // Roughly a third each (the bottom row has fewer keys, so allow some slack).
            foreach (float row in new[] { 0f, 1f, 2f })
                Assert.That(perRow[row], Is.InRange(600, 1400), $"row {row} got {perRow[row]} of 3000 notes");
        }

        [Test]
        public void TestRateModsChangeTheStarRating()
        {
            var beatmap = new Beatmap<HitObject> { BeatmapInfo = { Ruleset = new KeyboardRuleset().RulesetInfo } };
            for (int i = 0; i < 200; i++)
                beatmap.HitObjects.Add(new FakeCircle { StartTime = i * 250, Position = new Vector2(i % 512, 100) });

            var working = new osu.Game.Tests.Beatmaps.TestWorkingBeatmap(beatmap);
            var calculator = new KeyboardRuleset().CreateDifficultyCalculator(working);

            double normal = calculator.Calculate().StarRating;
            double doubleTime = calculator.Calculate(new osu.Game.Rulesets.Mods.Mod[] { new osu.Game.Rulesets.Keyboard.Mods.KeyboardModDoubleTime() }).StarRating;
            double halfTime = calculator.Calculate(new osu.Game.Rulesets.Mods.Mod[] { new osu.Game.Rulesets.Keyboard.Mods.KeyboardModHalfTime() }).StarRating;

            Assert.That(doubleTime, Is.GreaterThan(normal));
            Assert.That(halfTime, Is.LessThan(normal));
        }

        [Test]
        public void TestManiaHoldNoteIsNotASpinner()
        {
            var result = convert(new FakeManiaHold { StartTime = 1000, Duration = 500 });
            Assert.That(result.HitObjects.Single(), Is.Not.InstanceOf<KeyboardSpinner>());
        }

        [Test]
        public void TestNormalNotesNeverLandOnSpace()
        {
            var rng = new Random(3);
            var notes = Enumerable.Range(0, 3000).Select(i => (HitObject)new FakeCircle
            {
                StartTime = i * 50,
                // Bias towards the bottom of the screen, nearest to the space bar.
                Position = new Vector2(rng.Next(0, 513), rng.Next(250, 385)),
            }).ToArray();

            Assert.That(convert(notes).HitObjects.Count(h => h.Key == KeyboardAction.Space), Is.Zero);
        }

        [TestCase(3f)]
        [TestCase(5f)]
        [TestCase(9f)]
        public void TestTwoNotesOnOneKeyAreNeverOnScreenTogether(float approachRate)
        {
            // Worst case: a fast stream all on the same spot, which always wants the same key.
            var source = new Beatmap<HitObject>();
            source.Difficulty.ApproachRate = approachRate;
            for (int i = 0; i < 400; i++)
                source.HitObjects.Add(new FakeCircle { StartTime = 1000 + i * 100, Position = new Vector2(256, 192) });

            var converted = (Beatmap<KeyboardHitObject>)new KeyboardBeatmapConverter(source, new KeyboardRuleset()).Convert();
            foreach (var h in converted.HitObjects)
                h.ApplyDefaults(new osu.Game.Beatmaps.ControlPoints.ControlPointInfo(), source.Difficulty);

            foreach (var group in converted.HitObjects.GroupBy(h => h.Key))
            {
                var notes = group.OrderBy(h => h.StartTime).ToList();

                for (int i = 1; i < notes.Count; i++)
                {
                    // The next square may only appear once the previous note's late window (150ms at OD5) is over.
                    double nextStartsFilling = notes[i].StartTime - notes[i].AppearOffset;
                    Assert.That(nextStartsFilling, Is.GreaterThanOrEqualTo(notes[i - 1].StartTime + 150),
                        $"AR{approachRate}: two notes on {group.Key} at {notes[i - 1].StartTime} and {notes[i].StartTime} overlap on screen");
                }
            }
        }

        [Test]
        public void TestOnlyGreatOkMehAndMiss()
        {
            var windows = new KeyboardHitWindows();
            windows.SetDifficulty(5);

            var allowed = Enum.GetValues<HitResult>().Where(windows.IsHitResultAllowed).OrderBy(r => r).ToArray();
            Assert.That(allowed, Is.EqualTo(new[] { HitResult.Miss, HitResult.Meh, HitResult.Ok, HitResult.Great }.OrderBy(r => r)));

            // At OD 5, like osu!standard: Great within 49.5ms, Ok within 99.5ms, Meh within 149.5ms, Miss if pressed up to 400ms early.
            Assert.That(windows.ResultFor(0), Is.EqualTo(HitResult.Great));
            Assert.That(windows.ResultFor(-45), Is.EqualTo(HitResult.Great));
            Assert.That(windows.ResultFor(80), Is.EqualTo(HitResult.Ok));
            Assert.That(windows.ResultFor(-130), Is.EqualTo(HitResult.Meh));
            Assert.That(windows.ResultFor(-300), Is.EqualTo(HitResult.Miss));
            Assert.That(windows.ResultFor(-450), Is.EqualTo(HitResult.None), "far too early is ignored");

            Assert.That(new KeyboardHitObject().CreateJudgement().MaxResult, Is.EqualTo(HitResult.Great));
            Assert.That(new KeyboardSpinner().CreateJudgement().MaxResult, Is.EqualTo(HitResult.Great));
        }

        // Values from osu!standard's OsuHitWindows: floor(DifficultyRange(OD, 80/50/20, 140/100/60, 200/150/100)) - 0.5, Miss 400.
        [TestCase(0, 79.5, 139.5, 199.5)]
        [TestCase(5, 49.5, 99.5, 149.5)]
        [TestCase(8, 31.5, 75.5, 119.5)]
        [TestCase(10, 19.5, 59.5, 99.5)]
        public void TestWindowsMatchOsuStandard(double od, double great, double ok, double meh)
        {
            var windows = new KeyboardHitWindows();
            windows.SetDifficulty(od);

            Assert.That(windows.WindowFor(HitResult.Great), Is.EqualTo(great));
            Assert.That(windows.WindowFor(HitResult.Ok), Is.EqualTo(ok));
            Assert.That(windows.WindowFor(HitResult.Meh), Is.EqualTo(meh));
            Assert.That(windows.WindowFor(HitResult.Miss), Is.EqualTo(400));
        }

        [Test]
        public void TestLateNoteStartsPartlyFullAndGrowsAtTheSameSpeed()
        {
            var note = new KeyboardHitObject { StartTime = 5000, PreviousOnSameKey = 4500 };
            note.ApplyDefaults(new osu.Game.Beatmaps.ControlPoints.ControlPointInfo(), new BeatmapDifficulty { ApproachRate = 5 });

            // AR 5 = 1200ms to fill, but the key is only free from 4500 + 200.
            Assert.That(note.FillDuration, Is.EqualTo(1200));
            Assert.That(note.AppearOffset, Is.EqualTo(300));
            Assert.That(note.InitialFill, Is.EqualTo(0.75f).Within(0.001f), "appears three quarters full, so the growth speed is unchanged");

            var free = new KeyboardHitObject { StartTime = 5000 };
            free.ApplyDefaults(new osu.Game.Beatmaps.ControlPoints.ControlPointInfo(), new BeatmapDifficulty { ApproachRate = 5 });
            Assert.That(free.AppearOffset, Is.EqualTo(1200));
            Assert.That(free.InitialFill, Is.Zero);
        }

        [Test]
        public void TestEveryOsuLanguageHasATranslation()
        {
            // osu!lazer's own list of languages, e.g. "pt_br" -> "pt-br".
            var missing = Enum.GetNames<osu.Game.Localisation.Language>()
                              .Select(n => n.Replace('_', '-'))
                              .Where(code => KeyboardStrings.SectionFor(code) == null)
                              .ToList();

            Assert.That(missing, Is.Empty, "osu! languages without a translation: " + string.Join(", ", missing));
        }

        [Test]
        public void TestEveryLanguageTranslatesEveryText()
        {
            var keys = KeyboardStrings.AllKeys.ToList();
            Assert.That(keys, Has.Count.EqualTo(20));

            foreach (string language in KeyboardStrings.Languages)
            {
                foreach (string key in keys)
                    Assert.That(KeyboardStrings.RawTranslation(language, key), Is.Not.Null.And.Not.Empty, $"[{language}] is missing {key}");
            }
        }

        [TestCase("tr", nameof(KeyboardStrings.KeySpace), "Boşluk")]
        [TestCase("de", nameof(KeyboardStrings.KeyboardLayout), "Tastaturlayout")]
        [TestCase("ja", nameof(KeyboardStrings.KeyboardLayout), "キーボード配列")]
        [TestCase("pt", nameof(KeyboardStrings.KeyboardLayout), "Disposição do teclado")]
        [TestCase("pt-BR", nameof(KeyboardStrings.KeyboardLayout), "Layout do teclado")]
        [TestCase("zh-Hans", nameof(KeyboardStrings.KeyboardLayout), "键盘布局")]
        [TestCase("zh-Hant", nameof(KeyboardStrings.KeyboardLayout), "鍵盤配置")]
        [TestCase("zh-TW", nameof(KeyboardStrings.KeyboardLayout), "鍵盤配置")]
        [TestCase("es-419", nameof(KeyboardStrings.HoldSpace), "¡Mantén presionado Espacio!")]
        [TestCase("es", nameof(KeyboardStrings.HoldSpace), "¡Mantén Espacio!")]
        [TestCase("hr-HR", nameof(KeyboardStrings.KeySpace), "Razmaknica")]
        [TestCase("nb", nameof(KeyboardStrings.KeySpace), "Mellomrom")]
        [TestCase("ko", nameof(KeyboardStrings.KeyboardType), "키보드 종류")]
        [TestCase("xx", nameof(KeyboardStrings.KeyboardLayout), "Keyboard layout")]
        [TestCase(null, nameof(KeyboardStrings.KeyboardLayout), "Keyboard layout")]
        public void TestLanguageLookup(string? language, string key, string expected)
            => Assert.That(KeyboardStrings.Translate(key, language), Is.EqualTo(expected));

        [Test]
        public void TestTooltipsSayWhenTheyApply()
        {
            Assert.That(KeyboardStrings.KeyboardLayoutTooltip.ToString(), Is.EqualTo("What is printed on the keys. Applies the next time a map starts."));
        }

        [Test]
        public void TestLayoutNamesAreWrittenInTheirOwnLanguage()
        {
            var names = Enum.GetValues<KeyboardLayoutType>().Select(l => l.GetLocalisableDescription().ToString()).ToList();

            Assert.That(names, Is.Unique);
            Assert.That(names, Has.None.EqualTo(string.Empty));
            Assert.That(KeyboardLayoutType.TurkishQ.GetLocalisableDescription().ToString(), Is.EqualTo("Türkçe Q"));
            Assert.That(KeyboardLayoutType.Korean.GetLocalisableDescription().ToString(), Does.StartWith("한국어"));
            Assert.That(KeyboardType.Iso.GetLocalisableDescription().ToString(), Does.StartWith("ISO"));
        }

        private class FakeCircle : HitObject, IHasPosition, IHasCombo
        {
            public bool NewCombo { get; set; }
            public int ComboOffset { get; set; }
            public Vector2 Position { get; set; }
            public float X { get => Position.X; set => Position = new Vector2(value, Y); }
            public float Y { get => Position.Y; set => Position = new Vector2(X, value); }
        }

        private class FakeSlider : HitObject, IHasPathWithRepeats, IHasPosition
        {
            public double Duration { get; set; }
            public double EndTime { get => StartTime + Duration; set => Duration = value - StartTime; }
            public double Distance => 300;
            public SliderPath Path { get; } = new SliderPath(PathType.LINEAR, new[] { Vector2.Zero, new Vector2(300, 0) });
            public int RepeatCount { get; set; }
            public IList<IList<HitSampleInfo>> NodeSamples { get; } = new List<IList<HitSampleInfo>>();
            public Vector2 Position { get; set; } = new Vector2(100, 100);
            public float X { get => Position.X; set => Position = new Vector2(value, Y); }
            public float Y { get => Position.Y; set => Position = new Vector2(X, value); }
        }

        private class FakeSpinner : HitObject, IHasDuration
        {
            public double Duration { get; set; }
            public double EndTime { get => StartTime + Duration; set => Duration = value - StartTime; }
        }

        private class FakeManiaHold : HitObject, IHasDuration, IHasColumn
        {
            public double Duration { get; set; }
            public double EndTime { get => StartTime + Duration; set => Duration = value - StartTime; }
            public int Column { get; set; }
        }
    }
}
