# osu!keyboard

A custom [osu!lazer](https://github.com/ppy/osu) ruleset where the whole keyboard is the playfield.

Every key (A–Z plus the punctuation keys next to the letters) is a square on screen. A coloured square
grows inside a key; press that key the moment it fills the cell.

## Features

- Converts any existing osu! beatmap: each note's position is mapped to the nearest key.
- On-screen keyboard scaled to any window size. Two settings under Settings > Ruleset > Keyboard:
  - **Keyboard layout**: English (QWERTY), Turkish Q or Turkish F (what is printed on the keys).
  - **Keyboard type**: ANSI (flat Enter, `\` on the top row) or ISO (tall Enter, extra key left of Z).
  Both are saved and apply the next time a map starts.
- Mods: Autoplay, NoFail, HalfTime, DoubleTime.
- The 33 letter and punctuation keys are identical in every layout and keyboard type, so beatmap conversion
  never depends on what you picked.

## Notes

- The extra keys (ANSI backslash, ISO key left of Z) and Enter are drawn dimmed and never get notes.
  The ISO home-row key osu!framework cannot read at all, so it is only drawn.
- Input is read by physical key position, so the OS keyboard language does not matter.
- **Never renumber `KeyboardAction`.** lazer stores key bindings by number; changing one makes saved bindings
  point at the wrong keys, and two actions on one key makes lazer wipe that key. `TestActionNumbering` guards this.

## Tests

```sh
dotnet test Keyboard/osu.Game.Rulesets.Keyboard.Tests -c Release
```

Runs headless. It presses every key through the real input pipeline, loads real plays on each keyboard type,
and checks the layout geometry.

## Not done yet

- Replay recording (autoplay works).
- A real star-rating model (currently note density only).

Custom rulesets never award PP.

## Install

Requires the .NET SDK and osu!lazer.

```sh
./install.sh
```

This builds the ruleset and copies `osu.Game.Rulesets.Keyboard.dll` to `~/.local/share/osu/rulesets`.
Restart osu!lazer afterwards. Set `OSU_DATA` to use a different data directory.

The ruleset must be built against a `ppy.osu.Game` package whose ruleset API version matches your installed
lazer (checked: `2026.818.0`). After a lazer update, rebuild with `./install.sh`.

## Layout

- `Keyboard/osu.Game.Rulesets.Keyboard` – the ruleset
- `Keyboard/osu.Game.Rulesets.Keyboard.Tests` – visual test project from the official template

Generated from the [osu! ruleset template](https://github.com/ppy/osu-templates); template files are under the MIT licence.
