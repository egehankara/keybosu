// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    /// <summary>
    /// osu!lazer saves key bindings by the numeric value of <see cref="KeyboardAction"/>. If a number ever changes,
    /// everyone's saved bindings silently point at the wrong keys (this really happened: M triggered N).
    /// This test pins the numbers. Add new actions with new numbers, never renumber existing ones.
    /// </summary>
    [TestFixture]
    public class TestActionNumbering
    {
        private static readonly (string name, int value)[] pinned =
        {
            ("Q", 0), ("W", 1), ("E", 2), ("R", 3), ("T", 4), ("Y", 5), ("U", 6), ("I", 7), ("O", 8), ("P", 9),
            ("BracketLeft", 10), ("BracketRight", 11),
            ("A", 12), ("S", 13), ("D", 14), ("F", 15), ("G", 16), ("H", 17), ("J", 18), ("K", 19), ("L", 20),
            ("Semicolon", 21), ("Quote", 22),
            ("Z", 23), ("X", 24), ("C", 25), ("V", 26), ("B", 27), ("N", 28), ("M", 29),
            ("Comma", 30), ("Period", 31), ("Slash", 32),
            ("BackSlash", 40), ("NonUSBackSlash", 41),
        };

        [Test]
        public void TestNumbersNeverChange()
        {
            foreach (var (name, value) in pinned)
                Assert.That((int)Enum.Parse<KeyboardAction>(name), Is.EqualTo(value), $"{name} was renumbered");
        }

        [Test]
        public void TestNoActionIsMissingFromThePinnedList()
        {
            var unpinned = Enum.GetNames<KeyboardAction>().Except(pinned.Select(p => p.name)).ToList();
            Assert.That(unpinned, Is.Empty, "new action(s) must be added to the pinned list: " + string.Join(", ", unpinned));
        }

        [Test]
        public void TestNoTwoActionsShareAKey()
        {
            // lazer wipes the binding of any key that is assigned to more than one action.
            var duplicates = KeyboardLayout.AllBindableKeys.GroupBy(k => k.Key).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            Assert.That(duplicates, Is.Empty, "keys bound to several actions: " + string.Join(", ", duplicates));
        }
    }
}
