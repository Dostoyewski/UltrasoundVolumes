using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityVolumeRendering
{
    public class VolumeRendererEditorFunctions
    {
        [MenuItem("Volume Rendering/Load raw dataset")]
        private static void ShowDatasetImporter()
        {
            var file = EditorUtility.OpenFilePanel("Select a dataset to load", "DataFiles", "");
            if (File.Exists(file))
                EditorDatasetImporter.ImportDataset(file);
            else
                Debug.LogError("File doesn't exist: " + file);
        }

        [MenuItem("Volume Rendering/Load DICOM")]
        private static void ShowDICOMImporter()
        {
            var dir = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");
            if (Directory.Exists(dir))
            {
                var recursive = true;

                // Read all files
                var fileCandidates = Directory.EnumerateFiles(dir, "*.*",
                        recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) ||
                                p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) ||
                                p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase));

                if (!fileCandidates.Any())
                {
#if UNITY_EDITOR
                    if (EditorUtility.DisplayDialog("Could not find any DICOM files",
                        $"Failed to find any files with DICOM file extension.{Environment.NewLine}Do you want to include files without DICOM file extension?",
                        "Yes", "No"))
                        fileCandidates = Directory.EnumerateFiles(dir, "*.*",
                            recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
#endif
                }

                if (fileCandidates.Any())
                {
                    var importer = new DICOMImporter(fileCandidates, Path.GetFileName(dir));
                    var dataset = importer.Import();
                    if (dataset != null)
                        VolumeObjectFactory.CreateObject(dataset);
                }
                else
                {
                    Debug.LogError("Could not find any DICOM files to import.");
                }
            }
            else
            {
                Debug.LogError("Directory doesn't exist: " + dir);
            }
        }

        [MenuItem("Volume Rendering/Load image sequence")]
        private static void ShowSequenceImporter()
        {
            var dir = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");
            if (Directory.Exists(dir))
            {
                var importer = new ImageSequenceImporter(dir);
                var dataset = importer.Import();
                if (dataset != null)
                    VolumeObjectFactory.CreateObject(dataset);
            }
            else
            {
                Debug.LogError("Directory doesn't exist: " + dir);
            }
        }

        [MenuItem("Volume Rendering/Load Radial image sequence")]
        private static void ShowRadialSequenceImporter()
        {
            var dir = EditorUtility.OpenFolderPanel("Select a folder to load", "", "");
            if (Directory.Exists(dir))
            {
                var importer = new RadialImageSequenceImporter(dir);
                var dataset = importer.Import();
                if (dataset != null)
                    VolumeObjectFactory.CreateObject(dataset);
            }
            else
            {
                Debug.LogError("Directory doesn't exist: " + dir);
            }
        }

        [MenuItem("Volume Rendering/Cross section/Cross section plane")]
        private static void OnMenuItemClick()
        {
            var objects = Object.FindObjectsOfType<VolumeRenderedObject>();
            if (objects.Length == 1)
            {
                VolumeObjectFactory.SpawnCrossSectionPlane(objects[0]);
            }
            else
            {
                var wnd = new CrossSectionPlaneEditorWindow();
                wnd.Show();
            }
        }

        [MenuItem("Volume Rendering/Cross section/Box cutout")]
        private static void SpawnCutoutBox()
        {
            var objects = Object.FindObjectsOfType<VolumeRenderedObject>();
            if (objects.Length == 1)
                VolumeObjectFactory.SpawnCutoutBox(objects[0]);
        }
    }
}