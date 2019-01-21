// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Gaming
{
    using System;
    using System.Collections.Generic;
    using WixToolset.Extensibility;

    public class GamingExtensionFactory : BaseExtensionFactory
    {
        protected override IEnumerable<Type> ExtensionTypes => new[]
        {
            typeof(GamingCompiler),
            typeof(GamingExtensionData),
            typeof(GamingWindowsInstallerBackendBinderExtension),
        };
    }
}
