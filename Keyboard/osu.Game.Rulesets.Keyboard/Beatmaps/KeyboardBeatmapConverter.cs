// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
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

        /// <summary>Never closer than this, even on maps with a very high approach rate.</summary>
        private const double min_same_key_gap = 350;

        /// <summary>
        /// A slider, or one pass of a reverse slider, shorter than this many beats counts as short.
        /// Just under 1.5, so that real 1.5 beat sliders are not caught by rounding in their length.
        /// </summary>
        private const double short_slider_beats = 1.45;

        /// <summary>
        /// Two notes on the same key closer together than this (in ms) would be on screen at the same time,
        /// one square growing inside the other. Worked out from the map's approach rate on first use.
        /// </summary>
        private double? sameKeyGap;

        private readonly KeyboardLayout layout = KeyboardLayout.Default;

        /// <summary>Keys notes can land on: every playable key except Space (Space is only for spinners).</summary>
        private readonly KeyDefinition[] noteKeys;

        /// <summary>
        /// The rectangle, in key units, covered by <see cref="noteKeys"/>. osu! positions are stretched over this area
        /// only, so the bottom of the osu! playfield maps to the bottom letter row rather than to the empty Space row.
        /// </summary>
        private readonly float areaLeft, areaTop, areaWidth, areaHeight;
        private readonly Dictionary<KeyboardAction, SortedSet<double>> usedTimes = new Dictionary<KeyboardAction, SortedSet<double>>();

        public KeyboardBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
            noteKeys = layout.Keys.Where(k => k.Playable && k.Action != KeyboardAction.Space).ToArray();

            areaLeft = noteKeys.Min(k => k.X);
            areaTop = noteKeys.Min(k => k.Y);
            areaWidth = noteKeys.Max(k => k.X + k.Width) - areaLeft;
            areaHeight = noteKeys.Max(k => k.Y + 1) - areaTop;
        }

        public override bool CanConvert() => true;

        protected override Beatmap<KeyboardHitObject> CreateBeatmap() => new KeyboardBeatmap();

        protected override IEnumerable<KeyboardHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            // A note starts filling TimePreempt before it is due (same formula as KeyboardHitObject).
            sameKeyGap ??= Math.Max(min_same_key_gap,
                IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.ApproachRate, 1800, 1200, 450) + KeyboardHitObject.SAME_KEY_CLEARANCE);

            var random = new Random((int)original.StartTime);

            bool hasPosition = original is IHasPosition;
            Vector2 startPosition = (original as IHasPosition)?.Position ?? Vector2.Zero;

            if (original is IHasDuration duration)
            {
                if (original is IHasPath path)
                {
                    // Slider: a note on the head, one on every turn of a reverse slider, and one where it ends.
                    // Short sliders are only the head, and turns that come quickly get no note.
                    int repeats = (original as IHasRepeats)?.RepeatCount ?? 0;
                    double shortLength = beatmap.ControlPointInfo.TimingPointAt(original.StartTime).BeatLength * short_slider_beats;
                    double spanDuration = duration.Duration / (repeats + 1);
                    Vector2 farEnd = startPosition + path.Path.PositionAt(1);

                    yield return withCombo(create(original.StartTime, nodeSamples(original, 0), target(startPosition, hasPosition, random)), original);

                    if (duration.Duration < shortLength)
                        yield break;

                    if (spanDuration >= shortLength)
                    {
                        for (int turn = 1; turn <= repeats; turn++)
                            yield return create(original.StartTime + spanDuration * turn, nodeSamples(original, turn), target(turn % 2 == 0 ? startPosition : farEnd, hasPosition, random));
                    }

                    yield return create(duration.EndTime, nodeSamples(original, repeats + 1), target(repeats % 2 == 0 ? farEnd : startPosition, hasPosition, random));
                }
                else if (original is IHasColumn)
                {
                    // osu!mania hold notes are long but not spinners: just a note where they start.
                    yield return withCombo(create(original.StartTime, original.Samples, target(startPosition, hasPosition, random)), original);
                }
                else
                {
                    // Spinner (and taiko swells, catch banana showers): hold Space for its whole length.
                    yield return withCombo(new KeyboardSpinner
                    {
                        StartTime = original.StartTime,
                        Duration = duration.Duration,
                        Samples = original.Samples,
                    }, original);
                }

                yield break;
            }

            yield return withCombo(create(original.StartTime, original.Samples, target(startPosition, hasPosition, random)), original);
        }

        /// <summary>Copies where combos start (and the mapper's colour offset), so combo colours change like in the original map.</summary>
        private static KeyboardHitObject withCombo(KeyboardHitObject note, HitObject original)
        {
            if (original is IHasCombo combo)
            {
                note.NewCombo = combo.NewCombo;
                note.ComboOffset = combo.ComboOffset;
            }

            return note;
        }

        protected override Beatmap<KeyboardHitObject> ConvertBeatmap(IBeatmap original, CancellationToken cancellationToken)
        {
            var beatmap = base.ConvertBeatmap(original, cancellationToken);

            // Tell every note when the key it uses is free again, so it never appears while the previous note is still there.
            foreach (var notesOnKey in beatmap.HitObjects.GroupBy(h => h.Key))
            {
                double previousEnd = double.NegativeInfinity;

                foreach (var note in notesOnKey.OrderBy(h => h.StartTime))
                {
                    note.PreviousOnSameKey = previousEnd;
                    previousEnd = note.GetEndTime();
                }
            }

            return beatmap;
        }

        /// <summary>Converts an osu! position into key units. Objects without a position get a deterministic random one.</summary>
        private Vector2 target(Vector2 position, bool hasPosition, Random random)
        {
            Vector2 fraction = hasPosition
                ? new Vector2(position.X / osu_width, position.Y / osu_height)
                : new Vector2(random.NextSingle(), random.NextSingle());

            return new Vector2(areaLeft + fraction.X * areaWidth, areaTop + fraction.Y * areaHeight);
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
            var byDistance = noteKeys.OrderBy(k => Vector2.DistanceSquared(k.Centre, target)).ToList();

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

            double gap = sameKeyGap ?? min_same_key_gap;
            return set.GetViewBetween(time - gap, time + gap).Count == 0;
        }

        private static IList<HitSampleInfo> nodeSamples(HitObject original, int node)
        {
            if (original is IHasRepeats repeats && node < repeats.NodeSamples.Count)
                return repeats.NodeSamples[node];

            return original.Samples;
        }
    }
}
