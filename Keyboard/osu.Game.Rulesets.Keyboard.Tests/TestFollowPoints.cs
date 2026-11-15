// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Keyboard.UI.FollowPoints;
using osuTK;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    /// <summary>The numbers of osu!standard's follow points (FollowPointConnection in osu!lazer).</summary>
    [TestFixture]
    public class TestFollowPoints
    {
        [Test]
        public void TestPointsAreEvery32StartingAt48AndStopASpacingShort()
        {
            Assert.That(FollowPointConnection.PointDistances(200), Is.EqualTo(new[] { 48, 80, 112, 144 }));
            Assert.That(FollowPointConnection.PointDistances(80), Is.Empty);
            Assert.That(FollowPointConnection.PointDistances(900).Count(), Is.EqualTo(26));
        }

        [Test]
        public void TestFadeTimes()
        {
            var start = new KeyboardHitObject { StartTime = 1000, TimePreempt = 1400 };
            var end = new KeyboardHitObject { StartTime = 2000 };

            // Half-way: fades out when half the time between the notes has passed, in 800 ms before that.
            FollowPointConnection.GetFadeTimes(start, end, 0.5f, out double fadeIn, out double fadeOut);
            Assert.That(fadeOut, Is.EqualTo(1500));
            Assert.That(fadeIn, Is.EqualTo(700));

            // Faster than approach rate 10: appears later in proportion, as in osu!standard.
            start.TimePreempt = 225;
            FollowPointConnection.GetFadeTimes(start, end, 0.5f, out fadeIn, out _);
            Assert.That(fadeIn, Is.EqualTo(1100));
        }

        [Test]
        public void TestFollowPointsStartAfterASpinnerEnds()
        {
            // Not used between spinners and notes, but the time is taken from where the first object ends.
            var start = new KeyboardSpinner { StartTime = 1000, Duration = 500 };
            var end = new KeyboardHitObject { StartTime = 2500 };

            FollowPointConnection.GetFadeTimes(start, end, 0.5f, out _, out double fadeOut);
            Assert.That(fadeOut, Is.EqualTo(2000));
        }

        [Test]
        public void TestWhereThereAreNone()
        {
            var a = new KeyboardHitObject { StartTime = 1000 };
            var b = new KeyboardHitObject { StartTime = 2000 };
            Vector2 near = new Vector2(80, 0), far = new Vector2(500, 0);

            Assert.That(FollowPointConnection.Lifetime(a, b, Vector2.Zero, far), Is.Not.Null);
            Assert.That(FollowPointConnection.Lifetime(a, b, Vector2.Zero, near), Is.Null, "too close for a point");

            // Keys that touch: side by side (100), or diagonally on the next row (103, 112 or 125 with the rows' stagger).
            foreach (var touching in new[] { new Vector2(100, 0), new Vector2(25, 100), new Vector2(50, 100), new Vector2(75, 100) })
                Assert.That(FollowPointConnection.Lifetime(a, b, Vector2.Zero, touching), Is.Null, $"touching keys {touching}");

            // The nearest keys that do not touch: one key between them (200), or diagonally past the neighbour (160).
            foreach (var apart in new[] { new Vector2(200, 0), new Vector2(125, 100), new Vector2(0, 200) })
                Assert.That(FollowPointConnection.Lifetime(a, b, Vector2.Zero, apart), Is.Not.Null, $"keys apart {apart}");
            Assert.That(FollowPointConnection.Lifetime(a, new KeyboardHitObject { StartTime = 2000, NewCombo = true }, Vector2.Zero, far), Is.Null, "new combo");
            Assert.That(FollowPointConnection.Lifetime(a, new KeyboardSpinner { StartTime = 2000, Duration = 100 }, Vector2.Zero, far), Is.Null, "to a spinner");
            Assert.That(FollowPointConnection.Lifetime(new KeyboardSpinner { StartTime = 500, Duration = 100 }, b, Vector2.Zero, far), Is.Null, "from a spinner");
        }

        [Test]
        public void TestLifetimeCoversEveryPoint()
        {
            var a = new KeyboardHitObject { StartTime = 1000 };
            var b = new KeyboardHitObject { StartTime = 2000 };

            var lifetime = FollowPointConnection.Lifetime(a, b, Vector2.Zero, new Vector2(200, 0))!.Value;

            // First point (48 of 200): fades out at 1240, in at 440. Last (144 of 200): out at 1720, gone 400 later.
            Assert.That(lifetime.start, Is.EqualTo(440).Within(0.01));
            Assert.That(lifetime.end, Is.EqualTo(2120).Within(0.01));
        }
    }
}
