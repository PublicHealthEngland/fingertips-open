﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Profiles.MainUI.Skins;

namespace MainUISeleniumTest.Phof
{
    [TestClass]
    public class PhofBaseUnitTest : BaseUnitTest
    {
        [TestInitialize]
        public override void CalledOnceAtStartOfTests()
        {
            SetSkin(SkinNames.Phof);
        }
    }
}