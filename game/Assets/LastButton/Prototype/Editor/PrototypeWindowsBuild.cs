using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastButton.Prototype.Editor
{
    public static class PrototypeWindowsBuild
    {
        private const string SceneDirectory = "Assets/LastButton/Prototype/Scenes";
        private const string ScenePath = SceneDirectory + "/Prototype.unity";

        public static void Run()
        {
            try
            {
                CreateEmptyBootstrapScene();
                string executablePath = GetExecutablePath();
                Directory.CreateDirectory(Path.GetDirectoryName(executablePath));

                PlayerSettings.companyName = "LAST BUTTON Team";
                PlayerSettings.productName = "LAST BUTTON Prototype";
                PlayerSettings.bundleVersion = "0.0.1";

                BuildPlayerOptions options = new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = executablePath,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development
                };

                BuildReport report = BuildPipeline.BuildPlayer(options);
                if (report.summary.result != BuildResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Windows build failed: {report.summary.result}, errors={report.summary.totalErrors}");
                }

                Debug.Log($"LAST_BUTTON_BUILD_PASSED path={executablePath} size={report.summary.totalSize}");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError("LAST_BUTTON_BUILD_FAILED");
                EditorApplication.Exit(1);
            }
        }

        private static void CreateEmptyBootstrapScene()
        {
            Directory.CreateDirectory(SceneDirectory);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException("Could not save the prototype scene.");
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            AssetDatabase.SaveAssets();
        }

        private static string GetExecutablePath()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string workspaceRoot = Directory.GetParent(projectRoot).FullName;
            return Path.Combine(workspaceRoot, "outputs", "LAST_BUTTON_Prototype", "LAST_BUTTON_Prototype.exe");
        }
    }
}
