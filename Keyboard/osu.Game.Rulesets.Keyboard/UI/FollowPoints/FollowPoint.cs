// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Keyboard.UI.FollowPoints
{
    /// <summary>What a follow point looks like, which depends on the skin exactly as in osu!standard.</summary>
    public enum FollowPointStyle
    {
        /// <summary>The skin's own followpoint image or animation (classic / legacy skins).</summary>
        Legacy,

        /// <summary>Argon's pink double chevron.</summary>
        Argon,

        /// <summary>The small glowing dot of the Triangles skin.</summary>
        Default,
    }

    /// <summary>
    /// A single follow point between two notes: osu!standard's FollowPoint, with the same three looks. osu!standard asks the
    /// skin through its own skin lookups, which another ruleset cannot use, so the same choice is made here by hand.
    /// It follows skin changes during play.
    /// </summary>
    public partial class FollowPoint : SkinReloadableDrawable, IAnimationTimeReference
    {
        private const float width = 8;

        /// <summary>The largest a skin's image is drawn, as in osu!standard: two hit circle radii wide, one high.</summary>
        public static readonly Vector2 LEGACY_MAX_SIZE = new Vector2(128, 64);

        public FollowPointStyle Style { get; private set; }

        /// <summary>When a skin's animation starts playing: when this point starts to fade in, as in osu!standard.</summary>
        public Bindable<double> AnimationStartTime { get; } = new BindableDouble();

        public FollowPoint()
        {
            // No size of its own: whatever the skin draws is centred on this point.
            Origin = Anchor.Centre;
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            base.SkinChanged(skin);

            // The same lookup as osu!standard's legacy skins.
            var legacy = skin.GetAnimation("followpoint", true, true, true, startAtCurrentTime: false, maxSize: LEGACY_MAX_SIZE);

            if (legacy != null)
            {
                Style = FollowPointStyle.Legacy;
                legacy.Anchor = Anchor.Centre;
                legacy.Origin = Anchor.Centre;
                InternalChild = legacy;
                return;
            }

            // The skin in use is the first one asked. Argon Pro is an Argon skin too.
            if (skin.AllSources.FirstOrDefault(s => s is ArgonSkin || s is TrianglesSkin) is ArgonSkin)
            {
                Style = FollowPointStyle.Argon;
                InternalChild = createArgon();
                return;
            }

            Style = FollowPointStyle.Default;
            InternalChild = createDefault();
        }

        // osu.Game.Rulesets.Osu.Skinning.Argon.ArgonFollowPoint
        private static Drawable createArgon() => new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Blending = BlendingParameters.Additive,
            Colour = ColourInfo.GradientVertical(Colour4.FromHex("FC618F"), Colour4.FromHex("BB1A41")),
            AutoSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                new SpriteIcon
                {
                    Icon = FontAwesome.Solid.ChevronRight,
                    Size = new Vector2(8),
                    Colour = OsuColour.Gray(0.2f),
                },
                new SpriteIcon
                {
                    Icon = FontAwesome.Solid.ChevronRight,
                    Size = new Vector2(8),
                    X = 4,
                },
            },
        };

        // The fallback in osu.Game.Rulesets.Osu.Objects.Drawables.Connections.FollowPoint
        private static Drawable createDefault() => new CircularContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Masking = true,
            AutoSizeAxes = Axes.Both,
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = Color4.White.Opacity(0.2f),
                Radius = 4,
            },
            Child = new Box
            {
                Size = new Vector2(width),
                Blending = BlendingParameters.Additive,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Alpha = 0.5f,
            },
        };
    }
}
