// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Keyboard.Replays
{
    public class KeyboardReplayFrame : ReplayFrame
    {
        /// <summary>Every key held down at <see cref="ReplayFrame.Time"/>.</summary>
        public List<KeyboardAction> Actions = new List<KeyboardAction>();

        public KeyboardReplayFrame(IEnumerable<KeyboardAction>? actions = null)
        {
            if (actions != null)
                Actions.AddRange(actions);
        }

        public override bool IsEquivalentTo(ReplayFrame other)
            => other is KeyboardReplayFrame frame && Time == frame.Time && Actions.SequenceEqual(frame.Actions);
    }
}
