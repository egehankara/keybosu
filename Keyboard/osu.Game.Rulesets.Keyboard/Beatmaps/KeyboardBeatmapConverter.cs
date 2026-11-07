// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Keyboard.Beatmaps
{
    /// <summary>
    /// Converts any beatmap by mapping each object's playfield position onto the nearest key.
    /// </summary>
    public class KeyboardBeatmapConverter : BeatmapConverter<KeyboardHitObject>
    {
        // osu!standard playfield size.
        private const float osu_width = 512;
        private const float osu_height = 384;

        /// <summary>
        /// Two objects closer together than this (in ms) are never put on the same key,
        /// otherwise their filling squares would overlap in a single cell.
        /// </summary>
        private const double min_same_key_gap = 350;

        private readonly KeyboardLayout layout = KeyboardLayout.Qwerty;
        private readonly Dictionary<KeyboardAction, SortedSet<double>> usedTimes = new Dictionary<KeyboardAction, SortedSet<double>>();

        public KeyboardBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
        }

        public override bool CanConvert() => true;

        protected override IEnumerable<KeyboardHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            var random = new Random((int)original.StartTime);

            bool hasPosition = original is IHasPosition;
            Vector2 startPosition = (original as IHasPosition)?.Position ?? Vector2.Zero;

            if (original is IHasDuration duration)
            {
                if (original is IHasPath path)
                {
                    // Slider: a note on the head and another one where the slider ends.
                    int repeats = (original as IHasRepeats)?.RepeatCount ?? 0;
                    Vector2 endPosition = repeats % 2 == 0 ? startPosition + path.Path.PositionAt(1) : startPosition;

                    yield return create(original.StartTime, nodeSamples(original, true), target(startPosition, hasPosition, random));
                    yield return create(duration.EndTime, nodeSamples(original, false), target(endPosition, hasPosition, random));
                }
                else
                {
                    // Spinner and similar: a single note when it ends.
                    yield return create(duration.EndTime, original.Samples, target(startPosition, hasPosition, random));
                }

                yield break;
            }

            yield return create(original.StartTime, original.Samples, target(startPosition, hasPosition, random));
        }

        /// <summary>Converts an osu! position into key units. Objects without a position get a deterministic random one.</summary>
        private Vector2 target(Vector2 position, bool hasPosition, Random random)
        {
            if (!hasPosition)
                return new Vector2(random.NextSingle() * layout.Width, random.NextSingle() * layout.Height);

            return new Vector2(position.X / osu_width * layout.Width, position.Y / osu_height * layout.Height);
        }

        private KeyboardHitObject create(double time, IList<HitSampleInfo> samples, Vector2 target)
        {
            return new KeyboardHitObject
            {
                Key = pickKey(target, time),
                StartTime = time,
                Samples = samples,
            };
        }

        private KeyboardAction pickKey(Vector2 target, double time)
        {
            var byDistance = layout.Keys.OrderBy(k => Vector2.DistanceSquared(k.Centre, target)).ToList();

            // Nearest key that has not been used around this time; if every key is busy, just take the nearest.
            var chosen = byDistance.Cast<KeyDefinition?>().FirstOrDefault(k => isFree(k!.Value.Action, time)) ?? byDistance[0];

            if (!usedTimes.TryGetValue(chosen.Action, out var set))
                usedTimes[chosen.Action] = set = new SortedSet<double>();

            set.Add(time);
            return chosen.Action;
        }

        private bool isFree(KeyboardAction action, double time)
        {
            if (!usedTimes.TryGetValue(action, out var set))
                return true;

            return set.GetViewBetween(time - min_same_key_gap, time + min_same_key_gap).Count == 0;
        }

        private static IList<HitSampleInfo> nodeSamples(HitObject original, bool head)
        {
            if (original is IHasRepeats repeats && repeats.NodeSamples.Count > 0)
                return head ? repeats.NodeSamples[0] : repeats.NodeSamples[^1];

            return original.Samples;
        }
    }
}
