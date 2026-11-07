# osu!keyboard

A custom [osu!lazer](https://github.com/ppy/osu) ruleset where the whole keyboard is the playfield.

Every key (A–Z plus the punctuation keys next to the letters) is a square on screen. A coloured square
grows inside a key; press that key the moment it fills the cell.

## Features

- Converts any existing osu! beatmap: each note's position is mapped to the nearest key.
- On-screen US QWERTY layout (33 keys), scaled to any window size.
- Mods: Autoplay, NoFail, HalfTime, DoubleTime.
- Keyboard layout is plain data (`KeyboardLayout.cs`), so more layouts can be added later.

## Not done yet

- Layout selection in settings.
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
