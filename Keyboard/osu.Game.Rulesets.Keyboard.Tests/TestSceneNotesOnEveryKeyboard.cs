// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Keyboard.Configuration;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Keyboard.Objects.Drawables;
using osu.Game.Rulesets.Keyboard.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    /// <summary>
    /// Loads a real play with each keyboard type / layout selected and checks that notes show up inside their keys.
    /// </summary>
    [TestFixture]
    public partial class TestSceneNotesOnEveryKeyboard : PlayerTestScene
    {
        protected override bool HasCustomSteps => true;

        protected override Ruleset CreatePlayerRuleset() => new KeyboardRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap<KeyboardHitObject>
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Ruleset = ruleset,
                    Difficulty = new BeatmapDifficulty(),
                }
            };

            // One note per key on the keyboard, all close together so they are alive at the same time.
            foreach (var key in KeyboardLayout.Default.Keys.Where(k => k.Playable && k.Action != KeyboardAction.Space))
                beatmap.HitObjects.Add(new KeyboardHitObject { Key = key.Action, StartTime = 3000 });

            return beatmap;
        }

        [TestCase(KeyboardLayoutType.EnglishQwerty, KeyboardType.Ansi)]
        [TestCase(KeyboardLayoutType.EnglishQwerty, KeyboardType.Iso)]
        [TestCase(KeyboardLayoutType.TurkishQ, KeyboardType.Iso)]
        [TestCase(KeyboardLayoutType.TurkishF, KeyboardType.Iso)]
        [TestCase(KeyboardLayoutType.TurkishF, KeyboardType.Ansi)]
        public void TestNotesSitInsideTheirKeys(KeyboardLayoutType layoutType, KeyboardType keyboardType)
        {
            CreateTest(() => AddStep($"choose {layoutType} on {keyboardType}", () =>
            {
                var config = (KeyboardRulesetConfigManager)RulesetConfigs.GetConfigFor(new KeyboardRuleset())!;
                config.SetValue(KeyboardRulesetSetting.Layout, layoutType);
                config.SetValue(KeyboardRulesetSetting.KeyboardType, keyboardType);
            }));

            AddUntilStep("notes are on screen", () => Player.DrawableRuleset.Playfield.AllHitObjects.OfType<DrawableKeyboardHitObject>().Count() == 33);

            AddStep("playfield uses the chosen layout", () =>
            {
                var layout = ((KeyboardPlayfield)Player.DrawableRuleset.Playfield).Layout;
                var config = (KeyboardRulesetConfigManager)RulesetConfigs.GetConfigFor(new KeyboardRuleset())!;

                Assert.That(layout.LayoutType == layoutType && layout.KeyboardType == keyboardType,
                    $"wanted {layoutType}/{keyboardType} but playfield has {layout.LayoutType}/{layout.KeyboardType}; " +
                    $"config now says {config.Get<KeyboardLayoutType>(KeyboardRulesetSetting.Layout)}/{config.Get<KeyboardType>(KeyboardRulesetSetting.KeyboardType)}");
            });

            AddAssert("every note is inside the keyboard and on its own key", () =>
            {
                var notes = Player.DrawableRuleset.Playfield.AllHitObjects.OfType<DrawableKeyboardHitObject>().ToList();
                var layout = ((KeyboardPlayfield)Player.DrawableRuleset.Playfield).Layout;

                float width = layout.Width * KeyboardPlayfield.KEY_SIZE, height = layout.Height * KeyboardPlayfield.KEY_SIZE;
                bool inside = notes.All(n => n.Position.X > 0 && n.Position.X < width && n.Position.Y > 0 && n.Position.Y < height && n.Size.X > 0);
                bool distinctKeys = notes.Select(n => n.HitObject.Key).Distinct().Count() == notes.Count;
                bool matchesLayout = notes.All(n =>
                {
                    var key = layout[n.HitObject.Key];
                    return (n.Position - key.Centre * KeyboardPlayfield.KEY_SIZE).Length < 0.01f;
                });

                return inside && distinctKeys && matchesLayout;
            });
        }
    }
}
