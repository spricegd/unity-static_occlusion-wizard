using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SOCWizard.Data;

namespace SOCWizard.UI
{
    public class SOCWizard : EditorWindow
    {
        private UnityEngine.Object _occlusionPortalTemplate;
        private UnityEngine.Object _occlusionAreaTemplate;
        private GameObject _target;
        private Vector2 _scrollViewPosition;
        readonly SOCWizardLogic logic = new();
        readonly SOCWizardData data = new();

        [MenuItem("External Tools/SOC Wizard")]
        private static void GetSocWindow()
        {
            var window = (SOCWizard)GetWindow(typeof(SOCWizard));
            window.minSize = new Vector2(420, 780);
            window.Show();
        }

        private void OnGUI()
        {
            ToolsDisplayController();
        }

        private void ToolsDisplayController()
        {
            _scrollViewPosition = EditorGUILayout.BeginScrollView(_scrollViewPosition);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(15);
            DisplayOcclusionTools();
            GUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            OcclusionSceneTools();
            GUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            OcclusionBakeProfiler();
            GUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            DisplayContextInformation();
            GUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            DisplayProjectOcclusionInformation();
            GUILayout.Space(5);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            DisplayVisualizationOptions();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        #region GUI Methods
        //TODO: Fix scaling issue with right column in bake tools and Vis options
        private void DisplayOcclusionTools()
        {
            data._showBakeTools = EditorGUILayout.BeginFoldoutHeaderGroup(data._showBakeTools, "Bake Parameters & Tools");

            if(data._showBakeTools)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();

                data._bakeEditorBuildList = EditorGUILayout.ToggleLeft("Bake Editor Build Scene List", data._bakeEditorBuildList);

                StaticOcclusionCulling.backfaceThreshold = EditorGUILayout.FloatField("Backface Threshold", StaticOcclusionCulling.backfaceThreshold, GUILayout.Width(200));
                StaticOcclusionCulling.smallestHole = EditorGUILayout.FloatField("Smallest Hole", StaticOcclusionCulling.smallestHole, GUILayout.Width(200));
                StaticOcclusionCulling.smallestOccluder = EditorGUILayout.FloatField("Smallest Occluder", StaticOcclusionCulling.smallestOccluder, GUILayout.Width(200));
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                if (GUILayout.Button("Generate", GUILayout.Width(160)))
                {
                    if (data._bakeEditorBuildList)
                    {
                        UnityEngine.Debug.Log("SOC Baking all scenes in editor build scene list");
                        foreach (var scene in EditorBuildSettings.scenes)
                        {
                            UnityEngine.Debug.Log($"Opening {scene.path}");
                            EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
                            UnityEngine.Debug.Log($"Baking scene{scene.path}");
                            logic.BakeStaticOcclusion();
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"SOC Baking current scene only");
                        logic.BakeStaticOcclusion();
                    }
                }

                if (GUILayout.Button("Generate In Background", GUILayout.Width(160)))
                {
                    if (data._bakeEditorBuildList)
                    {
                        UnityEngine.Debug.Log("SOC Baking all scene in editor build scene list");
                        foreach (var scene in EditorBuildSettings.scenes)
                        {
                            UnityEngine.Debug.Log($"Opening {scene.path}");
                            EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
                            UnityEngine.Debug.Log($"Baking scene{scene.path}");
                            logic.BackgroundBakeStaticOcclusion();
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"SOC Baking current scene only");
                        logic.BackgroundBakeStaticOcclusion();
                    }
                }

                if(GUILayout.Button("Default Parameters", GUILayout.Width(160)))
                {
                    logic.ResetBakeParametersToDefault();
                }

                if (GUILayout.Button("Cancel Bake", GUILayout.Width(160)))
                {
                    StaticOcclusionCulling.Cancel();
                    AssetDatabase.SaveAssets();
                    UnityEngine.Debug.Log("SOC bake operation cancelled!");
                }

                if (GUILayout.Button("Remove Cache Data", GUILayout.Width(160)))
                {
                    StaticOcclusionCulling.RemoveCacheFolder();
                    AssetDatabase.SaveAssets();
                    UnityEngine.Debug.Log("SOC cache data has been deleted!");
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void OcclusionBakeProfiler()
        {
            data._showTest = EditorGUILayout.BeginFoldoutHeaderGroup(data._showTest, "Static Occlusion Profiler");

            if (data._showTest)
            {
                if (GUILayout.Button("Run Static Occlusion Test", GUILayout.Width(200)))
                {
                    logic.RunProfileTest();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        //TODO: Find a way to get the lib folder path
        private void DisplayContextInformation()
        {
            long umbraSize = StaticOcclusionCulling.umbraDataSize;
            var umbraSizeKb = umbraSize / 1024;
            var umbraSizeMb = umbraSize / (1024*1024);

            data._showUmbraInfo = EditorGUILayout.BeginFoldoutHeaderGroup(data._showUmbraInfo, "Umbra Cache Utils");

            if(data._showUmbraInfo)
            {
                EditorGUILayout.BeginHorizontal();
                if (StaticOcclusionCulling.isRunning)
                {
                    EditorGUILayout.HelpBox("SOC bake is running", MessageType.Info);
                }

                if (StaticOcclusionCulling.doesSceneHaveManualPortals)
                {
                    EditorGUILayout.HelpBox("Current scene has manual occlusion portals", MessageType.Info);
                }

                EditorGUILayout.BeginVertical();
                GUILayout.Label($"Umbra data size in Bytes: {umbraSize}");
                GUILayout.Label($"Umbra data size in Kilobytes: {umbraSizeKb}");
                GUILayout.Label($"Umbra data size in Megabytes: {umbraSizeMb}");
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                if (Directory.Exists(@"Library/Occlusion"))
                {
                    if (GUILayout.Button("Open Umbra Folder", GUILayout.Width(200)))
                    {
                        UnityEngine.Debug.Log("Opening Umbra data folder");
                        Process.Start("explorer.exe");
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No Umbra cache present", MessageType.Info);
                }

                if (GUILayout.Button("Write Umbra Log to Console", GUILayout.Width(200)))
                {
                    logic.WriteUmbraLogToConsole();
                }

                if (GUILayout.Button("Clear Current Umbra Data", GUILayout.Width(200)))
                {
                    StaticOcclusionCulling.Clear();
                    AssetDatabase.SaveAssets();
                    UnityEngine.Debug.Log("SOC bake data cleared");
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void OcclusionSceneTools()
        {
            data._showSceneTool = EditorGUILayout.BeginFoldoutHeaderGroup(data._showSceneTool, "Custom Occlusion Scene Tools");

            if(data._showSceneTool)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                data._spawnOnAsset = EditorGUILayout.ToggleLeft("Spawn Asset at Target", data._spawnOnAsset);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                data._isPortalOpen = EditorGUILayout.ToggleLeft("Portal Creation State", data._isPortalOpen);
                EditorGUILayout.EndHorizontal();

                _occlusionPortalTemplate = EditorGUILayout.ObjectField("Occlusion Portal Template", _occlusionPortalTemplate, typeof(GameObject), true, GUILayout.Width(350));
                _occlusionAreaTemplate = EditorGUILayout.ObjectField("Occlusion Area Template", _occlusionAreaTemplate, typeof(GameObject), true, GUILayout.Width(350));

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Create SOC Portal",GUILayout.Width(150)))
                {
                    logic.GenerateOcclusionPortal();
                }

                if (GUILayout.Button("Create SOC Area", GUILayout.Width(150)))
                {
                    logic.GenerateOcclusionArea();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        //TODO: Write paths to file and output path location
        private void DisplayProjectOcclusionInformation()
        {
            var ocDataFiles = AssetDatabase.FindAssets("OcclusionCullingData");

            var totalOcDataFileCount = ocDataFiles.Length;

            data._showSocInfo = EditorGUILayout.BeginFoldoutHeaderGroup(data._showSocInfo, "Static Occlusion Information");

            if(data._showSocInfo)
            {
                GUILayout.Label($"# of occlusion data files in project: {totalOcDataFileCount}");
                GUILayout.Label($"Size of project occlusion data: {totalOcDataFileCount}");

                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Occlusion Paths to Console",GUILayout.Width(180)))
                {
                    logic.GetProjectOcclusionData();
                }

                if (GUILayout.Button("Occlusion Paths to File", GUILayout.Width(180)))
                {
                    logic.WriteOcclusionDataToFile();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Occlusion File to Console", GUILayout.Width(180)))
                {
                    logic.OcclusionFileDataToConsole();
                }

                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Project Occlusion Size", GUILayout.Width(180)))
                {
                    logic.CalculateOverallOcclusionDataSize();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DisplayVisualizationOptions()
        {
            data._showVisOptions = EditorGUILayout.BeginFoldoutHeaderGroup(data._showVisOptions, "Visualization Options");

            if(data._showVisOptions)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                StaticOcclusionCullingVisualization.showDynamicObjectBounds = EditorGUILayout.ToggleLeft("Show Dynamic Object Bounds", StaticOcclusionCullingVisualization.showDynamicObjectBounds);
                StaticOcclusionCullingVisualization.showGeometryCulling = EditorGUILayout.ToggleLeft("Show Geometry Culling", StaticOcclusionCullingVisualization.showGeometryCulling);
                StaticOcclusionCullingVisualization.showOcclusionCulling = EditorGUILayout.ToggleLeft("Show Occlusion Culling", StaticOcclusionCullingVisualization.showOcclusionCulling);
                StaticOcclusionCullingVisualization.showPortals = EditorGUILayout.ToggleLeft("Show Portals", StaticOcclusionCullingVisualization.showPortals);
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                StaticOcclusionCullingVisualization.showPreVisualization = EditorGUILayout.ToggleLeft("Show Pre-Visualization", StaticOcclusionCullingVisualization.showPreVisualization);
                StaticOcclusionCullingVisualization.showViewVolumes = EditorGUILayout.ToggleLeft("Show View Volumes", StaticOcclusionCullingVisualization.showViewVolumes);
                StaticOcclusionCullingVisualization.showVisibilityLines = EditorGUILayout.ToggleLeft("Show Visibility Lines", StaticOcclusionCullingVisualization.showVisibilityLines);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        #endregion
    }
}