#!/bin/sh
# Builds the ruleset and copies it into osu!lazer's user ruleset folder.
set -e
cd "$(dirname "$0")/Keyboard/osu.Game.Rulesets.Keyboard"
dotnet build -c Release -v quiet -nologo
DEST="${OSU_DATA:-$HOME/.local/share/osu}/rulesets"
mkdir -p "$DEST"
# Older releases used another file name, and osu! would load both.
rm -f "$DEST/osu.Game.Rulesets.Keyboard.dll"
# osu! renames a ruleset that crashed to .broken and then says it is disabled.
rm -f "$DEST/osu.Game.Rulesets.Keybosu.dll.broken"
# Never write over the file in place: a running osu! has it mapped into memory, and changing it underneath corrupts the
# code it is running (it crashes and marks the ruleset as broken). Copy next to it, then swap it in with a rename:
# the running game keeps the old file, the next start gets the new one.
cp bin/Release/net10.0/osu.Game.Rulesets.Keybosu.dll "$DEST/.keybosu-new"
mv -f "$DEST/.keybosu-new" "$DEST/osu.Game.Rulesets.Keybosu.dll"
echo "Installed to $DEST - restart osu!lazer."
