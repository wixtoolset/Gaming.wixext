// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Gaming
{
    using System.Linq;
    using System.Xml;
    using WixToolset.Data.WindowsInstaller;
    using WixToolset.Extensibility;

    public class GamingWindowsInstallerBackendBinderExtension : BaseWindowsInstallerBackendBinderExtension
    {
        private static readonly TableDefinition[] Tables = LoadTables();

        protected override TableDefinition[] TableDefinitionsForTuples => Tables;

        private static TableDefinition[] LoadTables()
        {
            using (var resourceStream = typeof(GamingWindowsInstallerBackendBinderExtension).Assembly.GetManifestResourceStream("WixToolset.Gaming.tables.xml"))
            using (var reader = XmlReader.Create(resourceStream))
            {
                var tables = TableDefinitionCollection.Load(reader);
                return tables.ToArray();
            }
        }
    }
}
