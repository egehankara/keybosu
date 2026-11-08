// Copyright (c) 2026 xrayzack1010. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;

namespace osu.Game.Rulesets.Keyboard.Tests
{
    [TestFixture]
    public class TestRulesetName
    {
        [Test]
        public void TestShownAsKeybosu()
        {
            var ruleset = new KeyboardRuleset();

            Assert.That(ruleset.Description, Is.EqualTo("keybosu!"), "the name in the game, exactly as written");
            Assert.That(ruleset.ShortName, Is.EqualTo("keyboard"), "must not change: settings, key bindings and scores are stored under it");
        }
    }
}
