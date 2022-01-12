using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
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
        bool isRenderingSlices = false;
        private bool isSelectingSpotes = true;
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

            if (  GameObject.FindObjectOfType<VolumeRenderedObject>() != null && GUILayout.Button("Despawn dataset"))
            {
                this.DespawnAllDatasets();
                this.DespawnAllCrossPlanes();
                this.DespawnBoxes();
            }
            
            if ( GameObject.FindObjectOfType<CutoutBox>() != null && GUILayout.Button("Despawn cutout Box") )
            {
                this.DespawnBoxes();
            }
            
            if (GameObject.FindObjectOfType<CrossSectionPlane>() != null && GUILayout.Button("Despawn scalpels") )
            {
                DespawnAllCrossPlanes();
            }
            
            if (GameObject.FindObjectOfType<VolumeRenderedObject>() != null && GUILayout.Button("Scalpel") )
            {
                var objects = GameObject.FindObjectOfType<VolumeRenderedObject>();
                VolumeObjectFactory.SpawnCrossSectionPlane(objects);
                objects.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                var plane = GameObject.FindObjectOfType<CrossSectionPlane>();
                plane.transform.SetParent(objects.transform);
            }

            if (GameObject.FindObjectOfType<VolumeRenderedObject>() != null && GUILayout.Button("Cutout Box") )
            {
                var objects = GameObject.FindObjectOfType<VolumeRenderedObject>();
                VolumeObjectFactory.SpawnCutoutBox(objects);
				objects.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                var box = GameObject.FindObjectOfType<CutoutBox>();
                box.transform.SetParent(objects.transform);
            }

            // Show button for opening the dataset editor (for changing the visualisation)
            if (GameObject.FindObjectOfType<VolumeRenderedObject>() != null && GUILayout.Button("Edit imported dataset"))
            {
                EditVolumeGUI.ShowWindow(GameObject.FindObjectOfType<VolumeRenderedObject>());
            }

            if (GameObject.FindObjectOfType<CutoutBox>() != null && GUILayout.Button("Edit cutout box"))
            {
                EditCutGUI.ShowWindow(GameObject.FindObjectOfType<CutoutBox>());
            }
            if (GameObject.FindObjectOfType<VolumeRenderedObject>() != null && GUILayout.Button("Show slices") )
            {
                isRenderingSlices = !isRenderingSlices;
                FindObjectOfType<Camera>().rect = isRenderingSlices? (new Rect(0,0.5f,0.5f,0.5f)):(new Rect(0,0,1.0f,1.0f));
                SlicingPlane[] planes = FindObjectsOfType<SlicingPlane>();
                foreach (SlicingPlane item in planes)
                {
                    item.isRendering = !item.isRendering;
                }
            }
            if (GameObject.FindObjectOfType<VolumeRenderedObject>() != null && GUILayout.Button("Select spotes") )
            {
                var objects = GameObject.FindObjectOfType<VolumeRenderedObject>();
                objects.transform.rotation = Quaternion.Euler(90f, 90f, 90f);
                objects.transform.position = new Vector3(0f, 0f, 0f);
                var spot = GameObject.FindObjectOfType<SpotCapsule>();
                spot.transform.SetParent(objects.transform);
            }

            if ( GUILayout.Button("Exit"))
            {
                Application.Quit();
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
                string captPath = filePath + "/CaptSave.tag";
                ProcessStartInfo startInfo = new ProcessStartInfo("C:/Images.exe");
                startInfo.Arguments = filePath + " " + captPath;
                startInfo.UseShellExecute = true;
                Process p = Process.Start(startInfo);
                p.WaitForExit();
                RadialImageSequenceImporter importer = new RadialImageSequenceImporter(filePath + "/result_new_alg2_blur");
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
