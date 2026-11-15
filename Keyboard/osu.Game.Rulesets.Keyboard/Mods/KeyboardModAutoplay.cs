// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Keyboard.Replays;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Keyboard.Mods
{
    public class KeyboardModAutoplay : ModAutoplay
    {
        public override ModReplayData CreateReplayData(IBeatmap beatmap, IReadOnlyList<Mod> mods) => CreateAutoplayReplay(beatmap);

        /// <summary>The replay autoplay plays: every key pressed exactly on time, every spinner held to the end.</summary>
        public static ModReplayData CreateAutoplayReplay(IBeatmap beatmap)
            => new ModReplayData(new KeyboardAutoGenerator(beatmap).Generate(), new ModCreatedUser { Username = "Autoplay" });
    }

    // Autoplay with the interface hidden, for watching (or recording) a map.
    public class KeyboardModCinema : ModCinema<KeyboardHitObject>
    {
        public override ModReplayData CreateReplayData(IBeatmap beatmap, IReadOnlyList<Mod> mods) => KeyboardModAutoplay.CreateAutoplayReplay(beatmap);
    }
}
