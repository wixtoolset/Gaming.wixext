// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.Gaming
{
    using System.Linq;
    using WixBuildTools.TestSupport;
    using WixToolset.Core.TestPackage;
    using WixToolset.Gaming;
    using Xunit;

    public class GamingExtensionFixture
    {
        [Fact]
        public void CanBuildUsingGame()
        {
            var folder = TestData.Get(@"TestData\UsingGame");
            var build = new Builder(folder, typeof(GamingExtensionFactory), new[] { folder });

            var results = build.BuildAndQuery(Build, "WixGameExplorer");
            Assert.Equal(new[]
            {
                "WixGameExplorer:{12345678-1234-5678-90AB-1234567890AB}\tfilF5_pLhBuF5b4N9XEo52g_hUM5Lo",
            }, results.OrderBy(s => s).ToArray());
        }

        private static void Build(string[] args)
        {
            var result = WixRunner.Execute(args)
                                  .AssertSuccess();
        }
    }
}
