// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Keyboard.Replays
{
    public class KeyboardAutoGenerator : AutoGenerator<KeyboardReplayFrame>
    {
        /// <summary>How long autoplay holds each key down, in ms.</summary>
        private const double key_down_duration = 40;

        public new Beatmap<KeyboardHitObject> Beatmap => (Beatmap<KeyboardHitObject>)base.Beatmap;

        public KeyboardAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        protected override void GenerateFrames()
        {
            Frames.Add(new KeyboardReplayFrame());

            var events = new List<(double time, KeyboardAction action, bool down)>();

            foreach (KeyboardHitObject hitObject in Beatmap.HitObjects)
            {
                events.Add((hitObject.StartTime, hitObject.Key, true));
                // Spinners are held until they end; normal notes are tapped.
                double release = hitObject is KeyboardSpinner ? hitObject.GetEndTime() : hitObject.StartTime + key_down_duration;
                events.Add((release, hitObject.Key, false));
            }

            // Releases go before presses at the same instant so a key can be re-pressed cleanly.
            var held = new HashSet<KeyboardAction>();

            foreach (var (time, action, down) in events.OrderBy(e => e.time).ThenBy(e => e.down))
            {
                if (down)
                    held.Add(action);
                else
                    held.Remove(action);

                Frames.Add(new KeyboardReplayFrame(held) { Time = time });
            }
        }
    }
}
