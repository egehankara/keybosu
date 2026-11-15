// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets.Keyboard.Configuration;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Keyboard.UI;
using osu.Game.Rulesets.Keyboard.UI.FollowPoints;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    public static class FollowPointTestHelpers
    {
        /// <summary>
        /// Q at 6000, P (nine keys = 900 away) at 7000, O next to it at 7500, then a new combo on A at 8500, a spinner, and Z.
        /// Late enough that the test's first steps are over before anything shows.
        /// </summary>
        public static IBeatmap Beatmap(RulesetInfo ruleset) => new Beatmap<KeyboardHitObject>
        {
            BeatmapInfo = new BeatmapInfo { Ruleset = ruleset, Difficulty = new BeatmapDifficulty() },
            HitObjects =
            {
                new KeyboardHitObject { Key = KeyboardAction.Q, StartTime = 6000 },
                new KeyboardHitObject { Key = KeyboardAction.P, StartTime = 7000 },
                new KeyboardHitObject { Key = KeyboardAction.O, StartTime = 7500 },
                new KeyboardHitObject { Key = KeyboardAction.A, StartTime = 8500, NewCombo = true },
                new KeyboardSpinner { StartTime = 9000, Duration = 500 },
                new KeyboardHitObject { Key = KeyboardAction.Z, StartTime = 10500 },
            },
        };

        public static FollowPointRenderer Renderer(TestPlayer player) => ((KeyboardPlayfield)player.DrawableRuleset.Playfield).FollowPoints;

        public static FollowPointConnection? ConnectionTo(TestPlayer player, double endTime)
            => Renderer(player).Connections.SingleOrDefault(c => c.End.StartTime == endTime);
    }

    /// <summary>
    /// Watches every follow point in every frame: each fades in once and out once. A point showing again after it started
    /// to go would be seen as flicker.
    /// </summary>
    public class FollowPointWatcher
    {
        private readonly Dictionary<FollowPoint, (float alpha, bool falling)> seen = new Dictionary<FollowPoint, (float, bool)>();

        public readonly List<string> Problems = new List<string>();
        public int PointsSeen => seen.Count;

        /// <summary>How many points were seen fully showing at some moment.</summary>
        public int PointsSeenShowing => showed.Count;

        private readonly HashSet<FollowPoint> showed = new HashSet<FollowPoint>();
        public readonly HashSet<FollowPointStyle> Styles = new HashSet<FollowPointStyle>();

        public void Look(TestPlayer player)
        {
            double now = player.GameplayClockContainer.CurrentTime;

            foreach (var connection in FollowPointTestHelpers.Renderer(player).Connections)
            {
                foreach (var point in connection.Points)
                {
                    Styles.Add(point.Style);

                    if (point.Alpha > 0.95f)
                        showed.Add(point);

                    if (seen.TryGetValue(point, out var before))
                    {
                        if (before.falling && point.Alpha > before.alpha + 0.02f)
                            Problems.Add($"{now:0}: a point to the note at {connection.End.StartTime} came back {before.alpha:0.00} -> {point.Alpha:0.00}");

                        seen[point] = (point.Alpha, before.falling || point.Alpha < before.alpha - 0.01f);
                    }
                    else
                        seen[point] = (point.Alpha, false);
                }
            }
        }
    }

    [TestFixture]
    public partial class TestSceneFollowPoints : PlayerTestScene
    {
        protected override bool Autoplay => true;

        protected override bool HasCustomSteps => true;

        protected override Ruleset CreatePlayerRuleset() => new KeyboardRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => FollowPointTestHelpers.Beatmap(ruleset);

        [Test]
        public void TestPointsComeAndGoInOrderLikeOsu()
        {
            CreateTest(() => AddStep("follow points on", () => setShown(true)));

            // Only Q to P. None from P to O (their keys touch), to A (new combo), to or from the spinner.
            AddAssert("one connection in the map", () => FollowPointTestHelpers.Renderer(Player).ConnectionCount, () => Is.EqualTo(1));

            int count = 0;
            Vector2 firstRestsAt = Vector2.Zero, qCentre = Vector2.Zero, pCentre = Vector2.Zero;
            float firstRotation = 0, firstScale = 0;
            float firstEarly = 0, lastEarly = 1, firstLate = 1, lastLate = 0;
            bool goneAfter = false;

            // Q to P is 900 apart: 26 points. The first (48 along) fades in from 5253 and starts to go at 6053;
            // the last (848 along) fades in from 6142 and starts to go at 6942.
            AddUntilStep("watch Q to P", () =>
            {
                double now = Player.GameplayClockContainer.CurrentTime;
                var connection = FollowPointTestHelpers.ConnectionTo(Player, 7000);

                if (connection != null)
                {
                    var points = connection.Points;
                    count = points.Count;

                    if (now >= 5700 && now < 6050)
                    {
                        firstEarly = Math.Max(firstEarly, points[0].Alpha);
                        lastEarly = Math.Min(lastEarly, points[^1].Alpha);
                        firstRestsAt = points[0].Position;
                        firstRotation = points[0].Rotation;
                        firstScale = points[0].Scale.X;

                        var layout = ((KeyboardPlayfield)Player.DrawableRuleset.Playfield).Layout;
                        qCentre = layout[KeyboardAction.Q].Centre * KeyboardPlayfield.KEY_SIZE;
                        pCentre = layout[KeyboardAction.P].Centre * KeyboardPlayfield.KEY_SIZE;
                    }

                    if (now >= 6550 && now < 6940)
                    {
                        firstLate = Math.Min(firstLate, points[0].Alpha);
                        lastLate = Math.Max(lastLate, points[^1].Alpha);
                    }
                }

                if (now >= 7500)
                    goneAfter = connection == null;

                return now >= 7500;
            });

            AddAssert("26 points", () => count, () => Is.EqualTo(26));
            AddAssert("near Q shows first", () => firstEarly, () => Is.EqualTo(1));
            AddAssert("near P not yet", () => lastEarly, () => Is.EqualTo(0));
            AddAssert("near Q gone later", () => firstLate, () => Is.EqualTo(0));
            AddAssert("near P shows later", () => lastLate, () => Is.EqualTo(1));
            AddAssert("first point rests 48 from Q towards P", () => firstRestsAt, () => Is.EqualTo(qCentre + (pCentre - qCentre).Normalized() * 48).Using<Vector2>((a, b) => (a - b).Length < 0.5f));
            AddAssert("points along the row", () => firstRotation, () => Is.EqualTo(0).Within(0.01f));
            AddAssert("settled at normal size", () => firstScale, () => Is.EqualTo(1));
            AddAssert("connection removed when done", () => goneAfter);
        }

        private void setShown(bool shown) => ((KeyboardRulesetConfigManager)RulesetConfigs.GetConfigFor(new KeyboardRuleset())!).SetValue(KeyboardRulesetSetting.ShowFollowPoints, shown);

        [Test]
        public void TestSettingTurnsThemOff()
        {
            AddAssert("on by default", () => new KeyboardRulesetConfigManager(null, new KeyboardRuleset().RulesetInfo).Get<bool>(KeyboardRulesetSetting.ShowFollowPoints));

            CreateTest(() => AddStep("turn follow points off", () => setShown(false)));
            AddAssert("no connections", () => FollowPointTestHelpers.Renderer(Player).ConnectionCount, () => Is.EqualTo(0));
            AddUntilStep("play to where they would show", () => Player.GameplayClockContainer.CurrentTime > 6000);
            AddAssert("none on screen", () => FollowPointTestHelpers.Renderer(Player).Connections, () => Is.Empty);

            AddStep("turn back on", () => setShown(true));
        }

        [Test]
        public void TestNoFlicker()
        {
            var watcher = new FollowPointWatcher();
            CreateTest(() => AddStep("follow points on", () => setShown(true)));
            AddUntilStep("watch", () =>
            {
                watcher.Look(Player);
                return Player.GameplayClockContainer.CurrentTime > 8000;
            });
            AddAssert("saw points", () => watcher.PointsSeen, () => Is.GreaterThan(20));
            AddAssert("they really showed", () => watcher.PointsSeenShowing, () => Is.GreaterThan(10));
            AddAssert("not the skin's images in this skin", () => watcher.Styles, () => Has.None.EqualTo(FollowPointStyle.Legacy));
            AddAssert("each point came and went once", () => watcher.Problems, () => Is.Empty);
        }
    }

    /// <summary>With osu!'s classic skin the skin's own followpoint image is used.</summary>
    [TestFixture]
    public partial class TestSceneFollowPointsLegacySkin : LegacySkinPlayerTestScene
    {
        protected override bool Autoplay => true;

        protected override bool HasCustomSteps => true;

        protected override Ruleset CreatePlayerRuleset() => new KeyboardRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => FollowPointTestHelpers.Beatmap(ruleset);

        [Test]
        public void TestSkinImageUsedWithoutFlicker()
        {
            var watcher = new FollowPointWatcher();
            CreateTest();
            AddUntilStep("watch", () =>
            {
                watcher.Look(Player);
                return Player.GameplayClockContainer.CurrentTime > 8000;
            });
            AddAssert("saw points", () => watcher.PointsSeen, () => Is.GreaterThan(20));
            AddAssert("they really showed", () => watcher.PointsSeenShowing, () => Is.GreaterThan(10));
            AddAssert("all the skin's image", () => watcher.Styles, () => Is.EquivalentTo(new[] { FollowPointStyle.Legacy }));
            AddAssert("each point came and went once", () => watcher.Problems, () => Is.Empty);
        }
    }

    /// <summary>A real converted map, played through: follow points everywhere, none flickering.</summary>
    [TestFixture]
    public partial class TestSceneFollowPointsRealMap : PlayerTestScene
    {
        protected override bool Autoplay => true;

        protected override bool HasCustomSteps => true;

        protected override Ruleset CreatePlayerRuleset() => new KeyboardRuleset();

        [Test]
        public void TestNoFlicker()
        {
            var watcher = new FollowPointWatcher();
            CreateTest();
            AddUntilStep("watch 15 s of play", () =>
            {
                watcher.Look(Player);
                return Player.GameplayClockContainer.CurrentTime > 15000;
            });
            AddAssert("saw points", () => watcher.PointsSeen, () => Is.GreaterThan(50));
            AddAssert("they really showed", () => watcher.PointsSeenShowing, () => Is.GreaterThan(25));
            AddAssert("each point came and went once", () => watcher.Problems, () => Is.Empty);
        }
    }

    /// <summary>Changing the skin during play changes the follow points on screen at once, as in osu!standard.</summary>
    [TestFixture]
    public partial class TestSceneFollowPointsSkinChange : PlayerTestScene
    {
        [Resolved]
        private SkinManager skins { get; set; } = null!;

        protected override bool Autoplay => true;

        protected override bool HasCustomSteps => true;

        protected override Ruleset CreatePlayerRuleset() => new KeyboardRuleset();

        // Q and P in turn for two minutes: follow points are on screen the whole time, however long the skins take to load.
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap<KeyboardHitObject> { BeatmapInfo = new BeatmapInfo { Ruleset = ruleset, Difficulty = new BeatmapDifficulty() } };

            for (int i = 0; i < 120; i++)
                beatmap.HitObjects.Add(new KeyboardHitObject { Key = i % 2 == 0 ? KeyboardAction.Q : KeyboardAction.P, StartTime = 2000 + i * 1000 });

            return beatmap;
        }

        private FollowPointStyle[] stylesOnScreen() => FollowPointTestHelpers.Renderer(Player).Connections.SelectMany(c => c.Points).Select(p => p.Style).Distinct().ToArray();

        [Test]
        public void TestEverySkinHasItsOwnLook()
        {
            Live<SkinInfo>? original = null;

            AddStep("remember skin", () => original = skins.CurrentSkinInfo.Value);
            CreateTest();
            AddUntilStep("follow points on screen", () => stylesOnScreen().Length > 0);

            AddStep("Argon", () => skins.CurrentSkinInfo.Value = ArgonSkin.CreateInfo().ToLiveUnmanaged());
            AddUntilStep("pink chevrons", () => stylesOnScreen(), () => Is.EqualTo(new[] { FollowPointStyle.Argon }));

            AddStep("classic", () => skins.CurrentSkinInfo.Value = skins.DefaultClassicSkin.SkinInfo);
            AddUntilStep("the skin's image", () => stylesOnScreen(), () => Is.EqualTo(new[] { FollowPointStyle.Legacy }));

            AddStep("Triangles", () => skins.CurrentSkinInfo.Value = TrianglesSkin.CreateInfo().ToLiveUnmanaged());
            AddUntilStep("glowing dots", () => stylesOnScreen(), () => Is.EqualTo(new[] { FollowPointStyle.Default }));

            AddStep("change back", () => skins.CurrentSkinInfo.Value = original!);
        }
    }
}
