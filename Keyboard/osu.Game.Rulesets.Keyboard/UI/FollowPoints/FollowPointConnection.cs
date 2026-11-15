// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Keyboard.UI.FollowPoints
{
    /// <summary>
    /// The follow points between two notes: osu!standard's FollowPointConnection, with the same spacing, timing and
    /// animation. Positions are in the playfield's key units, where a key (100) is about one hit circle (128).
    /// </summary>
    public partial class FollowPointConnection : CompositeDrawable
    {
        public const int SPACING = 32;
        public const double PREEMPT = 800;

        /// <summary>osu!standard's shortest approach time (approach rate 10), below which follow points appear later too.</summary>
        public const double PREEMPT_MIN = 450;

        /// <summary>How long a point takes to fade in, and to fade out. A constant in osu!stable and osu!standard.</summary>
        public const double FADE_DURATION = 400;

        /// <summary>
        /// Notes closer than this (in key units) get no follow points: their keys touch, side by side or diagonally
        /// (at most 125 apart), so there is nothing to lead across. The nearest keys that do not touch are 160 apart.
        /// </summary>
        public const float MIN_DISTANCE = KeyboardPlayfield.KEY_SIZE * 1.5f;

        public KeyboardHitObject Start { get; }
        public KeyboardHitObject End { get; }

        /// <summary>The points, from the first note towards the second.</summary>
        public IReadOnlyList<FollowPoint> Points => points;

        private readonly List<FollowPoint> points = new List<FollowPoint>();

        private readonly Vector2 startPosition;
        private readonly Vector2 endPosition;

        public FollowPointConnection(KeyboardHitObject start, KeyboardHitObject end, Vector2 startPosition, Vector2 endPosition)
        {
            Start = start;
            End = end;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
        }

        // Transforms given to a drawable before it has loaded are finished at once instead of played, so the points are
        // made and animated here, once this connection is loaded and has the gameplay clock.
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Vector2 distanceVector = endPosition - startPosition;
            int distance = (int)distanceVector.Length;
            float rotation = (float)(Math.Atan2(distanceVector.Y, distanceVector.X) * (180 / Math.PI));

            foreach (int d in PointDistances(distance))
            {
                float fraction = (float)d / distance;
                Vector2 pointStartPosition = startPosition + (fraction - 0.1f) * distanceVector;
                Vector2 pointEndPosition = startPosition + fraction * distanceVector;

                GetFadeTimes(Start, End, fraction, out double fadeInTime, out double fadeOutTime);

                var fp = new FollowPoint
                {
                    Position = pointStartPosition,
                    Rotation = rotation,
                    Alpha = 0,
                    Scale = new Vector2(1.5f),
                };

                fp.AnimationStartTime.Value = fadeInTime;

                points.Add(fp);
                AddInternal(fp);

                using (fp.BeginAbsoluteSequence(fadeInTime))
                {
                    fp.FadeIn(FADE_DURATION);
                    fp.ScaleTo(1, FADE_DURATION, Easing.Out);
                    fp.MoveTo(pointEndPosition, FADE_DURATION, Easing.Out);
                    fp.Delay(fadeOutTime - fadeInTime).FadeOut(FADE_DURATION);
                }
            }
        }

        /// <summary>
        /// How far from the first note each point sits, for notes <paramref name="distance"/> apart: every 32, starting one
        /// and a half spacings in and stopping a spacing short of the second note.
        /// </summary>
        public static IEnumerable<int> PointDistances(int distance)
        {
            for (int d = (int)(SPACING * 1.5); d < distance - SPACING; d += SPACING)
                yield return d;
        }

        /// <summary>
        /// When the point <paramref name="fraction"/> of the way from <paramref name="start"/> to <paramref name="end"/>
        /// fades in and out: it starts fading out when its share of the time between the two notes has passed, and fades in
        /// <see cref="PREEMPT"/> before that.
        /// </summary>
        public static void GetFadeTimes(KeyboardHitObject start, KeyboardHitObject end, float fraction, out double fadeInTime, out double fadeOutTime)
        {
            double startTime = start.GetEndTime();
            double duration = end.StartTime - startTime;

            double preempt = PREEMPT * Math.Min(1, start.TimePreempt / PREEMPT_MIN);

            fadeOutTime = startTime + fraction * duration;
            fadeInTime = fadeOutTime - preempt;
        }

        /// <summary>
        /// When the connection has to be on screen: from the first point's fade-in until the last point has faded out.
        /// Null when there are no follow points between the two notes: the second starts a new combo, either is a spinner,
        /// or their keys touch (<see cref="MIN_DISTANCE"/>).
        /// </summary>
        public static (double start, double end)? Lifetime(KeyboardHitObject start, KeyboardHitObject end, Vector2 startPosition, Vector2 endPosition)
        {
            if (end.NewCombo || start is KeyboardSpinner || end is KeyboardSpinner)
                return null;

            if ((endPosition - startPosition).Length < MIN_DISTANCE)
                return null;

            int distance = (int)(endPosition - startPosition).Length;
            var distances = PointDistances(distance).ToList();

            if (distances.Count == 0)
                return null;

            GetFadeTimes(start, end, (float)distances[0] / distance, out double firstFadeIn, out _);
            GetFadeTimes(start, end, (float)distances[^1] / distance, out double lastFadeIn, out double lastFadeOut);

            return (firstFadeIn, Math.Max(lastFadeOut, lastFadeIn + FADE_DURATION) + FADE_DURATION);
        }
    }
}
