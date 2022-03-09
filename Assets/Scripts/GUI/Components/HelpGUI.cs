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

        private string help =
            "Для подгрузки секвенции нажмите кнопку `Загрузить секвенцию`" +
            " и укажите путь до папки с файлами. Режим работы со скальпелем: перемещение осуществляется стрелочками, поворот осуществляется" +
            " нажатием кнопки R. ";
        private static HelpGUI instance;

        private int windowID;

        private void Awake()
        {
            // Fetch a unique ID for our window (see GUI.Window)
            windowID = WindowGUID.GetUniqueWindowID();
        }

        private void Start()
        {
        }

        public static void ShowWindow()
        {
            if(instance != null)
                GameObject.Destroy(instance);

            GameObject obj = new GameObject($"Справка");
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
            GUILayout.Label(help);
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