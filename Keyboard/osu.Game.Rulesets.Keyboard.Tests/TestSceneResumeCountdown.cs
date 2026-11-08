// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    /// <summary>
    /// Pausing and resuming during play shows the same 3-2-1 countdown as osu!mania, and play only continues after it.
    /// Like osu!mania, pausing in a break (or the intro) resumes straight away, since there is nothing to miss.
    /// </summary>
    public partial class TestSceneResumeCountdown : PlayerTestScene
    {
        protected override bool HasCustomSteps => true;

        private bool longIntro;

        protected override Ruleset CreatePlayerRuleset() => new KeyboardRuleset();

        // The test scene's player has pausing turned off by default; in the real game it is on.
        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new TestPlayer(allowPause: true);

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap<KeyboardHitObject>
            {
                BeatmapInfo = new BeatmapInfo { Ruleset = ruleset, Difficulty = new BeatmapDifficulty() },
            };

            // Either a steady stream of notes (no breaks) or one far-away note (a long intro).
            if (longIntro)
                beatmap.HitObjects.Add(new KeyboardHitObject { Key = KeyboardAction.A, StartTime = 30000 });
            else
            {
                for (int i = 0; i < 60; i++)
                    beatmap.HitObjects.Add(new KeyboardHitObject { Key = i % 2 == 0 ? KeyboardAction.A : KeyboardAction.L, StartTime = 1000 + i * 400 });
            }

            return beatmap;
        }

        private void pauseAt(double time)
        {
            AddUntilStep($"playing until {time}ms", () => Player.GameplayClockContainer.CurrentTime > time);
            AddStep("pause", () => Player.Pause());
            AddUntilStep("paused", () => Player.GameplayClockContainer.IsPaused.Value);
            AddStep("resume", () => Player.Resume());
        }

        [Test]
        public void TestCountdownAfterPauseDuringPlay()
        {
            CreateTest(() => AddStep("notes all the way", () => longIntro = false));
            AddAssert("same countdown overlay as osu!mania", () => Player.DrawableRuleset.ResumeOverlay is DelayedResumeOverlay);

            pauseAt(3000);
            AddUntilStep("countdown is showing", () => Player.DrawableRuleset.ResumeOverlay.State.Value == Visibility.Visible);
            AddAssert("still paused during the countdown", () => Player.GameplayClockContainer.IsPaused.Value);
            AddUntilStep("countdown finishes and play continues", () => !Player.GameplayClockContainer.IsPaused.Value);
            AddAssert("countdown gone", () => Player.DrawableRuleset.ResumeOverlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestNoCountdownInTheIntro()
        {
            CreateTest(() => AddStep("long intro", () => longIntro = true));

            pauseAt(1000);
            AddUntilStep("play continues at once", () => !Player.GameplayClockContainer.IsPaused.Value);
            AddAssert("no countdown", () => Player.DrawableRuleset.ResumeOverlay.State.Value != Visibility.Visible);
        }
    }
}
