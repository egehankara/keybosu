// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
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
using osu.Game.Screens.Play;

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

        // After a pause, count down 3-2-1 before play continues - the very same overlay osu!mania and osu!taiko use.
        protected override ResumeOverlay CreateResumeOverlay() => new DelayedResumeOverlay();

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new KeyboardFramedReplayInputHandler(replay);

        public override DrawableHitObject<KeyboardHitObject> CreateDrawableRepresentation(KeyboardHitObject h)
        {

            if (h is KeyboardSpinner spinner)
                return new DrawableKeyboardSpinner(spinner);

            return new DrawableKeyboardHitObject(h);
        }

        protected override PassThroughInputManager CreateInputManager() => new KeyboardInputManager(Ruleset.RulesetInfo);
    }
}
