// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    /// <summary>
    /// Presses every physical key through the real input pipeline and checks which ruleset action comes out.
    /// This catches binding or mapping mistakes for any single key (e.g. one that "does nothing" or triggers a neighbour).
    /// </summary>
    [TestFixture]
    public partial class TestSceneKeyHitting : PlayerTestScene
    {
        private ActionProbe probe = null!;

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

            // One early note so there is no "skip intro" (which takes Space for itself while it is offered),
            // the rest far in the future so nothing else is judged while we poke at the input.
            beatmap.HitObjects.Add(new KeyboardHitObject { Key = KeyboardAction.Q, StartTime = 1000 });

            foreach (var key in KeyboardLayout.Default.Keys)
                beatmap.HitObjects.Add(new KeyboardHitObject { Key = key.Action, StartTime = 600000 });

            return beatmap;
        }

        [Test]
        public void TestEveryKeyProducesItsAction()
        {
            CreateTest();

            AddStep("attach input probe", () =>
            {
                probe = new ActionProbe();

                // AddInternal is protected; the probe only needs to sit inside the ruleset's input manager.
                typeof(CompositeDrawable)
                    .GetMethod("AddInternal", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(Player.DrawableRuleset.Playfield, new object[] { probe });
            });

            // Every key that can exist on any keyboard type, so ANSI-only and ISO-only keys are covered too.
            foreach (var definition in KeyboardLayout.AllBindableKeys)
            {
                var key = (Key)(int)definition.Key;
                var expected = definition.Action;

                AddStep($"press {key}", () =>
                {
                    probe.Pressed.Clear();
                    InputManager.PressKey(key);
                });

                AddStep($"{key} must produce {expected} and nothing else", () =>
                {
                    string got = probe.Pressed.Count == 0 ? "nothing" : string.Join(", ", probe.Pressed);
                    Assert.That(probe.Pressed.SequenceEqual(new[] { expected }), Is.True, $"pressing {key} produced: {got} (wanted {expected})");
                });

                AddStep($"release {key}", () => InputManager.ReleaseKey(key));
            }
        }

        private partial class ActionProbe : Container, IKeyBindingHandler<KeyboardAction>
        {
            public readonly List<KeyboardAction> Pressed = new List<KeyboardAction>();

            public bool OnPressed(KeyBindingPressEvent<KeyboardAction> e)
            {
                Pressed.Add(e.Action);
                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<KeyboardAction> e)
            {
            }
        }
    }
}
