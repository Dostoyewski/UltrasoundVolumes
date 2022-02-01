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
        private bool isSelectingSpotes = false;

        public void SpotsMode(bool state)
        {
            isSelectingSpotes = state;
        }
        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            
			if (GameObject.FindObjectOfType<VolumeRenderedObject>() == null && GUILayout.Button("Загрузить радиальную секвенцию"))
            {
                RuntimeFileBrowser.ShowOpenDirectoryDialog(OnRadialSequenceResult);
            }
            if (GameObject.FindObjectOfType<VolumeRenderedObject>() == null && GUILayout.Button("Загрузить линейную секвенцию"))
            {
                RuntimeFileBrowser.ShowOpenDirectoryDialog(OnLinearSequenceResult);
            }

            if ( GameObject.FindObjectOfType<VolumeRenderedObject>() != null && GUILayout.Button("Удалить скан"))
            {
                this.DespawnAllDatasets();
                this.DespawnAllCrossPlanes();
                this.DespawnBoxes();
            }
            
            // if ( GameObject.FindObjectOfType<CutoutBox>() != null && GUILayout.Button("Despawn cutout Box") )
            // {
            //     this.DespawnBoxes();
            // }
            
            if (GameObject.FindObjectOfType<CrossSectionPlane>() != null && GUILayout.Button("Удалить скальпель") )
            {
                DespawnAllCrossPlanes();
            }
            
            if (GameObject.FindObjectOfType<CrossSectionPlane>() == null && GameObject.FindObjectOfType<VolumeRenderedObject>() != null && GUILayout.Button("Скальпель") )
            {
                var objects = GameObject.FindObjectOfType<VolumeRenderedObject>();
                VolumeObjectFactory.SpawnCrossSectionPlane(objects);
                objects.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                var plane = GameObject.FindObjectOfType<CrossSectionPlane>();
                plane.transform.SetParent(objects.transform);
            }

            // if (GameObject.FindObjectOfType<VolumeRenderedObject>() != null && GUILayout.Button("Cutout Box") )
            // {
                // var objects = GameObject.FindObjectOfType<VolumeRenderedObject>();
                // VolumeObjectFactory.SpawnCutoutBox(objects);
				// objects.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                // var box = GameObject.FindObjectOfType<CutoutBox>();
                // box.transform.SetParent(objects.transform);
            // }

            // Show button for opening the dataset editor (for changing the visualisation)
            if (GameObject.FindObjectOfType<VolumeRenderedObject>() != null && GUILayout.Button("Редактировать скан"))
            {
                EditVolumeGUI.ShowWindow(GameObject.FindObjectOfType<VolumeRenderedObject>());
            }

            // if (GameObject.FindObjectOfType<CutoutBox>() != null && GUILayout.Button("Edit cutout box"))
            // {
            //     EditCutGUI.ShowWindow(GameObject.FindObjectOfType<CutoutBox>());
            // }
            if (GameObject.FindObjectOfType<VolumeRenderedObject>() != null && GUILayout.Button("Показать разрезы") )
            {
                isRenderingSlices = !isRenderingSlices;
                FindObjectOfType<Camera>().rect = isRenderingSlices? (new Rect(0,0.5f,0.5f,0.5f)):(new Rect(0,0,1.0f,1.0f));
                SlicingPlane[] planes = FindObjectsOfType<SlicingPlane>();
                foreach (SlicingPlane item in planes)
                {
                    item.isRendering = !item.isRendering;
                }
            }
            if (GameObject.FindObjectOfType<VolumeRenderedObject>() != null && GUILayout.Button("Выбрать споты") )
            {
                var objects = GameObject.FindObjectOfType<VolumeRenderedObject>();
                objects.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                objects.transform.position = new Vector3(0f, 0f, 0f);
                var spot = GameObject.FindObjectOfType<SpotCapsule>();
                spot.SetActive(true);
                spot.transform.SetParent(objects.transform);
                spot.transform.Rotate(0, 0, 90);
                isSelectingSpotes = true;

                var r1 = spot.GetMeshSize();
            }
            if (isSelectingSpotes && GUILayout.Button("Настройки выбора спотов"))
            {
                var spots = GameObject.FindObjectOfType<SpotCapsule>();
                EditSpotsGUI.ShowWindow(spots);
                var transCont = GameObject.FindObjectOfType<TransformController>();
                transCont.DisableRotation();
                var target = GameObject.FindObjectOfType<VolumeRenderedObject>();
                spots.targetObject = target;
            }
            if (GUILayout.Button("Путь выходного файла"))
            {
                RuntimeFileBrowser.ShowOpenDirectoryDialog(OnRobotPathResult);
            }
            if ( GUILayout.Button("Выход"))
            {
                Application.Quit();
            }
            GUILayout.EndHorizontal();
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

        private void OnRobotPathResult(RuntimeFileBrowser.DialogResult result)
        {
            if (!result.cancelled)
            {
                var writer = GameObject.FindObjectOfType<RobotWriter>();
                writer.Path = result.path;
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
