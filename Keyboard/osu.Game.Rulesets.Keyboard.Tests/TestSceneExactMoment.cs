// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Keyboard.Objects.Drawables;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    /// <summary>
    /// The square shows the exact moment to press: it brightens towards it, is brightest when the note is due, and fades
    /// once the Great window is over. Nobody presses here, so the note can be looked at before, at and after its time.
    /// </summary>
    [TestFixture]
    public partial class TestSceneExactMoment : PlayerTestScene
    {
        protected override bool HasCustomSteps => true;

        protected override bool AllowFail => false;

        protected override Ruleset CreatePlayerRuleset() => new KeyboardRuleset();

        // Overall difficulty 0: Great is 79.5 ms and Ok 139.5 ms either side.
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap<KeyboardHitObject>
        {
            BeatmapInfo = new BeatmapInfo { Ruleset = ruleset, Difficulty = new BeatmapDifficulty { OverallDifficulty = 0 } },
            HitObjects = { new KeyboardHitObject { Key = KeyboardAction.F, StartTime = 5000 } },
        };

        private DrawableKeyboardHitObject note => Player.DrawableRuleset.Playfield.AllHitObjects.OfType<DrawableKeyboardHitObject>().Single();

        /// <summary>What the glow and the square's own alpha should be at a time, from the times and the easings used.</summary>
        private static (float glow, float fill) expectedAt(double now, (double glowStart, double due, double greatEnd, double okEnd) t)
        {
            float glow = 0;

            if (now >= t.glowStart && now < t.due)
            {
                // Easing.In: slow first, then faster and faster.
                double x = (now - t.glowStart) / (t.due - t.glowStart);
                glow = (float)(DrawableKeyboardHitObject.GLOW_PEAK * x * x);
            }
            else if (now >= t.due && now < t.greatEnd)
            {
                double x = (now - t.due) / (t.greatEnd - t.due);
                glow = (float)(DrawableKeyboardHitObject.GLOW_PEAK * (1 - x) * (1 - x));
            }

            float fill = DrawableKeyboardHitObject.FILL_ALPHA;

            if (now >= t.greatEnd)
            {
                double x = Math.Min(1, (now - t.greatEnd) / (t.okEnd - t.greatEnd));
                fill = (float)(DrawableKeyboardHitObject.FILL_ALPHA + (DrawableKeyboardHitObject.LATE_ALPHA - DrawableKeyboardHitObject.FILL_ALPHA) * x);
            }

            return (glow, fill);
        }

        [Test]
        public void TestBrightestWhenDueThenFades()
        {
            CreateTest();

            // Headless frames are far apart, so one pass over the note sees only a few moments of it. Play over it many
            // times (as a replay going back does) and check every moment seen against what it should be.
            int passes = 0, before = 0, rising = 0, falling = 0, late = 0;
            float worst = 0, brightest = 0;
            string worstAt = string.Empty;

            AddUntilStep("play over the note 40 times", () =>
            {
                var drawable = note;
                double now = drawable.Time.Current;

                if (now > 5190 || now < 4500)
                {
                    if (now > 5190)
                        passes++;

                    Player.GameplayClockContainer.Seek(4600 + passes * 7 % 100);
                    return passes >= 40;
                }

                if (drawable.Judged)
                    return false;

                var times = DrawableKeyboardHitObject.HighlightTimes(drawable.HitObject);
                var expected = expectedAt(now, times);

                float error = Math.Max(Math.Abs(drawable.GlowAlpha - expected.glow), Math.Abs(drawable.FillAlpha - expected.fill));

                if (error > worst)
                {
                    worst = error;
                    worstAt = $"at {now:0.0}: glow {drawable.GlowAlpha:0.000} (expected {expected.glow:0.000}), fill {drawable.FillAlpha:0.000} (expected {expected.fill:0.000})";
                }

                brightest = Math.Max(brightest, drawable.GlowAlpha);

                if (now < times.glowStart) before++;
                else if (now < times.due) rising++;
                else if (now < times.greatEnd) falling++;
                else late++;

                return false;
            });

            AddAssert("seen before, while brightening, while the glow goes and when late", () => new[] { before, rising, falling, late }, () => Has.All.GreaterThan(0));
            AddAssert("always as bright and as solid as it should be", () => worst < 0.02f ? "ok" : worstAt, () => Is.EqualTo("ok"));
            AddAssert("never brighter than at the exact moment", () => brightest, () => Is.LessThanOrEqualTo(DrawableKeyboardHitObject.GLOW_PEAK));
        }

        [Test]
        public void TestMissedNoteKeepsItsColour()
        {
            CreateTest();

            osu.Framework.Graphics.Colour4 before = default, whileGoing = default;
            float scaleWhileGoing = 1;

            AddUntilStep("note on screen", () => note.Time.Current > 4000 && note.Alpha == 1);
            AddStep("remember its colour", () => before = note.NoteColour);

            // Missed at 5199.5 (the end of the Meh window), then gone within 400 ms.
            AddUntilStep("look while it goes", () =>
            {
                var drawable = Player.DrawableRuleset.Playfield.AllHitObjects.OfType<DrawableKeyboardHitObject>().SingleOrDefault();

                if (drawable != null && drawable.Judged && drawable.Alpha > 0)
                {
                    whileGoing = drawable.NoteColour;
                    scaleWhileGoing = drawable.Scale.X;
                }

                return Player.GameplayClockContainer.CurrentTime > 5700;
            });

            AddAssert("was missed", () => Player.ScoreProcessor.Combo.Value == 0 && Player.ScoreProcessor.JudgedHits == 1);
            AddAssert("same colour, not red", () => whileGoing, () => Is.EqualTo(before));
            AddAssert("shrinks instead of growing", () => scaleWhileGoing, () => Is.LessThan(1));
        }

        [Test]
        public void TestTimesFollowTheHitWindows()
        {
            CreateTest();
            AddAssert("glow from 250 ms before, to the end of Great, fade to the end of Ok",
                () => DrawableKeyboardHitObject.HighlightTimes(note.HitObject), () => Is.EqualTo((4750d, 5000d, 5079.5d, 5139.5d)));
        }
    }
}
