// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Game.Rulesets.Keyboard.Configuration;
using osu.Game.Rulesets.Keyboard.Localisation;
using osu.Game.Rulesets.Keyboard.UI;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    [TestFixture]
    public class TestCentringName
    {
        [Test]
        public void TestNamedAfterTheIndexFingers()
        {
            Assert.That(KeyboardCentring.BetweenGAndH.GetLocalisableDescription().ToString(), Is.EqualTo("Between the index fingers"));
            Assert.That(KeyboardStrings.Translate(nameof(KeyboardStrings.CentreBetweenIndexFingers), "tr"), Is.EqualTo("İşaret parmaklarının arası"));
        }

        [Test]
        public void TestThatPointIsExactlyHalfwayBetweenFAndJOnEveryLayout()
        {
            foreach (var layoutType in Enum.GetValues<KeyboardLayoutType>())
            {
                foreach (var type in Enum.GetValues<KeyboardType>())
                {
                    var layout = KeyboardLayout.Create(layoutType, type);
                    float halfway = (layout[KeyboardAction.F].Centre.X + layout[KeyboardAction.J].Centre.X) / 2;

                    Assert.That(KeyboardPlayfield.CentreX(layout, KeyboardCentring.BetweenGAndH), Is.EqualTo(halfway).Within(0.0001f), $"{layoutType} / {type}");
                }
            }
        }
    }
}
