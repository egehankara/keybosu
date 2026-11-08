// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Keyboard.Configuration;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Keyboard.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Keyboard.UI
{
    [Cached]
    public partial class KeyboardPlayfield : Playfield
    {
        /// <summary>
        /// Size of one key in the keyboard's own coordinate space. The whole keyboard is laid out in a fixed-size space
        /// (like osu!'s 512x384 playfield) and scaled to the screen, so anything that moves things by pixels
        /// (for example lazer's fail animation, which drops notes by 400) behaves sensibly.
        /// </summary>
        public const float KEY_SIZE = 100;

        /// <summary>Empty space kept on the left and on the right of the keyboard, as a fraction of the screen width.</summary>
        public const float SIDE_MARGIN = 0.06f;

        /// <summary>
        /// The layout chosen in settings. It is fixed for the whole play: changing the setting applies to the next map.
        /// Hit objects read this to know where their key is.
        /// </summary>
        public KeyboardLayout Layout { get; private set; } = KeyboardLayout.Default;

        private readonly Container judgementLayer = new Container { RelativeSizeAxes = Axes.Both };

        private KeyLabel? spaceLabel;

        // One pool per result: a judgement decides which skin graphic to show when it is first loaded,
        // so it has to be created for the result it will display (a single shared pool showed "Perfect" for everything).
        private readonly JudgementPooler<KeyboardJudgement> judgementPooler = new JudgementPooler<KeyboardJudgement>(new[]
        {
            HitResult.Great, HitResult.Ok, HitResult.Meh, HitResult.Miss,
        });

        [BackgroundDependencyLoader]
        private void load(KeyboardRulesetConfigManager config)
        {
            Layout = KeyboardLayout.Create(
                config.Get<KeyboardLayoutType>(KeyboardRulesetSetting.Layout),
                config.Get<KeyboardType>(KeyboardRulesetSetting.KeyboardType));

            bool highlightHomeRow = config.Get<bool>(KeyboardRulesetSetting.HighlightHomeRow);

            if (config.Get<bool>(KeyboardRulesetSetting.ShowJudgements))
                NewResult += onNewResult;

            var frame = ComputeFrame(Layout, config.Get<KeyboardCentring>(KeyboardRulesetSetting.Centring));

            HitObjectContainer.RelativeSizeAxes = Axes.Both;

            // The outer box keeps an aspect ratio wide enough that the chosen centre point can sit in its middle
            // with the keyboard fully on screen. Inside it, the keyboard box is shifted so that point lands in the middle,
            // and the fixed-size keyboard space is scaled to fill that box.
            AddRangeInternal(new Drawable[]
            {
                judgementPooler,
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    // Narrower than the screen so the keyboard never touches its edges.
                    Width = 1 - 2 * SIDE_MARGIN,
                    FillMode = FillMode.Fit,
                    FillAspectRatio = frame.AspectRatio,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        RelativePositionAxes = Axes.X,
                        Width = frame.KeyboardWidth,
                        X = frame.KeyboardX,
                        Child = new KeyboardSpace(Layout)
                        {
                            Children = new Drawable[]
                            {
                                // Shapes that are only drawn (the ISO home-row key), then the real keys.
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ChildrenEnumerable = Layout.Decorations.Select(d => new KeyBackground(d, Layout)),
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ChildrenEnumerable = Layout.Keys.Select(k => new KeyBackground(k, Layout, highlightHomeRow)),
                                },
                                HitObjectContainer,
                                // Letters above the notes, so a filling square never hides which key it is on.
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children = Layout.Decorations.Select(d => (Drawable)new KeyLabel(d, Layout))
                                                     .Concat(Layout.Keys.Select(k => createLabel(k, highlightHomeRow)))
                                                     .ToArray(),
                                },
                                // Above the notes so a judgement is never hidden by the next filling square.
                                judgementLayer,
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ChildrenEnumerable = Layout.Keys.Select(k => new KeyPressHighlight(k, Layout)),
                                },
                            }
                        }
                    }
                }
            });
        }

        private KeyLabel createLabel(KeyDefinition key, bool highlightHomeRow)
        {
            var label = new KeyLabel(key, Layout, highlightHomeRow);

            if (key.Action == KeyboardAction.Space)
                spaceLabel = label;

            return label;
        }

        protected override void Update()
        {
            base.Update();

            // While a spinner sits on the Space bar it shows its own "Hold Space!" text; the bar's own label would
            // show through and make both unreadable, so hide it until the spinner is over.
            bool spinnerOnScreen = HitObjectContainer.AliveObjects.OfType<DrawableKeyboardSpinner>().Any(s => !s.Judged);
            spaceLabel?.SetLabelVisible(!spinnerOnScreen);
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!judgedObject.DisplayResult || !DisplayJudgements.Value || result.HitObject is not KeyboardHitObject note || !Layout.Contains(note.Key))
                return;

            var key = Layout[note.Key];

            var judgement = judgementPooler.Get(result.Type, j =>
            {
                j.Apply(result, judgedObject);
                j.PlaceOn(Layout, key);
            });

            if (judgement != null)
                judgementLayer.Add(judgement);
        }

        /// <summary>The point of the keyboard, in key units from its left edge, that should be in the middle of the screen.</summary>
        public static float CentreX(KeyboardLayout layout, KeyboardCentring centring)
        {
            switch (centring)
            {
                case KeyboardCentring.SpaceBar:
                    return layout[KeyboardAction.Space].Centre.X;

                case KeyboardCentring.BetweenGAndH:
                    // The left edge of H is the right edge of G.
                    return layout[KeyboardAction.H].X;

                default:
                    return layout.Width / 2;
            }
        }

        /// <summary>
        /// How to frame the keyboard so <see cref="CentreX"/> is in the middle.
        /// <see cref="Frame.KeyboardWidth"/> and <see cref="Frame.KeyboardX"/> are fractions of the outer box.
        /// </summary>
        public static Frame ComputeFrame(KeyboardLayout layout, KeyboardCentring centring)
        {
            float centre = CentreX(layout, centring);

            // Room needed on each side of the centre point so nothing goes off screen.
            float half = Math.Max(centre, layout.Width - centre);
            float frameWidth = half * 2;

            return new Frame(frameWidth / layout.Height, layout.Width / frameWidth, (half - centre) / frameWidth);
        }

        public readonly record struct Frame(float AspectRatio, float KeyboardWidth, float KeyboardX);

        /// <summary>
        /// The keyboard's fixed-size coordinate space (<see cref="KEY_SIZE"/> per key), scaled to fill its parent.
        /// </summary>
        private partial class KeyboardSpace : Container
        {
            public KeyboardSpace(KeyboardLayout layout)
            {
                Size = new Vector2(layout.Width * KEY_SIZE, layout.Height * KEY_SIZE);
            }

            protected override void Update()
            {
                base.Update();

                // The parent box has the same aspect ratio, so one uniform scale fits it exactly.
                if (Parent != null && Parent.DrawWidth > 0)
                    Scale = new Vector2(Parent.DrawWidth / DrawWidth);
            }
        }
    }
}
