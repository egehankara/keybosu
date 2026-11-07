// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Keyboard.Configuration;
using osu.Game.Rulesets.Keyboard.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Keyboard.UI
{
    [Cached]
    public partial class KeyboardPlayfield : Playfield
    {
        /// <summary>
        /// The layout chosen in settings. It is fixed for the whole play: changing the setting applies to the next map.
        /// Hit objects read this to know where their key is.
        /// </summary>
        public KeyboardLayout Layout { get; private set; } = KeyboardLayout.Default;

        private readonly Container judgementLayer = new Container { RelativeSizeAxes = Axes.Both };
        private readonly DrawablePool<KeyboardJudgement> judgementPool = new DrawablePool<KeyboardJudgement>(20);

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
            // with the keyboard fully on screen. Inside it, the keyboard box is shifted so that point lands in the middle.
            // Everything else is positioned in fractions of the keyboard box, so it scales to any window.
            AddRangeInternal(new Drawable[]
            {
                judgementPool,
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
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
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    // Enter and other shapes that are only drawn, then the real keys.
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
                                }
                            },
                            HitObjectContainer,
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
            });
        }

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!judgedObject.DisplayResult || !DisplayJudgements.Value || result.HitObject is not KeyboardHitObject note || !Layout.Contains(note.Key))
                return;

            var key = Layout[note.Key];

            judgementLayer.Add(judgementPool.Get(j =>
            {
                j.Apply(result, judgedObject);
                j.PlaceOn(Layout, key);
            }));
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
    }
}
