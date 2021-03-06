﻿using System.Linq;
using ActionStreetMap.Core.MapCss;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Core.MapCss
{
    [TestFixture]
    public class StylesheetTests
    {
        [Test]
        public void CanCreate()
        {
            // ARRANGE
            var provider = new StylesheetProvider(TestHelper.TestBaseMapcssFile, TestHelper.GetFileSystemService());

            // ACT
            var stylesheet = provider.Get();

            // ASSERT
            Assert.IsNotNull(stylesheet);
        }

        [Test]
        public void CanParse()
        {
            // ARRANGE
            var provider = new StylesheetProvider(TestHelper.TestBaseMapcssFile, TestHelper.GetFileSystemService());

            // ACT & ASSERT
            var stylesheet = provider.Get();

            Assert.AreEqual(9, stylesheet.Count);

            var testStyle1 = MapCssHelper.GetStyles(stylesheet)[1];

            Assert.AreEqual(2, testStyle1.Selectors.Count);
            Assert.AreEqual(7, testStyle1.Declarations.Count);

            var testSelector1 = testStyle1.Selectors.First();
            Assert.AreEqual("man_made", testSelector1.Tag);
            Assert.AreEqual("tower", testSelector1.Value);
            Assert.AreEqual("=", testSelector1.Operation);

            var testSelector2 = testStyle1.Selectors.Last();

            Assert.AreEqual("building", testSelector2.Tag);
            Assert.AreEqual("OP_EXIST", testSelector2.Operation);

            var lastStyle = MapCssHelper.GetStyles(stylesheet)[7];
            Assert.AreEqual(2, lastStyle.Selectors.Count);
            Assert.AreEqual(1, lastStyle.Declarations.Count);
        }
    }
}