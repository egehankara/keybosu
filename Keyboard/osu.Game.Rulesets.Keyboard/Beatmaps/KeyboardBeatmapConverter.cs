// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Keyboard.Beatmaps
{
    /// <summary>
    /// Converts any beatmap by mapping each object's playfield position onto the nearest key,
    /// thinning out the fastest notes, filling slower and mostly avoiding the keys at the edges unless <see cref="OriginalDensity"/> is set.
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

        /// <summary>Notes closer together than half a beat are thinned out, as long as that is within these limits (in ms).</summary>
        private const double min_note_gap = 180;

        private const double max_note_gap = 400;

        /// <summary>Slightly under half a beat, so that real half beats are not caught by rounding in their timing.</summary>
        private const double note_gap_beats = 0.475;

        /// <summary>The share of notes that may still use <see cref="edgeKeys"/>.</summary>
        private const double edge_note_chance = 0.25;

        /// <summary>How much more a move to another row counts than a sideways one, when moving away from an edge.</summary>
        private const float off_row_penalty = 9;

        /// <summary>The hardest keys to reach: the first key of every row and the last two, which are mostly punctuation.</summary>
        private readonly HashSet<KeyboardAction> edgeKeys;

        /// <summary>Every note of the map; which ones stay and what key they use is decided once all of them are known.</summary>
        private readonly List<NoteCandidate> candidates = new List<NoteCandidate>();

        private int currentObject;
        private double currentNoteGap;

        /// <summary>Keeps every note, the map's own approach speed and every key, instead of the easier default.</summary>
        public bool OriginalDensity { get; set; }

        public KeyboardBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
            noteKeys = layout.Keys.Where(k => k.Playable && k.Action != KeyboardAction.Space).ToArray();

            areaLeft = noteKeys.Min(k => k.X);
            areaTop = noteKeys.Min(k => k.Y);
            areaWidth = noteKeys.Max(k => k.X + k.Width) - areaLeft;
            areaHeight = noteKeys.Max(k => k.Y + 1) - areaTop;

            edgeKeys = noteKeys.GroupBy(k => k.Y)
                               .SelectMany(row =>
                               {
                                   var keys = row.OrderBy(k => k.X).ToList();
                                   return keys.Take(1).Concat(keys.TakeLast(2));
                               })
                               .Select(k => k.Action)
                               .ToHashSet();
        }

        public override bool CanConvert() => true;

        protected override Beatmap<KeyboardHitObject> CreateBeatmap() => new KeyboardBeatmap();

        protected override IEnumerable<KeyboardHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            // A note starts filling TimePreempt before it is due (same formula as KeyboardHitObject).
            sameKeyGap ??= Math.Max(min_same_key_gap,
                KeyboardHitObject.ApproachTime(beatmap.Difficulty.ApproachRate, OriginalDensity) + KeyboardHitObject.SAME_KEY_CLEARANCE);

            currentObject++;
            currentNoteGap = Math.Clamp(beatmap.ControlPointInfo.TimingPointAt(original.StartTime).BeatLength * note_gap_beats, min_note_gap, max_note_gap);

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

                    yield return withCombo(create(original.StartTime, nodeSamples(original, 0), target(startPosition, hasPosition, original.StartTime)), original);

                    if (duration.Duration < shortLength)
                        yield break;

                    if (spanDuration >= shortLength)
                    {
                        for (int turn = 1; turn <= repeats; turn++)
                            yield return create(original.StartTime + spanDuration * turn, nodeSamples(original, turn), target(turn % 2 == 0 ? startPosition : farEnd, hasPosition, original.StartTime + spanDuration * turn));
                    }

                    yield return create(duration.EndTime, nodeSamples(original, repeats + 1), target(repeats % 2 == 0 ? farEnd : startPosition, hasPosition, duration.EndTime));
                }
                else if (isManiaHold(original))
                {
                    // osu!mania hold notes are long but not spinners: just a note where they start.
                    yield return withCombo(create(original.StartTime, original.Samples, target(startPosition, hasPosition, original.StartTime)), original);
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

            yield return withCombo(create(original.StartTime, original.Samples, target(startPosition, hasPosition, original.StartTime)), original);
        }

        /// <summary>
        /// Whether a long object is an osu!mania hold note. Maps read from a .osu file are not mania objects yet: their holds
        /// only say so in the object type from the file.
        /// </summary>
        private static bool isManiaHold(HitObject original)
            => original is IHasColumn
               || (original is IHasLegacyHitObjectType legacy && (legacy.LegacyType & LegacyHitObjectType.Hold) != 0);

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

            var removed = OriginalDensity ? new HashSet<KeyboardHitObject>() : thinOut();

            foreach (var candidate in candidates.Where(c => !removed.Contains(c.Note)))
                candidate.Note.Key = pickKey(candidate.Target, candidate.Note.StartTime);

            beatmap.HitObjects = beatmap.HitObjects.Where(h => !removed.Contains(h)).ToList();

            foreach (var note in beatmap.HitObjects)
                note.OriginalApproach |= OriginalDensity;

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

        /// <summary>
        /// Picks the notes that are too close to the one before them. Starts of objects are looked at first, so that the end of a
        /// slider never costs the next object its note. Without its first note, the rest of a slider goes too.
        /// </summary>
        private HashSet<KeyboardHitObject> thinOut()
        {
            var removed = new HashSet<KeyboardHitObject>();
            var removedObjects = new HashSet<int>();
            var keptTimes = new SortedSet<double>();

            double lastTime = double.NegativeInfinity;
            bool carriedNewCombo = false;
            int carriedComboOffset = 0;

            foreach (var first in candidates.Where(c => c.IsFirstOfObject).OrderBy(c => c.Note.StartTime))
            {
                if (first.Note.StartTime - lastTime < first.MinGap)
                {
                    // Colours change where a combo starts, which must not be lost along with this note.
                    carriedNewCombo |= first.Note.NewCombo;
                    carriedComboOffset += first.Note.ComboOffset;

                    removed.Add(first.Note);
                    removedObjects.Add(first.ObjectIndex);
                    continue;
                }

                first.Note.NewCombo |= carriedNewCombo;
                first.Note.ComboOffset += carriedComboOffset;
                carriedNewCombo = false;
                carriedComboOffset = 0;

                lastTime = first.Note.StartTime;
                keptTimes.Add(lastTime);
            }

            foreach (var later in candidates.Where(c => !c.IsFirstOfObject).OrderBy(c => c.Note.StartTime))
            {
                double time = later.Note.StartTime;

                if (removedObjects.Contains(later.ObjectIndex) || keptTimes.GetViewBetween(time - later.MinGap + 0.001, time + later.MinGap - 0.001).Count > 0)
                    removed.Add(later.Note);
                else
                    keptTimes.Add(time);
            }

            return removed;
        }

        /// <summary>Converts an osu! position into key units. Objects without a position get a deterministic random one.</summary>
        private Vector2 target(Vector2 position, bool hasPosition, double time)
        {
            Vector2 fraction = hasPosition
                ? new Vector2(position.X / osu_width, position.Y / osu_height)
                : new Vector2((float)RandomFor(time, 1), (float)RandomFor(time, 2));

            return new Vector2(areaLeft + fraction.X * areaWidth, areaTop + fraction.Y * areaHeight);
        }

        private KeyboardHitObject create(double time, IList<HitSampleInfo> samples, Vector2 target)
        {
            var note = new KeyboardHitObject
            {
                StartTime = time,
                Samples = samples,
            };

            candidates.Add(new NoteCandidate(note, target, currentObject, currentNoteGap, candidates.LastOrDefault()?.ObjectIndex != currentObject));
            return note;
        }

        private KeyboardAction pickKey(Vector2 target, double time)
        {
            var byDistance = noteKeys.OrderBy(k => Vector2.DistanceSquared(k.Centre, target)).ToList();

            // Most notes stay away from the edges. The same note gets the same answer on every play.
            bool edgesAllowed = OriginalDensity || RandomFor(time, 0) < edge_note_chance;

            // Moving away from an edge stays on the same row as much as possible, so it is a short sideways move.
            var preferred = edgesAllowed
                ? byDistance
                : noteKeys.Where(k => !edgeKeys.Contains(k.Action))
                          .OrderBy(k => MathF.Pow(k.Centre.X - target.X, 2) + MathF.Pow(k.Centre.Y - target.Y, 2) * off_row_penalty)
                          .ToList();

            KeyDefinition? firstFree(IEnumerable<KeyDefinition> keys) => keys.Cast<KeyDefinition?>().FirstOrDefault(k => isFree(k!.Value.Action, time));

            // Nearest key that has not been used around this time; if every key is busy, just take the nearest.
            var chosen = firstFree(preferred) ?? firstFree(byDistance) ?? byDistance[0];

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

        /// <summary>
        /// A number from 0 to 1 that looks random but is always the same for the same time (and <paramref name="stream"/>),
        /// so a map converts the same way every time. new Random(time) is not used: for times close together its first
        /// numbers are close together too, which turned "random" choices into a repeating pattern.
        /// </summary>
        public static double RandomFor(double time, int stream)
        {
            // SplitMix64 over the time's bits: nearby inputs give unrelated outputs.
            ulong x = (ulong)BitConverter.DoubleToInt64Bits(time) + (ulong)stream * 0x9E3779B97F4A7C15UL + 0x9E3779B97F4A7C15UL;
            x = (x ^ (x >> 30)) * 0xBF58476D1CE4E5B9UL;
            x = (x ^ (x >> 27)) * 0x94D049BB133111EBUL;
            x ^= x >> 31;

            return (x >> 11) * (1.0 / (1UL << 53));
        }

        private static IList<HitSampleInfo> nodeSamples(HitObject original, int node)
        {
            if (original is IHasRepeats repeats && node < repeats.NodeSamples.Count)
                return repeats.NodeSamples[node];

            return original.Samples;
        }

        private sealed record NoteCandidate(KeyboardHitObject Note, Vector2 Target, int ObjectIndex, double MinGap, bool IsFirstOfObject);
    }
}
