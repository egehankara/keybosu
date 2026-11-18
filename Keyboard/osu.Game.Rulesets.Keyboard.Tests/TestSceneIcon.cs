// Copyright (c) 2026 Egehan Kara. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    /// <summary>Loads the ruleset icon and checks its parts: a thick white square frame with soft corners, a solid key in the middle with the same corner shape.</summary>
    public partial class TestSceneIcon : OsuTestScene
    {
        private Drawable icon = null!;

        private IEnumerable<Container> outlines => icon.ChildrenOfType<Container>().Where(c => c.BorderThickness > 0).OrderByDescending(c => c.DrawWidth);

        private Container frame => outlines.First();

        [Test]
        public void TestIcon()
        {
            AddStep("show icon", () => Child = icon = new KeyboardRuleset().CreateIcon().With(d =>
            {
                d.Anchor = Anchor.Centre;
                d.Origin = Anchor.Centre;
                d.Scale = new Vector2(2);
            }));
            AddUntilStep("loaded", () => icon.IsLoaded);
            AddAssert("a thick frame like osu!'s icons", () =>
                frame.BorderThickness == KeyboardRuleset.Icon.SIZE * KeyboardRuleset.Icon.FRAME_THICKNESS);
            AddAssert("outer corner is the inner corner plus the thickness", () =>
                Precision.AlmostEquals(frame.CornerRadius, KeyboardRuleset.Icon.SIZE * (KeyboardRuleset.Icon.INNER_CORNER_RADIUS + KeyboardRuleset.Icon.FRAME_THICKNESS)));
            AddAssert("only the frame is an outline", () => outlines.Count() == 1);
            AddAssert("a solid key in the middle, its corners in the same share as the frame's", () =>
                icon.ChildrenOfType<Container>().Any(c => c.BorderThickness == 0 && c.ChildrenOfType<Box>().Any(b => b.Alpha == 1)
                                                          && Precision.AlmostEquals(c.CornerRadius / c.DrawWidth, frame.CornerRadius / frame.DrawWidth)));
            AddAssert("no letter", () => !icon.ChildrenOfType<SpriteText>().Any());
        }
    }
}
