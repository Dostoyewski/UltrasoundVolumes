using System.IO;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Runtime (play mode) GUI for editing a slice orientation.
    /// </summary>
    public class EditCutGUI : MonoBehaviour
    {
        public CutoutBox slicingPlane;

        private Rect windowRect = new Rect(150, 0, WINDOW_WIDTH, WINDOW_HEIGHT);

        private const int WINDOW_WIDTH = 250;
        private const int WINDOW_HEIGHT = 200;

        private Vector3 scale1;
        private Vector3 scale2;
        
        private static EditCutGUI instance;

        private int windowID;

        private void Awake()
        {
            // Fetch a unique ID for our window (see GUI.Window)
            windowID = WindowGUID.GetUniqueWindowID();
            //FindObjectOfType<Transformer>().windowID.Add(windowID);
        }

        private void Start()
        {
            scale1=slicingPlane.transform.localPosition-slicingPlane.transform.localScale/2+0.5f*Vector3.one;
            scale2=slicingPlane.transform.localScale+scale1;
        }

        public static void ShowWindow(CutoutBox sliceRendObj)
        {
            if(instance != null)
                GameObject.Destroy(instance);

            GameObject obj = new GameObject($"EditCutGUI");
            instance = obj.AddComponent<EditCutGUI>();
            instance.slicingPlane = sliceRendObj;
            sliceRendObj.gameObject.GetComponent<MeshRenderer>().enabled=true;
        }

        private void OnGUI()
        {
            windowRect = GUI.Window(windowID, windowRect, UpdateWindow, $"Edit cut");
        }

        private void UpdateWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUILayout.BeginVertical();

            if(slicingPlane != null)
            {
                // Slice Rotation
                GUILayout.Label("Window");
                //GUILayout.Label("x:");
                scale1.x = GUILayout.HorizontalSlider(scale1.x, 0.0f, scale2.x);
                //GUILayout.Label("y:");
                scale1.y = GUILayout.HorizontalSlider(scale1.y, 0.0f, scale2.y);
                //GUILayout.Label("z:");
                scale1.z = GUILayout.HorizontalSlider(scale1.z, 0.0f, scale2.z);
                //GUILayout.Label("x:");
                scale2.x = GUILayout.HorizontalSlider(scale2.x, scale1.x, 1.0f);
                //GUILayout.Label("y:");
                scale2.y = GUILayout.HorizontalSlider(scale2.y, scale1.y, 1.0f);
                //GUILayout.Label("z:");
                scale2.z = GUILayout.HorizontalSlider(scale2.z, scale1.z, 1.0f);

                //slicingPlane.transform.localScale = scale2-scale1;
                //slicingPlane.transform.localPosition=(scale2-scale1)/2+scale1-0.5f*Vector3.one;
                FindObjectOfType<TransformController>().SetBoxPosandSc((scale2-scale1)/2+scale1-0.5f*Vector3.one,scale2-scale1);
                
            }

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            // Show close button
            if (GUILayout.Button("Close"))
            {
                slicingPlane.gameObject.GetComponent<MeshRenderer>().enabled=false;
                //FindObjectOfType<Transformer>().windowID.Remove(windowID);
                GameObject.Destroy(this.gameObject);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}
