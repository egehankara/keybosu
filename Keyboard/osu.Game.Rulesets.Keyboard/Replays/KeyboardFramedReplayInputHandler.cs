// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input.StateChanges;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Keyboard.Replays
{
    public class KeyboardFramedReplayInputHandler : FramedReplayInputHandler<KeyboardReplayFrame>
    {
        public KeyboardFramedReplayInputHandler(Replay replay)
            : base(replay)
        {
        }

        protected override bool IsImportant(KeyboardReplayFrame frame) => true;

        protected override void CollectReplayInputs(List<IInput> inputs)
        {
            inputs.Add(new ReplayState<KeyboardAction>
            {
                PressedActions = CurrentFrame?.Actions ?? new List<KeyboardAction>(),
            });
        }
    }
}
