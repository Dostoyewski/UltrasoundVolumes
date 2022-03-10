using System.IO;
using UnityEngine;
using System.Globalization;


namespace UnityVolumeRendering
{
    public class HelpGUI : MonoBehaviour
    {
        private Rect windowRect = new Rect(150, 0, WINDOW_WIDTH, WINDOW_HEIGHT);

        private const int WINDOW_WIDTH = 400;
        private const int WINDOW_HEIGHT = 250;
        private bool ScalpelEnabled = false;

        private string help =
            "Для подгрузки секвенции нажмите кнопку `Загрузить секвенцию`" +
            " и укажите путь до папки с файлами.";
        
        private string ScalpelHelp = "Режим работы со скальпелем: перемещение осуществляется стрелочками, поворот осуществляется" +
        " нажатием кнопки R. ";
        private static HelpGUI instance;

        private int windowID;

        private void Awake()
        {
            // Fetch a unique ID for our window (see GUI.Window)
            windowID = WindowGUID.GetUniqueWindowID();
        }

        public void SetScalpel(bool stats)
        {
            ScalpelEnabled = stats;
        }

        private void Start()
        {
        }

        public static void ShowWindow()
        {
            if(instance != null)
                GameObject.Destroy(instance);

            GameObject obj = new GameObject($"HelpGUI");
            instance = obj.AddComponent<HelpGUI>();
        }

        private void OnGUI()
        {
            windowRect = GUI.Window(windowID, windowRect, UpdateWindow, $"Справка");
        }

        private void UpdateWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
            GUILayout.BeginVertical();

            
            GUILayout.FlexibleSpace();
            var runGui = GameObject.FindObjectOfType<RuntimeGUI>();
            if (runGui.scalpel)
            {
                GUILayout.Label(ScalpelHelp);
            }
            else
            {
                GUILayout.Label(help);
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            // Show close button
            if (GUILayout.Button("Закрыть"))
            {
                instance.Close();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void Close()
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}