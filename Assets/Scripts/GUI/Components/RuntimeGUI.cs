using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// This is a basic runtime GUI, that can be used during play mode.
    /// You can import datasets, and edit them.
    /// Add this component to an empty GameObject in your scene (it's already in the test scene) and click play to see the GUI.
    /// </summary>
    public class RuntimeGUI : MonoBehaviour
    {
        private void OnGUI()
        {
            GUILayout.BeginVertical();
            
			if (GUILayout.Button("Import Radial sequence"))
            {
                RuntimeFileBrowser.ShowOpenDirectoryDialog(OnRadialSequenceResult);
            }
            if (GUILayout.Button("Import Linear sequence"))
            {
                RuntimeFileBrowser.ShowOpenDirectoryDialog(OnLinearSequenceResult);
            }

            if (GUILayout.Button("Despawn dataset") && GameObject.FindObjectOfType<VolumeRenderedObject>() != null)
            {
                this.DespawnAllDatasets();
                this.DespawnAllCrossPlanes();
                this.DespawnBoxes();
            }
            
            if (GUILayout.Button("Despawn cutout Box") && GameObject.FindObjectOfType<CutoutBox>() != null)
            {
                this.DespawnBoxes();
            }

            if (GUILayout.Button("Cutout Box") && GameObject.FindObjectOfType<VolumeRenderedObject>() != null)
            {
                var objects = GameObject.FindObjectOfType<VolumeRenderedObject>();
                VolumeObjectFactory.SpawnCutoutBox(objects);
            }

            // Show button for opening the dataset editor (for changing the visualisation)
            if (GameObject.FindObjectOfType<VolumeRenderedObject>() != null && GUILayout.Button("Edit imported dataset"))
            {
                EditVolumeGUI.ShowWindow(GameObject.FindObjectOfType<VolumeRenderedObject>());
            }

            // Show button for opening the slicing plane editor (for changing the orientation and position)
            if (GameObject.FindObjectOfType<SlicingPlane>() != null && GUILayout.Button("Edit slicing plane"))
            {
                EditSliceGUI.ShowWindow(GameObject.FindObjectOfType<SlicingPlane>());
            }

            GUILayout.EndVertical();
        }

        private void OnOpenRAWDatasetResult(RuntimeFileBrowser.DialogResult result)
        {
            if(!result.cancelled)
            {
                // We'll only allow one dataset at a time in the runtime GUI (for simplicity)
                DespawnAllDatasets();

                // Did the user try to import an .ini-file? Open the corresponding .raw file instead
                string filePath = result.path;
                if (System.IO.Path.GetExtension(filePath) == ".ini")
                    filePath = filePath.Replace(".ini", ".raw");

                // Parse .ini file
                DatasetIniData initData = DatasetIniReader.ParseIniFile(filePath + ".ini");
                if(initData != null)
                {
                    // Import the dataset
                    RawDatasetImporter importer = new RawDatasetImporter(filePath, initData.dimX, initData.dimY, initData.dimZ, initData.format, initData.endianness, initData.bytesToSkip);
                    VolumeDataset dataset = importer.Import();
                    // Spawn the object
                    if (dataset != null)
                    {
                        VolumeObjectFactory.CreateObject(dataset);
                    }
                }
            }
        }
        
        private void OnLinearSequenceResult(RuntimeFileBrowser.DialogResult result)
        {
            if (!result.cancelled)
            {
                // We'll only allow one dataset at a time in the runtime GUI (for simplicity)
                DespawnAllDatasets();

                string filePath = result.path;

                ImageSequenceImporter importer = new ImageSequenceImporter(filePath);
                VolumeDataset dataset = importer.Import();
                // Spawn the object
                if (dataset != null)
                {
                    VolumeObjectFactory.CreateObject(dataset);
                }
            }
        }

		private void OnRadialSequenceResult(RuntimeFileBrowser.DialogResult result)
        {
            if (!result.cancelled)
            {
                // We'll only allow one dataset at a time in the runtime GUI (for simplicity)
                DespawnAllDatasets();

                string filePath = result.path;

                RadialImageSequenceImporter importer = new RadialImageSequenceImporter(filePath);
                VolumeDataset dataset = importer.Import();
                // Spawn the object
                if (dataset != null)
                {
                    VolumeObjectFactory.CreateObject(dataset);
                }
            }
        }

        private void OnOpenDICOMDatasetResult(RuntimeFileBrowser.DialogResult result)
        {
            if (!result.cancelled)
            {
                // We'll only allow one dataset at a time in the runtime GUI (for simplicity)
                DespawnAllDatasets();

                bool recursive = true;

                // Read all files
                IEnumerable<string> fileCandidates = Directory.EnumerateFiles(result.path, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase));

                // Import the dataset
                DICOMImporter importer = new DICOMImporter(fileCandidates, Path.GetFileName(result.path));
                VolumeDataset dataset = importer.Import();
                // Spawn the object
                if (dataset != null)
                {
                    VolumeObjectFactory.CreateObject(dataset);
                }
            }
        }

        private void DespawnAllDatasets()
        {
            VolumeRenderedObject[] volobjs = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            foreach(VolumeRenderedObject volobj in volobjs)
            {
                GameObject.Destroy(volobj.gameObject);
            }
        }

        private void DespawnAllCrossPlanes()
        {
            CrossSectionPlane[] cobjs = GameObject.FindObjectsOfType<CrossSectionPlane>();
            foreach(CrossSectionPlane cobj in cobjs)
            {
                GameObject.Destroy(cobj.gameObject);
            }
        }
        
        private void DespawnBoxes()
        {
            CutoutBox[] cobjs = GameObject.FindObjectsOfType<CutoutBox>();
            foreach(CutoutBox cobj in cobjs)
            {
                GameObject.Destroy(cobj.gameObject);
            }
        }

    }
}
