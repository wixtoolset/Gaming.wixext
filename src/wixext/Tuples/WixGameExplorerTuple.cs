// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Gaming
{
    using WixToolset.Data;
    using WixToolset.Gaming.Tuples;

    public static partial class GamingTupleDefinitions
    {
        public static readonly IntermediateTupleDefinition WixGameExplorer = new IntermediateTupleDefinition(
            GamingTupleDefinitionType.WixGameExplorer.ToString(),
            new[]
            {
                new IntermediateFieldDefinition(nameof(WixGameExplorerTupleFields.InstanceId), IntermediateFieldType.String),
                new IntermediateFieldDefinition(nameof(WixGameExplorerTupleFields.FileRef), IntermediateFieldType.String),
            },
            typeof(WixGameExplorerTuple));
    }
}

namespace WixToolset.Gaming.Tuples
{
    using WixToolset.Data;

    public enum WixGameExplorerTupleFields
    {
        InstanceId,
        FileRef,
    }

    public class WixGameExplorerTuple : IntermediateTuple
    {
        public WixGameExplorerTuple() : base(GamingTupleDefinitions.WixGameExplorer, null, null)
        {
        }

        public WixGameExplorerTuple(SourceLineNumber sourceLineNumber, Identifier id = null) : base(GamingTupleDefinitions.WixGameExplorer, sourceLineNumber, id)
        {
        }

        public IntermediateField this[WixGameExplorerTupleFields index] => this.Fields[(int)index];

        public string InstanceId
        {
            get => this.Fields[(int)WixGameExplorerTupleFields.InstanceId].AsString();
            set => this.Set((int)WixGameExplorerTupleFields.InstanceId, value);
        }

        public string FileRef
        {
            get => this.Fields[(int)WixGameExplorerTupleFields.FileRef].AsString();
            set => this.Set((int)WixGameExplorerTupleFields.FileRef, value);
        }
    }
}