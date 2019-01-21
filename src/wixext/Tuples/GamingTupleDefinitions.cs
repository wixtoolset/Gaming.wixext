// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Gaming
{
    using System;
    using WixToolset.Data;

    public enum GamingTupleDefinitionType
    {
        WixGameExplorer,
    }

    public static partial class GamingTupleDefinitions
    {
        public static readonly Version Version = new Version("4.0.0");

        public static IntermediateTupleDefinition ByName(string name)
        {
            if (!Enum.TryParse(name, out GamingTupleDefinitionType type))
            {
                return null;
            }

            return ByType(type);
        }

        public static IntermediateTupleDefinition ByType(GamingTupleDefinitionType type)
        {
            switch (type)
            {
                case GamingTupleDefinitionType.WixGameExplorer:
                    return GamingTupleDefinitions.WixGameExplorer;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}
