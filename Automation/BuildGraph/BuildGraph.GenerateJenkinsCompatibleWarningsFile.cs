using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using EpicGames.Core;
using Gauntlet;
using Microsoft.Extensions.Logging;

namespace AutomationTool.Tasks
{
    /// <summary>
    /// Parameters for a <see cref="GenerateJenkinsCompatibleWarningsFile"/>.
    /// </summary>
    public class GenerateJenkinsCompatibleWarningsFileParameters
    {
        /// <summary>
        /// Path to the log file to parse.
        /// </summary>
        [TaskParameter]
        public string LogFile;

        /// <summary>
        /// Path to the log file to parse.
        /// </summary>
        [TaskParameter]
        public string Category;

        /// <summary>
        /// Path to the file to write.
        /// </summary>
        [TaskParameter]
        public FileReference OutputFile;

        /// <summary>
        /// Optional, whether or not to append to the file rather than overwrite.
        /// </summary>
        [TaskParameter(Optional = true)]
        public bool Append;

        /// <summary>
        /// Tag to be applied to build products of this task.
        /// </summary>
        [TaskParameter(Optional = true, ValidationType = TaskParameterValidationType.TagList)]
        public string Tag;

        /// <summary>
        /// Path to the file that contains all the messages to ignore
        /// </summary>
        [TaskParameter(Optional = true)]
        public FileReference WarningMessagesToIgnoreFile;
    }

    /// <summary>
    /// Writes text to a file.
    /// </summary>
    [TaskElement("GenerateJenkinsCompatibleWarningsFile", typeof(GenerateJenkinsCompatibleWarningsFileParameters))]
    public class GenerateJenkinsCompatibleWarningsFile : CustomTask
    {
        /// <summary>
        /// Parameters for this task.
        /// </summary>
        GenerateJenkinsCompatibleWarningsFileParameters Parameters;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="InParameters">Parameters for this task.</param>
        public GenerateJenkinsCompatibleWarningsFile(GenerateJenkinsCompatibleWarningsFileParameters InParameters)
        {
            Parameters = InParameters;
        }

        /// <summary>
        /// Execute the task.
        /// </summary>
        /// <param name="Job">Information about the current job.</param>
        /// <param name="BuildProducts">Set of build products produced by this node.</param>
        /// <param name="TagNameToFileSet">Mapping from tag names to the set of files they include.</param>
        public override void Execute(JobContext Job, HashSet<FileReference> BuildProducts, Dictionary<string, HashSet<FileReference>> TagNameToFileSet)
        {
            IEnumerable<string>  WarningMessagesToIgnore = Enumerable.Empty<string>();

            if (FileReference.Exists(Parameters.WarningMessagesToIgnoreFile))
            {
                WarningMessagesToIgnore = File.ReadAllLines(Parameters.WarningMessagesToIgnoreFile.FullName).AsEnumerable<String>();
            }

            string LogFileContent = File.ReadAllText(Parameters.LogFile);

            UnrealLogParser LogParser = new UnrealLogParser(LogFileContent);
            List<UnrealLog.LogEntry> LogEntries = new List<UnrealLog.LogEntry>();

            foreach ( UnrealLog.LogEntry LogEntry in LogParser.LogEntries)
            {
                if (LogEntry.Level >= UnrealLog.LogLevel.Warning)
                {
                    if (WarningMessagesToIgnore.Any(message_to_ignore =>
                    {
                        return Regex.IsMatch(LogEntry.Message, message_to_ignore);
                    }))
                    {
                        continue;
                    }

                    LogEntries.Add(LogEntry);
                }
            }

            string FileText = string.Join(Environment.NewLine, LogEntries.Select(entry =>
            {
                return $"{Parameters.Category} - {entry}";
            }));

            if (string.IsNullOrEmpty(FileText))
            {
                return;
            }

            // Make sure output folder exists.
            if (!DirectoryReference.Exists(Parameters.OutputFile.Directory))
            {
                DirectoryReference.CreateDirectory(Parameters.OutputFile.Directory);
            }

            if (Parameters.Append)
            {
                Logger.LogInformation("Appending text to file '{0}': {1}", Parameters.OutputFile, FileText);
                FileReference.AppendAllText(Parameters.OutputFile, Environment.NewLine + FileText);
            }
            else
            {
                Logger.LogInformation("Writing text to file '{0}': {1}", Parameters.OutputFile, FileText);
                FileReference.WriteAllText(Parameters.OutputFile, FileText);
            }
        }

        /// <summary>
        /// Output this task out to an XML writer.
        /// </summary>
        public override void Write(XmlWriter Writer)
        {
            Write(Writer, Parameters);
        }

        /// <summary>
        /// Find all the tags which are used as inputs to this task
        /// </summary>
        /// <returns>The tag names which are read by this task</returns>
        public override IEnumerable<string> FindConsumedTagNames()
        {
            return FindTagNamesFromFilespec(Parameters.LogFile);
        }

        /// <summary>
        /// Find all the tags which are modified by this task
        /// </summary>
        /// <returns>The tag names which are modified by this task</returns>
        public override IEnumerable<string> FindProducedTagNames()
        {
            return FindTagNamesFromList(Parameters.Tag);
        }
    }
}
