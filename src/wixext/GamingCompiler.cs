// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml.Linq;
    using WixToolset.Data;
    using WixToolset.Data.Tuples;
    using WixToolset.Extensibility;

    /// <summary>
    /// The compiler for the WiX Toolset Gaming Extension.
    /// </summary>
    public sealed class GamingCompiler : BaseCompilerExtension
    {
        /// <summary>
        /// All Game Explorer tasks are either play tasks or support tasks. For more information, see http://msdn2.microsoft.com/en-us/library/bb173450(VS.85).aspx.
        /// </summary>
        private enum TaskType
        {
            /// <summary>
            /// Play tasks run the game with optional command-line arguments.
            /// </summary>
            Play = 0,

            /// <summary>
            /// Support tasks are URL shortcuts.
            /// </summary>
            Support
        }

        public override XNamespace Namespace => "http://wixtoolset.org/schemas/v4/wxs/gaming";

        /// <summary>
        /// Processes an attribute for the Compiler.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line number for the parent element.</param>
        /// <param name="parentElement">Parent element of element to process.</param>
        /// <param name="attribute">Attribute to process.</param>
        /// <param name="context">Extra information about the context in which this element is being parsed.</param>
        public override void ParseAttribute(Intermediate intermediate, IntermediateSection section, XElement parentElement, XAttribute attribute, IDictionary<string, string> context)
        {
            SourceLineNumber sourceLineNumbers = this.ParseHelper.GetSourceLineNumbers(parentElement);
            switch (parentElement.Name.LocalName)
            {
                case "Extension":
                    // at the time the IsRichSavedGame extension attribute is parsed, the compiler
                    // might not yet have parsed the Id attribute, so we need to get it directly
                    // from the parent element and put it into the contextValues dictionary.
                    XAttribute idAttribute = parentElement.Attribute("Id");
                    if (null == idAttribute)
                    {
                        this.Messaging.Write(ErrorMessages.ExpectedParentWithAttribute(sourceLineNumbers, "Extension", "IsRichSavedGame", "Id"));
                    }
                    else
                    {
                        context["ExtensionId"] = idAttribute.Value;
                        switch (attribute.Name.LocalName)
                        {
                            case "IsRichSavedGame":
                                if (YesNoType.Yes == this.ParseHelper.GetAttributeYesNoValue(sourceLineNumbers, attribute))
                                {
                                    this.ProcessIsRichSavedGameAttribute(section, sourceLineNumbers, context);
                                }
                                break;
                            default:
                                this.ParseHelper.UnexpectedAttribute(parentElement, attribute);
                                break;
                        }
                    }
                    break;
                default:
                    this.ParseHelper.UnexpectedElement(parentElement, parentElement);
                    break;
            }
        }

        /// <summary>
        /// Processes an element for the Compiler.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line number for the parent element.</param>
        /// <param name="parentElement">Parent element of element to process.</param>
        /// <param name="element">Element to process.</param>
        /// <param name="contextValues">Extra information about the context in which this element is being parsed.</param>
        public override void ParseElement(Intermediate intermediate, IntermediateSection section, XElement parentElement, XElement element, IDictionary<string, string> context)
        {
            switch (parentElement.Name.LocalName)
            {
                case "File":
                    string fileId = context["FileId"];
                    string componentId = context["ComponentId"];
                    string componentDirectoryId = context["DirectoryId"];

                    switch (element.Name.LocalName)
                    {
                        case "Game":
                            this.ParseGameElement(intermediate, section, element, fileId, componentId, componentDirectoryId);
                            break;
                        default:
                            this.ParseHelper.UnexpectedElement(parentElement, element);
                            break;
                    }
                    break;
                default:
                    this.ParseHelper.UnexpectedElement(parentElement, element);
                    break;
            }
        }

        /// <summary>
        /// Processes the Extension/@IsRichSavedGame attribute.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line number for the parent element.</param>
        /// <param name="contextValues">Extra information about the context in which this element is being parsed.</param>
        private void ProcessIsRichSavedGameAttribute(IntermediateSection section, SourceLineNumber sourceLineNumbers, IDictionary<string, string> context)
        {
            const RegistryRootType MsidbRegistryRootClassesRoot = RegistryRootType.ClassesRoot;
            const RegistryRootType MsidbRegistryRootLocalMachine = RegistryRootType.LocalMachine;

            string progId = context["ProgId"];
            string componentId = context["ComponentId"];
            string extensionId = context["ExtensionId"];
            
            if (null == extensionId || null == progId || null == componentId)
            {
                this.Messaging.Write(ErrorMessages.ExpectedParentWithAttribute(sourceLineNumbers, "Extension", "IsRichSavedGame", "ProgId"));
            }

            if (!this.Messaging.EncounteredError)
            {
                // write Registry rows according to http://msdn2.microsoft.com/en-us/library/bb173448.aspx
                this.ParseHelper.CreateRegistryRow(section, sourceLineNumbers, MsidbRegistryRootClassesRoot, progId, "PreviewTitle", "prop:System.Game.RichSaveName;System.Game.RichApplicationName", componentId, false);
                this.ParseHelper.CreateRegistryRow(section, sourceLineNumbers, MsidbRegistryRootClassesRoot, progId, "PreviewDetails", "prop:System.Game.RichLevel;System.DateChanged;System.Game.RichComment;System.DisplayName;System.DisplayType", componentId, false);
                this.ParseHelper.CreateRegistryRow(section, sourceLineNumbers, MsidbRegistryRootClassesRoot, String.Concat(".", extensionId, "\\{BB2E617C-0920-11D1-9A0B-00C04FC2D6C1}"), null, "{4E5BFBF8-F59A-4E87-9805-1F9B42CC254A}", componentId, false);
                this.ParseHelper.CreateRegistryRow(section, sourceLineNumbers, MsidbRegistryRootLocalMachine, String.Concat("Software\\Microsoft\\Windows\\CurrentVersion\\PropertySystem\\PropertyHandlers\\.", extensionId), null, "{ECDD6472-2B9B-4B4B-AE36-F316DF3C8D60}", componentId, false);
            }
        }

        /// <summary>
        /// Parses a Game element for Game Explorer registration.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <param name="fileId">The file identifier of the parent element.</param>
        /// <param name="componentId">The component identifier of the game executable.</param>
        private void ParseGameElement(Intermediate intermediate, IntermediateSection section, XElement node, string fileId, string componentId, string componentDirectoryId)
        {
            SourceLineNumber sourceLineNumbers = this.ParseHelper.GetSourceLineNumbers(node);
            string id = null;
            string gdfResourceFileId = fileId;
            string executableFileId = fileId;
            int playTaskOrder = 0;
            int supportTaskOrder = 0;

            foreach (XAttribute attrib in node.Attributes())
            {
                if (String.IsNullOrEmpty(attrib.Name.NamespaceName) || this.Namespace == attrib.Name.Namespace)
                {
                    switch (attrib.Name.LocalName)
                    {
                        case "Id":
                            id = this.ParseHelper.GetAttributeGuidValue(sourceLineNumbers, attrib, false);
                            break;
                        case "GdfResourceFile":
                            gdfResourceFileId = this.ParseHelper.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        case "ExecutableFile":
                            executableFileId = this.ParseHelper.GetAttributeIdentifierValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.ParseHelper.UnexpectedAttribute(node, attrib);
                            break;
                    }
                }
                else
                {
                    this.ParseHelper.ParseExtensionAttribute(this.Context.Extensions, intermediate, section, node, attrib);
                }
            }

            foreach (XElement child in node.Elements())
            {
                if (this.Namespace == child.Name.Namespace)
                {
                    switch (child.Name.LocalName)
                    {
                            case "PlayTask":
                                if (0 == playTaskOrder && 0 == supportTaskOrder)
                                {
                                    this.CreateTaskRootDirectoryCustomActions(section, sourceLineNumbers, id, componentId);
                                }
                                this.ParsePlayTaskElement(intermediate, section, child, id, executableFileId, componentId, playTaskOrder, componentDirectoryId);
                                ++playTaskOrder;
                                break;
                            case "SupportTask":
                                if (0 == playTaskOrder && 0 == supportTaskOrder)
                                {
                                    this.CreateTaskRootDirectoryCustomActions(section, sourceLineNumbers, id, componentId);
                                }
                                this.ParseSupportTaskElement(intermediate, section, child, id, componentId, supportTaskOrder);
                                ++supportTaskOrder;
                                break;
                        default:
                            this.ParseHelper.UnexpectedElement(node, child);
                            break;
                    }
                }
                else
                {
                    this.ParseHelper.ParseExtensionElement(this.Context.Extensions, intermediate, section, node, child);
                }
            }

            if (null == id)
            {
                this.Messaging.Write(ErrorMessages.ExpectedAttribute(sourceLineNumbers, node.Name.LocalName, "Id"));
            }

            if (0 != String.Compare(fileId, gdfResourceFileId, StringComparison.Ordinal))
            {
                this.ParseHelper.CreateSimpleReference(section, sourceLineNumbers, "File", gdfResourceFileId);
            }

            if (0 != String.Compare(fileId, executableFileId, StringComparison.Ordinal))
            {
                this.ParseHelper.CreateSimpleReference(section, sourceLineNumbers, "File", executableFileId);
            }

            if (!this.Messaging.EncounteredError)
            {
                var row = this.ParseHelper.CreateRow(section, sourceLineNumbers, "WixGameExplorer");
                row.Set(0, id);
                row.Set(1, gdfResourceFileId);
                this.ParseHelper.CreateSimpleReference(section, sourceLineNumbers, "CustomAction", "WixSchedGameExplorer");
            }
        }

        /// <summary>
        /// Parses a PlayTask element for Game Explorer task registration.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <param name="gameId">The game's instance identifier.</param>
        /// <param name="fileId">The file identifier of the game executable.</param>
        /// <param name="componentId">The component identifier of the game executable.</param>
        /// <param name="taskOrder">The order this play task should appear in Game Explorer.</param>
        private void ParsePlayTaskElement(Intermediate intermediate, IntermediateSection section, XElement node, string gameId, string fileId, string componentId, int taskOrder, string componentDirectoryId)
        {
            SourceLineNumber sourceLineNumbers = this.ParseHelper.GetSourceLineNumbers(node);
            string name = null;
            string arguments = null;

            foreach (XAttribute attrib in node.Attributes())
            {
                if (String.IsNullOrEmpty(attrib.Name.NamespaceName) || this.Namespace == attrib.Name.Namespace)
                {
                    switch (attrib.Name.LocalName)
                    {
                        case "Name":
                            name = this.ParseHelper.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Arguments":
                            arguments = this.ParseHelper.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.ParseHelper.UnexpectedAttribute(node, attrib);
                            break;
                    }
                }
                else
                {
                    this.ParseHelper.ParseExtensionAttribute(this.Context.Extensions, intermediate, section, node, attrib);
                }
            }

            this.ParseHelper.ParseForExtensionElements(this.Context.Extensions, intermediate, section, node);

            if (null == name)
            {
                this.Messaging.Write(ErrorMessages.ExpectedAttribute(sourceLineNumbers, node.Name.LocalName, "Name"));
            }

            if (!this.Messaging.EncounteredError)
            {
                // create Shortcut rows pointing to the parent File
                string directoryId = this.CreateTaskDirectoryRow(section, sourceLineNumbers, componentId, TaskType.Play, taskOrder);
                var row = this.ParseHelper.CreateRow(section, sourceLineNumbers, "Shortcut");
                row.Set(0, directoryId); // just need something unique-and-stable
                row.Set(1, directoryId);
                row.Set(2, this.ParseHelper.IsValidShortFilename(name, false) ? name : String.Concat(this.ParseHelper.CreateShortName(name, true, false, directoryId, name), "|", name));
                row.Set(3, componentId);
                row.Set(4, String.Format(CultureInfo.InvariantCulture, "[#{0}]", fileId));
                row.Set(5, arguments);
                // skipping Description, Hotkey, Icon_, IconIndex, ShowCmd
                row.Set(11, componentDirectoryId);
            }
        }

        /// <summary>
        /// Parses a SupportTask element for Game Explorer task registration.
        /// </summary>
        /// <param name="node">The element to parse.</param>
        /// <param name="gameId">The game's instance identifier.</param>
        /// <param name="componentId">The component identifier of the game executable.</param>
        /// <param name="taskOrder">The order this support task should appear in Game Explorer.</param>
        private void ParseSupportTaskElement(Intermediate intermediate, IntermediateSection section, XElement node, string gameId, string componentId, int taskOrder)
        {
            SourceLineNumber sourceLineNumbers = this.ParseHelper.GetSourceLineNumbers(node);
            string name = null;
            string address = null;

            foreach (XAttribute attrib in node.Attributes())
            {
                if (String.IsNullOrEmpty(attrib.Name.NamespaceName) || this.Namespace == attrib.Name.Namespace)
                {
                    switch (attrib.Name.LocalName)
                    {
                        case "Name":
                            name = this.ParseHelper.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        case "Address":
                            address = this.ParseHelper.GetAttributeValue(sourceLineNumbers, attrib);
                            break;
                        default:
                            this.ParseHelper.UnexpectedAttribute(node, attrib);
                            break;
                    }
                }
                else
                {
                    this.ParseHelper.ParseExtensionAttribute(this.Context.Extensions, intermediate, section, node, attrib);
                }
            }

            this.ParseHelper.ParseForExtensionElements(this.Context.Extensions, intermediate, section, node);

            if (null == name)
            {
                this.Messaging.Write(ErrorMessages.ExpectedAttribute(sourceLineNumbers, node.Name.LocalName, "Name"));
            }

            if (null == address)
            {
                this.Messaging.Write(ErrorMessages.ExpectedAttribute(sourceLineNumbers, node.Name.LocalName, "Address"));
            }

            if (!this.Messaging.EncounteredError)
            {
                // create support shortcuts as WixUtilExtension's WixInternetShortcut rows;
                // use the directory ID as the shortcut ID because Game Explorer wants one
                // shortcut per directory, so that makes the directory ID unique
                string directoryId = this.CreateTaskDirectoryRow(section, sourceLineNumbers, componentId, TaskType.Support, taskOrder);
#if TODO_CREATE_SHORTCUT
                UtilCompiler.CreateWixInternetShortcut(this.Core, sourceLineNumbers, componentId, directoryId, directoryId, name, address, UtilCompiler.InternetShortcutType.Link, null, 0);
#endif
            }
        }

        /// <summary>
        /// Creates and schedules custom actions to set the root directories for this game's play and support tasks.
        /// </summary>
        /// <param name="sourceLineNumbers">Source line number for the parent element.</param>
        /// <param name="gameId">The game's instance identifier.</param>
        /// <param name="componentId">The component identifier of the game executable.</param>
        private void CreateTaskRootDirectoryCustomActions(IntermediateSection section, SourceLineNumber sourceLineNumbers, string gameId, string componentId)
        {
            string playTasksDirectoryId = this.GetTaskDirectoryId(sourceLineNumbers, "WixPlayTasksRoot", componentId);
            string supportTasksDirectoryId = this.GetTaskDirectoryId(sourceLineNumbers, "WixSupportTasksRoot", componentId);
            string rootDirectoryPath = String.Format(CultureInfo.InvariantCulture, @"[CommonAppDataFolder]Microsoft\Windows\GameExplorer\{0}\", gameId);

            // create placeholder directories for this game's tasks
            var row = this.ParseHelper.CreateRow(section, sourceLineNumbers, "Directory");
            row.Set(0, playTasksDirectoryId);
            row.Set(1, "TARGETDIR");
            row.Set(2, ".");

            row = this.ParseHelper.CreateRow(section, sourceLineNumbers, "Directory");
            row.Set(0, supportTasksDirectoryId);
            row.Set(1, "TARGETDIR");
            row.Set(2, ".");

            // set the PlayTasks path and schedule it
            row = this.ParseHelper.CreateRow(section, sourceLineNumbers, "CustomAction");
            row.Set(0, playTasksDirectoryId);
            row.Set(1, 51);
            row.Set(2, playTasksDirectoryId);
            row.Set(3, String.Concat(rootDirectoryPath, "PlayTasks\\"));

            row = this.ParseHelper.CreateRow(section, sourceLineNumbers, "WixAction");
            row.Set(0, "InstallExecuteSequence");
            row.Set(1, playTasksDirectoryId);
            row.Set(4, "CostFinalize");

            // set the SupportTasks path and schedule it
            row = this.ParseHelper.CreateRow(section, sourceLineNumbers, "CustomAction");
            row.Set(0, supportTasksDirectoryId);
            row.Set(1, 51);
            row.Set(2, supportTasksDirectoryId);
            row.Set(3, String.Concat(rootDirectoryPath, "SupportTasks\\"));

            row = this.ParseHelper.CreateRow(section, sourceLineNumbers, "WixAction");
            row.Set(0, "InstallExecuteSequence");
            row.Set(1, supportTasksDirectoryId);
            row.Set(4, "CostFinalize");
        }

        /// <summary>
        /// Creates a new Directory table row for each task, first creating the tree of directories for the task subdirectories.
        /// </summary>
        /// <remarks>
        /// The directory hierarchy is defined at http://msdn2.microsoft.com/en-us/library/bb173450(VS.85).aspx and looks like this in the Directory table:
        /// <code>
        ///  WixPlayTasksRoot_MyGameExeComp (PlayTasks, set by custom action)
        ///  -- WixPlayTask0_MyGameExeComp (primary task)
        ///  -- WixPlayTask1_MyGameExeComp (some other play task)
        ///  WixSupportTasksRoot_MyGameExeComp (SupportTasks, set by custom action)
        ///  -- WixSupportTask0_MyGameExeComp (first support task)
        ///  -- WixSupportTask1_MyGameExeComp (second support task)
        /// </code>
        /// The primary purpose of this method is to verify early that the generated identifier won't fail ICE03 validation.
        /// </remarks>
        /// <param name="sourceLineNumbers">Source line number for the parent element.</param>
        /// <param name="componentId">The component identifier of the game executable.</param>
        /// <param name="taskType">The type of this task (because play tasks and support tasks go into different directories).</param>
        /// <param name="taskOrder">The order this support task should appear in Game Explorer.</param>
        /// <returns>The generated Directory table identifier.</returns>
        private string CreateTaskDirectoryRow(IntermediateSection section, SourceLineNumber sourceLineNumbers, string componentId, TaskType taskType, int taskOrder)
        {
            string parentDirectoryId = this.GetTaskDirectoryId(sourceLineNumbers, TaskType.Play == taskType ? "WixPlayTasksRoot" : "WixSupportTasksRoot", componentId);
            string taskOrderString = taskOrder.ToString(CultureInfo.InvariantCulture.NumberFormat);
            string thisDirectoryName = String.Concat(TaskType.Play == taskType ? "WixPlayTask" : "WixSupportTask", taskOrderString);
            string thisDirectoryId = this.GetTaskDirectoryId(sourceLineNumbers, thisDirectoryName, componentId);
            
            // create the numbered directory where the task shortcut itself will live
            var row = this.ParseHelper.CreateRow(section, sourceLineNumbers, "Directory");
            row.Set(0, thisDirectoryId);
            row.Set(1, parentDirectoryId);
            row.Set(2, taskOrder.ToString(CultureInfo.InvariantCulture.NumberFormat));

            return thisDirectoryId;
        }

        /// <summary>
        /// Creates a stable and unique Directory table identifier by combining a prefix with a component identifier (Id, not GUID).
        /// </summary>
        /// <param name="sourceLineNumbers">Source line number for the parent element.</param>
        /// <param name="prefix">A prefix that "uniquifies" the generated identifier.</param>
        /// <param name="componentId">The owning component's identifier.</param>
        /// <returns>The generated Directory table identifier.</returns>
        private string GetTaskDirectoryId(SourceLineNumber sourceLineNumbers, string prefix, string componentId)
        {
            string id = String.Concat(prefix, "_", componentId);
            
            if (72 < id.Length || !this.ParseHelper.IsValidIdentifier(id))
            {
                this.Messaging.Write(GamingErrors.IllegalGameTaskDirectoryIdentifier(sourceLineNumbers, id));
            }

            return id;
        }
    }
}
