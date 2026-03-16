using EpicGames.Core;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Extensions.Logging;
using UnrealBuildTool;
using UnrealBuildBase;

namespace AutomationTool.Tasks
{
    /// <summary>
    /// Parameters for a Git task
    /// </summary>
    public class StaticAnalysisTaskParameters
    {
        /// <summary>
        /// The target to use
        /// </summary>
        [TaskParameter]
        public string Target;

        /// <summary>
        /// The platform to use
        /// </summary>
        [TaskParameter]
        public string Platform;

        /// <summary>
        /// The configuration to use
        /// </summary>
        [TaskParameter]
        public string Configuration;

        /// <summary>
        /// The path of the uproject file
        /// </summary>
        [TaskParameter]
        public string ProjectPath;

        /// <summary>
        /// The analyzer to use
        /// </summary>
        [TaskParameter(Optional = true)]
        public string Analyzer = "VisualCpp";

        /// <summary>
        /// Additional arguments to pass to UBT
        /// </summary>
        [TaskParameter(Optional = true)]
        public string AdditionalArguments;
    }

    /// <summary>
    /// Spawns Git and waits for it to complete.
    /// </summary>
    [TaskElement("StaticAnalysis", typeof(StaticAnalysisTaskParameters))]
    public class StaticAnalysisTask : CustomTask
    {
        /// <summary>
        /// Parameters for this task
        /// </summary>
        StaticAnalysisTaskParameters Parameters;

        /// <summary>
        /// Construct a Git task
        /// </summary>
        /// <param name="InParameters">Parameters for the task</param>
        public StaticAnalysisTask(StaticAnalysisTaskParameters InParameters)
        {
            Parameters = InParameters;
        }

        /// <summary>
        /// Execute the task.
        /// </summary>
        /// <param name="Job">Information about the current job</param>
        /// <param name="BuildProducts">Set of build products produced by this node.</param>
        /// <param name="TagNameToFileSet">Mapping from tag names to the set of files they include</param>
        public override void Execute(JobContext Job, HashSet<FileReference> BuildProducts, Dictionary<string, HashSet<FileReference>> TagNameToFileSet)
        {
            List<string> CleanArguments = new List<string> { 
                Parameters.Target,
                Parameters.Platform,
                Parameters.Configuration,
                "-Project=" + Parameters.ProjectPath,
                "-Clean"
            };

            CommandUtils.RunUBT(CommandUtils.CmdEnv, Unreal.UnrealBuildToolDllPath, CommandLineArguments.Join(CleanArguments));

            string Directory = Path.GetDirectoryName(Parameters.ProjectPath);
            string LogPath = Path.Combine( Directory, $"Saved/Logs/StaticAnalysis_{Parameters.Target}_{Parameters.Platform}_{Parameters.Configuration}.log" );

            List<string> AnalyzerArguments = new List<string> { 
                Parameters.Target,
                Parameters.Platform,
                Parameters.Configuration,
                "-Project=" + Parameters.ProjectPath,
                "-StaticAnalyzer=" + Parameters.Analyzer,
                "-log=" + LogPath
            };

            if ( !string.IsNullOrEmpty(Parameters.AdditionalArguments))
            {
                AnalyzerArguments.Add( Parameters.AdditionalArguments );
            }

            CommandUtils.RunUBT(CommandUtils.CmdEnv, Unreal.UnrealBuildToolDllPath, CommandLineArguments.Join(AnalyzerArguments));
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
            yield break;
        }

        /// <summary>
        /// Find all the tags which are modified by this task
        /// </summary>
        /// <returns>The tag names which are modified by this task</returns>
        public override IEnumerable<string> FindProducedTagNames()
        {
            yield break;
        }
    }
}
