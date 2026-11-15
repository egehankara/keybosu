// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Keyboard.Objects;
using osuTK;

namespace osu.Game.Rulesets.Keyboard.UI.FollowPoints
{
    /// <summary>
    /// Draws follow points from every note to the next, in a layer of its own under the notes: osu!standard's
    /// FollowPointRenderer. Only the connections due on screen exist at any moment, also when play jumps in time.
    /// </summary>
    public partial class FollowPointRenderer : CompositeDrawable
    {
        private readonly List<Entry> entries = new List<Entry>();
        private readonly Dictionary<Entry, FollowPointConnection> alive = new Dictionary<Entry, FollowPointConnection>();

        /// <summary>The connections on screen now.</summary>
        public IEnumerable<FollowPointConnection> Connections => alive.Values;

        /// <summary>How many pairs of notes have follow points between them in the whole map.</summary>
        public int ConnectionCount => entries.Count;

        public FollowPointRenderer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        /// <summary>Sets the notes to connect. Each is connected to the one due after it.</summary>
        public void SetHitObjects(IEnumerable<KeyboardHitObject> hitObjects, KeyboardLayout layout)
        {
            foreach (var connection in alive.Values)
                RemoveInternal(connection, true);

            alive.Clear();
            entries.Clear();

            var ordered = hitObjects.Where(h => layout.Contains(h.Key)).OrderBy(h => h.StartTime).ToList();

            for (int i = 0; i < ordered.Count - 1; i++)
            {
                var start = ordered[i];
                var end = ordered[i + 1];

                Vector2 startPosition = layout[start.Key].Centre * KeyboardPlayfield.KEY_SIZE;
                Vector2 endPosition = layout[end.Key].Centre * KeyboardPlayfield.KEY_SIZE;

                if (FollowPointConnection.Lifetime(start, end, startPosition, endPosition) is (double from, double to))
                    entries.Add(new Entry(start, end, startPosition, endPosition, from, to));
            }
        }

        protected override void Update()
        {
            base.Update();

            double now = Time.Current;

            foreach (var entry in entries)
            {
                bool due = now >= entry.From && now < entry.To;
                bool present = alive.TryGetValue(entry, out var connection);

                if (due && !present)
                {
                    connection = new FollowPointConnection(entry.Start, entry.End, entry.StartPosition, entry.EndPosition);
                    alive[entry] = connection;
                    AddInternal(connection);
                }
                else if (!due && present)
                {
                    alive.Remove(entry);
                    RemoveInternal(connection!, true);
                }
            }
        }

        private sealed record Entry(KeyboardHitObject Start, KeyboardHitObject End, Vector2 StartPosition, Vector2 EndPosition, double From, double To);
    }
}
