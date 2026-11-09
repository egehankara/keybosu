#!/bin/sh
# Builds the ruleset and copies it into osu!lazer's user ruleset folder.
set -e
cd "$(dirname "$0")/Keyboard/osu.Game.Rulesets.Keyboard"
dotnet build -c Release -v quiet -nologo
DEST="${OSU_DATA:-$HOME/.local/share/osu}/rulesets"
mkdir -p "$DEST"
# Older releases used another file name, and osu! would load both.
rm -f "$DEST/osu.Game.Rulesets.Keyboard.dll"
cp bin/Release/net10.0/osu.Game.Rulesets.Keybosu.dll "$DEST/"
echo "Installed to $DEST - restart osu!lazer."
