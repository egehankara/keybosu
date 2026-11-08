// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    /// <summary>Loads the ruleset icon and checks its parts: a thick white ring and a solid rounded key inside it.</summary>
    public partial class TestSceneIcon : OsuTestScene
    {
        private Drawable icon = null!;

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
            AddAssert("a thick ring like osu!'s icons", () =>
                icon.ChildrenOfType<CircularContainer>().Single(c => c.BorderThickness > 0).BorderThickness == KeyboardRuleset.Icon.SIZE * KeyboardRuleset.Icon.RING_THICKNESS);
            AddAssert("a solid rounded key inside the ring", () =>
                icon.ChildrenOfType<Container>().Any(c => c is not CircularContainer && c.CornerRadius > 0 && c.DrawWidth < icon.DrawWidth
                                                          && c.ChildrenOfType<Box>().Any(b => b.Alpha == 1)));
            AddAssert("no letter", () => !icon.ChildrenOfType<SpriteText>().Any());
        }
    }
}
