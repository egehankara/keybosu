// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Keyboard.Mods
{
    public class KeyboardModHalfTime : ModHalfTime
    {
    }

    // HalfTime's speed with the pitch lowered, like Daycore in osu!.
    public class KeyboardModDaycore : ModDaycore
    {
    }

    public class KeyboardModDoubleTime : ModDoubleTime
    {
    }

    // DoubleTime's speed plus a drum beat on top, like Nightcore in osu!.
    public class KeyboardModNightcore : ModNightcore<KeyboardHitObject>
    {
    }

    // The song speeds up (or slows down) little by little while it plays.
    public class KeyboardModWindUp : ModWindUp
    {
    }

    public class KeyboardModWindDown : ModWindDown
    {
    }
}
