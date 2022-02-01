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
                GUILayout.Label("Режим отображения");
                selectedRenderModeIndex = targetObject.GetRenderMode();
                selectedRenderModeIndex = GUILayout.SelectionGrid(selectedRenderModeIndex, new string[] { "Рамка", "Обрезка внутренняя", "Обрезка наружняя" }, 2);
                targetObject.SetRenderMode(selectedRenderModeIndex);

                // Visibility window
                GUILayout.Label("Минимальный и максимальный радиус селектора");
                GUILayout.BeginHorizontal();
                GUILayout.Label("min:");
                targetObject.SetMinRadius(GUILayout.HorizontalSlider(targetObject.GetMinRadius(), 0f, 1.0f, GUILayout.Width(150.0f)));
                GUILayout.Label("max:");
                targetObject.SetMaxRadius(GUILayout.HorizontalSlider(targetObject.GetMaxRadius(), 0f, 1.0f, GUILayout.Width(150.0f)));
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Опустить селектор", GUILayout.Width(200.0f)))
                {
                    targetObject.NextLevel();
                }

                GUILayout.Label("Current level: ");
                GUILayout.Label(targetObject.GetCurrentLevel().ToString());
                GUILayout.EndHorizontal();
                
                if(GUILayout.Button("Поднять селектор", GUILayout.Width(200.0f)))
                {
                    targetObject.PrevLevel();
                }

                // Load transfer function
                if(GUILayout.Button("Отправить споты на отжиг", GUILayout.Width(200.0f)))
                {
                    instance.Close();
                    targetObject.SetActive(false);
                    targetObject.SetRenderMode(0);
                    var writer = GameObject.FindObjectOfType<RobotWriter>();
                    writer.WriteSpotsFile();
                }
            }

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            // Show close button
            if (GUILayout.Button("Close"))
            {
                instance.Close();
            }
            if (GUILayout.Button("Disable Spots"))
            {
                instance.Close();
                targetObject.SetActive(false);
                var rGUI = GameObject.FindObjectOfType<RuntimeGUI>();
                if (rGUI != null) rGUI.SpotsMode(false);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void Close()
        {
            GameObject.Destroy(this.gameObject);
            var transCont = GameObject.FindObjectOfType<TransformController>();
            transCont.EnableRotation();
        }
    }
}
