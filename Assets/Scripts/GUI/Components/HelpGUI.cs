using System.IO;
using UnityEngine;
using System.Globalization;


namespace UnityVolumeRendering
{
    public class HelpGUI : MonoBehaviour
    {
        private Rect windowRect = new Rect(150, 0, WINDOW_WIDTH, WINDOW_HEIGHT);

        private const int WINDOW_WIDTH = 400;
        private const int WINDOW_HEIGHT = 350;
        private bool ScalpelEnabled = false;

        private string help =
            "Для подгрузки секвенции нажмите кнопку `Загрузить секвенцию`" +
            " и укажите путь до папки с файлами.";
        
        private string ScalpelHelp = "Режим работы со скальпелем: перемещение осуществляется стрелочками, вертикальное перемещение осуществляется кнопками Space и LShift" +
                                     ", поворот осуществляется" + 
                                     " нажатием кнопки R. ";

        private string EditingDataHelp =
            "Выбор режима рендеринга:\n1. Direct volume rendering - отображение всего объекта на основе данных УЗИ;\n" +
            "2. Maximum intensity projection - отображение только областей с максимальной интенсивностью белого цвета;\n" +
            "3. Isosurface rendering - отображение данных на основе интерполяции;\n" +
            "Visibility Window - настройка диапазона отображения пикселей, позволяет задавать для отображения пиксели с определенной" +
            " интенсивностью;\n" +
            "Build transfer function - генерация трансфер-функции отображения цветов (подбирает цветовую гамму для наиболее полной визуализации;\n" +
            "Load default transfer function - отображение данных в цветовой гамме <как есть>.";

        private string SlicesHelp =
            "Режим отображения разрезов объекта в нескольких плоскостях. Верхний левый экран - исследуемый объект, можно вращать. " +
            "Нижний левый экран - вертикальный разрез объекта в плоскости, перпендикулярной взгляду. Верхний правый экран - вертикальный разрез объекта в плоскости," +
            "параллельной взгляду. Нижний правый экран - горизонтальный разрез объекта. Все разрезы можно перемещать, двигая мышку по экрану " +
            "по вертикальной оси. При выборе спотов на всех экранах появляется текущее положение селектора в виде зеленого контура. Все точки, попавшие " +
            "в контур, будут отправлены на отжиг.";

        private string SpotesHelp = "Настройки выбора спотов:\n" +
                                    "1. Режим отображения <Рамка> отображает селектор как полый квадрат;\n" +
                                    "2. Режим отображения <Обрезка внутренняя> обрезает все, что находится за пределами селектора;\n" +
                                    "3. Режим отображения <Обрезка наружняя> вырезает из рассматриваемого объекта область внутри селектора;\n" +
                                    "Минимальный и макимальный радиус селектора: позволяет задавать диапазоны размера селектора. Радиус селектора изменяется с помощью " +
                                    "кнопок <1> и <2> на клавиатуре.\n" +
                                    "Кнопки <поднять селектор> и <опустить селектор> поднимают и опускают селектор на один уровень. Текущий уровень в миллиметрах отображается" +
                                    " в графе <Current Level>.\n" +
                                    "Кнопка <Отправить споты на отжиг> отправляет споты на робота.\n" +
                                    "Кнопка <тестовая абляция> закрашивает область спотов черным для оценки выжигаемой области.";
        
        private string DefaultHelp =
            " Для включения скальпеля нажмите на кнопку <Скальпель>;\n Для редактирования скана нажмите на кнопку <Редактировать скан>;\n" +
            " Для отображения разрезов нажмите кнопку <Показать разрезы>;\n Для выбора спотов нажмите кнопку <Выбор спотов>, затем откройте" +
            " интерфейс настройки спотов, нажав на кнопку <Настройка выбора спотов>;\n Для запрета перемещения по вертикальной оси нажмите на кнопку " +
            "<Фиксация верт. оси>.";
        
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
            else if (runGui.isEditingDataset)
            {
                GUILayout.Label(EditingDataHelp);
            }
            else if (runGui.isRenderingSlices)
            {
                GUILayout.Label(SlicesHelp);
            }
            else if (runGui.isSelectingSpotes)
            {
                GUILayout.Label(SpotesHelp);
            }
            else if (GameObject.FindObjectOfType<VolumeRenderedObject>() != null)
            {
                GUILayout.Label(DefaultHelp);
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