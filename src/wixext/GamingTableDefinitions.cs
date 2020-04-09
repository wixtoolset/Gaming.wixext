// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Gaming
{
    using WixToolset.Data.WindowsInstaller;

    public static class GamingTableDefinitions
    {
        public static readonly TableDefinition WixGameExplorer = new TableDefinition(
            "WixGameExplorer",
            new[]
            {
                new ColumnDefinition("InstanceId", ColumnType.String, 38, primaryKey: true, nullable: false, ColumnCategory.Guid, description: "The instance ID of the game executable to be registered in Game Explorer."),
                new ColumnDefinition("File_", ColumnType.String, 72, primaryKey: false, nullable: false, ColumnCategory.Identifier, keyTable: "File", keyColumn: 1, description: "The file representing the game executable.", modularizeType: ColumnModularizeType.Column),
            },
            tupleDefinitionName: GamingTupleDefinitions.WixGameExplorer.Name,
            tupleIdIsPrimaryKey: false
        );

        public static readonly TableDefinition[] All = new[]
        {
            WixGameExplorer,
        };
    }
}
