// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Keyboard.Scoring
{
    public partial class KeyboardScoreProcessor : ScoreProcessor
    {
        public KeyboardScoreProcessor(Ruleset ruleset)
            : base(ruleset)
        {
        }

        // By default lazer counts Perfect the same as Great (300). Like osu!mania, make Perfect worth a little more
        // so better timing is actually rewarded (it also makes a Great slightly less than 100% accuracy).
        public override int GetBaseScoreForResult(HitResult result)
        {
            if (result == HitResult.Perfect)
                return 305;

            return base.GetBaseScoreForResult(result);
        }
    }
}
