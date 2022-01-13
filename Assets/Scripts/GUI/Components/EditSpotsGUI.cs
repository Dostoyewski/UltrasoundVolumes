using System.IO;
using UnityEngine;
using System.Globalization;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Rutnime (play mode) GUI for editing a volume's visualisation.
    /// </summary>
    public class EditSpotsGUI : MonoBehaviour
    {
        public SpotCapsule targetObject;

        private Rect windowRect = new Rect(150, 0, WINDOW_WIDTH, WINDOW_HEIGHT);

        private const int WINDOW_WIDTH = 400;
        private const int WINDOW_HEIGHT = 250;

        private int selectedRenderModeIndex = 0;
        private Vector3 rotation;

        private static EditSpotsGUI instance;

        private int windowID;

        private void Awake()
        {
            // Fetch a unique ID for our window (see GUI.Window)
            windowID = WindowGUID.GetUniqueWindowID();
        }

        private void Start()
        {
            rotation = targetObject.transform.rotation.eulerAngles;
        }

        public static void ShowWindow(SpotCapsule spots)
        {
            if(instance != null)
                GameObject.Destroy(instance);

            GameObject obj = new GameObject($"Edit spots settings");
            instance = obj.AddComponent<EditSpotsGUI>();
            instance.targetObject = spots;
        }

        private void OnGUI()
        {
            windowRect = GUI.Window(windowID, windowRect, UpdateWindow, $"Edit spots selector");
        }

        private void UpdateWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
            GUILayout.BeginVertical();

            if(targetObject != null)
            {
                // Render mode
                GUILayout.Label("Render mode");
                selectedRenderModeIndex = GUILayout.SelectionGrid(selectedRenderModeIndex, new string[] { "Direct volume rendering", "Maximum intensity projection", "Isosurface rendering" }, 2);
                // targetObject.SetRenderMode((RenderMode)selectedRenderModeIndex);

                // Visibility window
                GUILayout.Label("Selector minimum and maximum radius");
                GUILayout.BeginHorizontal();
                GUILayout.Label("min:");
                targetObject.SetMinRadius(GUILayout.HorizontalSlider(targetObject.GetMinRadius(), 0f, 1.0f, GUILayout.Width(150.0f)));
                GUILayout.Label("max:");
                targetObject.SetMaxRadius(GUILayout.HorizontalSlider(targetObject.GetMaxRadius(), 0f, 1.0f, GUILayout.Width(150.0f)));
                GUILayout.EndHorizontal();
                
                
                if(GUILayout.Button("Build transfer function", GUILayout.Width(200.0f)))
                {
                    // TransferFunction tf = TransferFunctionDatabase.CreateTransferFunction();
                    // targetObject.transferFunction = tf;
                }
                
                if(GUILayout.Button("Load default transfer function", GUILayout.Width(200.0f)))
                {
                    // TransferFunction tf = TransferFunctionDatabase.CreateDefaultTransferFunction();
                    // targetObject.transferFunction = tf;
                }

                // Load transfer function
                if(GUILayout.Button("Load transfer function", GUILayout.Width(200.0f)))
                {
                    // RuntimeFileBrowser.ShowOpenFileDialog(OnLoadTransferFunction);
                }
            }

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            // Show close button
            if (GUILayout.Button("Close"))
            {
                GameObject.Destroy(this.gameObject);
                var transCont = GameObject.FindObjectOfType<TransformController>();
                transCont.EnableRotation();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}
