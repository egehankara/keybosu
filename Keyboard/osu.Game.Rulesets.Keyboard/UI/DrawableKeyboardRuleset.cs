// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Keyboard.Objects.Drawables;
using osu.Game.Rulesets.Keyboard.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Keyboard.UI
{
    [Cached]
    public partial class DrawableKeyboardRuleset : DrawableRuleset<KeyboardHitObject>
    {
        public DrawableKeyboardRuleset(KeyboardRuleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod>? mods = null)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override Playfield CreatePlayfield() => new KeyboardPlayfield();

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new KeyboardFramedReplayInputHandler(replay);

        public override DrawableHitObject<KeyboardHitObject> CreateDrawableRepresentation(KeyboardHitObject h) => new DrawableKeyboardHitObject(h);

        protected override PassThroughInputManager CreateInputManager() => new KeyboardInputManager(Ruleset.RulesetInfo);
    }
}
