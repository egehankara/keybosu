// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    [TestFixture]
    public class TestKeyboardLayouts
    {
        private static IEnumerable<TestCaseData> allCombinations()
        {
            foreach (var layout in Enum.GetValues<KeyboardLayoutType>())
            {
                foreach (var type in Enum.GetValues<KeyboardType>())
                    yield return new TestCaseData(layout, type).SetName($"{layout}_{type}");
            }
        }

        [TestCaseSource(nameof(allCombinations))]
        public void TestNothingOverlapsAndNothingIsNegative(KeyboardLayoutType layoutType, KeyboardType type)
        {
            var layout = KeyboardLayout.Create(layoutType, type);

            var rects = layout.Keys.Select(k => (name: $"key {k.Label}", x: k.X, y: k.Y, w: k.Width, h: 1f))
                              .Concat(layout.Decorations.Select(d => (name: $"decoration {d.Label}", x: d.X, y: d.Y, w: d.Width, h: d.Height)))
                              .ToList();

            foreach (var r in rects)
            {
                Assert.That(r.x, Is.GreaterThanOrEqualTo(0), $"{r.name} sticks out on the left");
                Assert.That(r.y, Is.GreaterThanOrEqualTo(0), $"{r.name} sticks out on the top");
                Assert.That(r.x + r.w, Is.LessThanOrEqualTo(layout.Width + 0.001f), $"{r.name} sticks out on the right");
            }

            for (int i = 0; i < rects.Count; i++)
            {
                for (int j = i + 1; j < rects.Count; j++)
                {
                    var a = rects[i];
                    var b = rects[j];
                    bool overlaps = a.x < b.x + b.w - 0.001f && b.x < a.x + a.w - 0.001f && a.y < b.y + b.h - 0.001f && b.y < a.y + a.h - 0.001f;
                    Assert.That(overlaps, Is.False, $"{a.name} overlaps {b.name}");
                }
            }
        }

        [TestCaseSource(nameof(allCombinations))]
        public void TestSharedKeysAreTheSameEverywhere(KeyboardLayoutType layoutType, KeyboardType type)
        {
            var layout = KeyboardLayout.Create(layoutType, type);
            var reference = KeyboardLayout.Default.Keys.Where(k => k.Playable).Select(k => k.Action).OrderBy(a => a);

            Assert.That(layout.Keys.Where(k => k.Playable).Select(k => k.Action).OrderBy(a => a), Is.EqualTo(reference),
                "conversion relies on the playable keys being identical for every layout and keyboard type");
            Assert.That(layout.Keys.Count(k => !k.Playable), Is.EqualTo(1), "each keyboard type has exactly one extra (dimmed) key");
        }

        [TestCaseSource(nameof(allCombinations))]
        public void TestEveryLetterKeyHasItsOwnLabel(KeyboardLayoutType layoutType, KeyboardType type)
        {
            var labels = KeyboardLayout.Create(layoutType, type).Keys.Where(k => k.Playable && k.Action != KeyboardAction.Space).Select(k => k.Label).ToList();

            Assert.That(labels, Has.Count.EqualTo(33));
            Assert.That(labels, Is.Unique, "two keys show the same character");
            Assert.That(labels, Has.All.Not.Empty);
        }

        [TestCaseSource(nameof(allCombinations))]
        public void TestSpaceBarIsBelowEverything(KeyboardLayoutType layoutType, KeyboardType type)
        {
            var layout = KeyboardLayout.Create(layoutType, type);
            var space = layout[KeyboardAction.Space];

            Assert.That(space.Y, Is.EqualTo(layout.Keys.Max(k => k.Y)), "space is the bottom row");
            Assert.That(space.Width, Is.GreaterThan(4), "space is a long bar");
        }

        [Test]
        public void TestIsoHasTallEnterAndAnsiHasFlatEnter()
        {
            var ansi = KeyboardLayout.Create(KeyboardLayoutType.EnglishQwerty, KeyboardType.Ansi).Decorations.Single(d => d.Label == "Enter");
            var iso = KeyboardLayout.Create(KeyboardLayoutType.EnglishQwerty, KeyboardType.Iso).Decorations.Single(d => d.Label == "Enter");

            Assert.That(ansi.Height, Is.EqualTo(1f), "ANSI Enter is one row tall");
            Assert.That(ansi.Width, Is.GreaterThan(1f), "ANSI Enter is wider than a normal key");
            Assert.That(iso.Height, Is.EqualTo(2f), "ISO Enter spans two rows");
        }

        [Test]
        public void TestTurkishLayoutsHaveAllTurkishLetters()
        {
            foreach (var type in new[] { KeyboardLayoutType.TurkishQ, KeyboardLayoutType.TurkishF })
            {
                var labels = KeyboardLayout.Create(type, KeyboardType.Iso).Keys.Select(k => k.Label).ToHashSet();

                foreach (char c in "ÇĞİÖŞÜ")
                    Assert.That(labels, Does.Contain(c.ToString()), $"{type} is missing {c}");
            }
        }
    }
}
