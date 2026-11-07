// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Keyboard.Beatmaps;
using osu.Game.Rulesets.Keyboard.Localisation;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
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

        [Test]
        public void TestEveryTextIsTranslatedIntoEveryLanguage()
        {
            foreach (string key in KeyboardStrings.AllKeys)
            {
                var row = KeyboardStrings.RowFor(key);
                Assert.That(row, Has.Count.EqualTo(KeyboardStrings.Languages.Length), $"{key} is missing a language");
                Assert.That(row, Has.All.Not.Empty, $"{key} has an empty translation");
            }
        }

        [Test]
        public void TestLanguageLookup()
        {
            Assert.That(KeyboardStrings.Translate(nameof(KeyboardStrings.KeySpace), "tr"), Is.EqualTo("Boşluk"));
            Assert.That(KeyboardStrings.Translate(nameof(KeyboardStrings.KeyboardLayout), "de"), Is.EqualTo("Tastaturlayout"));
            Assert.That(KeyboardStrings.Translate(nameof(KeyboardStrings.KeyboardLayout), "ja"), Is.EqualTo("キーボード配列"));
            Assert.That(KeyboardStrings.Translate(nameof(KeyboardStrings.KeyboardLayout), "xx"), Is.EqualTo("Keyboard layout"), "unknown language falls back to English");
            Assert.That(KeyboardStrings.Translate(nameof(KeyboardStrings.KeyboardLayout), null), Is.EqualTo("Keyboard layout"));
        }

        [Test]
        public void TestDropdownNamesComeFromTheTable()
        {
            // The settings dropdowns show these; they must resolve to our translations, not the raw enum name.
            foreach (var layout in Enum.GetValues<KeyboardLayoutType>())
                Assert.That(layout.GetLocalisableDescription().ToString(), Is.Not.EqualTo(layout.ToString()).Or.EqualTo("Dvorak").Or.EqualTo("Colemak"), $"{layout} has no translated name");

            Assert.That(KeyboardLayoutType.TurkishQ.GetLocalisableDescription().ToString(), Is.EqualTo("Turkish Q"));
            Assert.That(KeyboardType.Iso.GetLocalisableDescription().ToString(), Does.StartWith("ISO"));
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

        private class FakeManiaHold : HitObject, IHasDuration, IHasColumn
        {
            public double Duration { get; set; }
            public double EndTime { get => StartTime + Duration; set => Duration = value - StartTime; }
            public int Column { get; set; }
        }
    }
}
