// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings.Sections.Input;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Keyboard.Beatmaps;
using osu.Game.Rulesets.Keyboard.Configuration;
using osu.Game.Rulesets.Keyboard.Mods;
using osu.Game.Rulesets.Keyboard.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Keyboard
{
    public partial class KeyboardRuleset : Ruleset
    {
        // The name shown in the game, exactly as written and never translated. ShortName below stays "keyboard":
        // lazer stores settings, key bindings and scores under it, so changing that would reset them.
        public override string Description => "keybosu!";

        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) =>
            new DrawableKeyboardRuleset(this, beatmap, mods);

        // Works out combo numbers from NewCombo, which is what the combo colours are picked by.
        public override IBeatmapProcessor CreateBeatmapProcessor(IBeatmap beatmap) => new BeatmapProcessor(beatmap);

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) =>
            new KeyboardBeatmapConverter(beatmap, this);

        public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) =>
            new KeyboardDifficultyCalculator(RulesetInfo, beatmap);

        // Grouped and ordered like osu!standard's own mod list.
        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new KeyboardModEasy(),
                        new KeyboardModNoFail(),
                        new KeyboardModHalfTime(),
                        new KeyboardModDaycore(),
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new KeyboardModHardRock(),
                        new KeyboardModSuddenDeath(),
                        new KeyboardModPerfect(),
                        new KeyboardModDoubleTime(),
                        new KeyboardModNightcore(),
                        new KeyboardModHidden(),
                        new KeyboardModOriginalDensity(),
                    };

                case ModType.Conversion:
                    return new Mod[]
                    {
                        new KeyboardModDifficultyAdjust(),
                        new KeyboardModMirror(),
                        new KeyboardModLeftHand(),
                        new KeyboardModRightHand(),
                        new KeyboardModHomeRow(),
                    };

                case ModType.Automation:
                    return new Mod[]
                    {
                        new KeyboardModAutoplay(),
                        new KeyboardModCinema(),
                    };

                case ModType.Fun:
                    return new Mod[]
                    {
                        new KeyboardModWindUp(),
                        new KeyboardModWindDown(),
                        new KeyboardModMuted(),
                    };

                default:
                    return Array.Empty<Mod>();
            }
        }

        public override string ShortName => SHORT_NAME;

        public const string SHORT_NAME = "keyboard";

        /// <summary>Where the "feedback" button in the settings leads.</summary>
        public const string PROJECT_URL = "https://github.com/egehankara/keybosu";

        // Each action is bound to its own physical key, taken from the layout (including keys only some keyboards have).
        // osu! also asks for the editor's bindings; this ruleset has no editor, so there are none.
        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0)
            => variant == EDITOR_VARIANT
                ? Array.Empty<KeyBinding>()
                : KeyboardLayout.AllBindableKeys.Select(k => new KeyBinding(k.Key, k.Action));

        // Everything else would show up on the results screen as an always-empty column. Miss is always included by osu!.
        public override IEnumerable<HitResult> GetValidHitResults() => new[] { HitResult.Great, HitResult.Ok, HitResult.Meh };

        // Keys have a fixed size, so the map's circle size has no effect here.
        public override IEnumerable<RulesetBeatmapAttribute> GetBeatmapAttributesForDisplay(IBeatmapInfo beatmapInfo, IReadOnlyCollection<Mod> mods)
            => base.GetBeatmapAttributesForDisplay(beatmapInfo, mods).Where(a => a.Acronym != "CS");

        public override IRulesetConfigManager CreateConfig(SettingsStore? settings) => new KeyboardRulesetConfigManager(settings, RulesetInfo);

        public override RulesetSettingsSubsection CreateSettings() => new KeyboardSettingsSubsection(this);

        public override Drawable CreateIcon() => new Icon();

        /// <summary>
        /// The ruleset icon, matching osu!'s own ruleset icons: a thick white frame with a white shape inside -
        /// here a square frame with soft corners around a solid key. Drawn at a fixed size; lazer scales it to wherever it is shown.
        /// </summary>
        public partial class Icon : CompositeDrawable
        {
            public const float SIZE = 100;

            /// <summary>Frame thickness as a share of the icon's size, measured from osu!'s own ruleset icons.</summary>
            public const float FRAME_THICKNESS = 0.13f;

            /// <summary>
            /// Corner radius of the inside edge of the frame, as a share of the icon's size.
            /// The outer corner is this plus the frame thickness, so both corners share one centre (concentric corners).
            /// </summary>
            public const float INNER_CORNER_RADIUS = 0.044f;

            /// <summary>Size of the solid key in the middle, as a share of the icon's size.</summary>
            public const float KEY_SIZE = 0.44f;

            /// <summary>
            /// The outer frame's corner radius as a share of its size. The key uses the same share,
            /// so both squares have the same shape at different sizes.
            /// </summary>
            public const float CORNER_SHARE = INNER_CORNER_RADIUS + FRAME_THICKNESS;

            private KeyBindingPanel? panelAwaitingWarning;
            private KeyBindingPanel? panelAwaitingEditorCleanup;

            protected override void LoadComplete()
            {
                base.LoadComplete();
                panelAwaitingWarning = panelAwaitingEditorCleanup = KeyBindingsWarning.FindPanelOf(this);
            }

            protected override void Update()
            {
                base.Update();

                if (panelAwaitingWarning != null && KeyBindingsWarning.TryAddTo(panelAwaitingWarning, SHORT_NAME))
                    panelAwaitingWarning = null;

                if (panelAwaitingEditorCleanup != null && KeyBindingsWarning.TryRemoveEditorHeading(panelAwaitingEditorCleanup, SHORT_NAME))
                    panelAwaitingEditorCleanup = null;
            }

            public Icon()
            {
                Size = new Vector2(SIZE);

                InternalChildren = new Drawable[]
                {
                    // The frame: a rounded square outline. The inside corner follows from the outer one minus the thickness.
                    outline(SIZE, FRAME_THICKNESS),
                    // The key
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(SIZE * KEY_SIZE),
                        Masking = true,
                        CornerRadius = SIZE * KEY_SIZE * CORNER_SHARE,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                        },
                    },
                };
            }

            /// <summary>A white rounded square outline of the given size, with its corners in <see cref="CORNER_SHARE"/>.</summary>
            private static Container outline(float size, float thickness) => new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(size),
                Masking = true,
                CornerRadius = size * CORNER_SHARE,
                BorderThickness = SIZE * thickness,
                BorderColour = Color4.White,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    AlwaysPresent = true,
                },
            };
        }

        // Leave this line intact. It will bake the correct version into the ruleset on each build/release.
        public override string RulesetAPIVersionSupported => CURRENT_RULESET_API_VERSION;
    }
}
