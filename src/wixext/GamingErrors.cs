// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Gaming
{
    using System.Resources;
    using WixToolset.Data;

    public static class GamingErrors
    {
        public static Message IllegalGameTaskDirectoryIdentifier(SourceLineNumber sourceLineNumbers, string identifier)
        {
            return Message(sourceLineNumbers, Ids.IllegalGameTaskDirectoryIdentifier, "The generated identifier ({0}) for the directory containing Game Explorer tasks is too long. Shorten the component identifier of your game executable so that the prefix and component ID combined are no more than 72 characters in length.", identifier);
        }

        private static Message Message(SourceLineNumber sourceLineNumber, Ids id, string format, params object[] args)
        {
            return new Message(sourceLineNumber, MessageLevel.Error, (int)id, format, args);
        }

        private static Message Message(SourceLineNumber sourceLineNumber, Ids id, ResourceManager resourceManager, string resourceName, params object[] args)
        {
            return new Message(sourceLineNumber, MessageLevel.Error, (int)id, resourceManager, resourceName, args);
        }

        public enum Ids
        {
            IllegalGameTaskDirectoryIdentifier = 6301,
        }
    }
}
