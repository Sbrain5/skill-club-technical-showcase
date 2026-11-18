using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

/// <summary>
/// builds the windows game from the command line for the ci pipeline
/// </summary>
public static class CIBuild
{
    #region methods
    /// <summary>
    /// builds the windows 64-bit game
    /// </summary>
    public static void BuildWin64()
    {
        string buildFolder = ReadCommandLineArgument("-buildPath"); // reads the output folder passed by the azure pipeline

        if (string.IsNullOrWhiteSpace(buildFolder))
        {
            throw new Exception("Missing -buildPath argument");
        }

        Directory.CreateDirectory(buildFolder); // makes sure the build output folder exists
        string[] scenePaths = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(); // includes only scenes that are enabled in the unity build settings

        if (scenePaths.Length == 0)
        {
            throw new Exception("No enabled scenes in Build Settings.");
        }

        // sets the scenes, output file and windows build target
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenePaths,
            locationPathName = Path.Combine(buildFolder, "SkillClub.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions); // starts the unity build and returns its final result

        // throws an error so the pipeline is marked as failed
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception("Unity build failed: " + report.summary.result);
        }
    }

    /// <summary>
    /// reads one command line argument
    /// </summary>
    /// <param name="argumentName">argument name</param>
    /// <returns>argument value or null</returns>
    private static string ReadCommandLineArgument(string argumentName)
    {
        string[] arguments = Environment.GetCommandLineArgs();

        // stops before the final item because each argument needs a value after it
        for (int i = 0; i < arguments.Length - 1; i++)
        {
            if (arguments[i] == argumentName)
            {
                return arguments[i + 1];
            }
        }

        return null;
    }
    #endregion
}