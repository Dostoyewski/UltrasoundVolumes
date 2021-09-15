using UnityEngine;

namespace UnityVolumeRendering
{
    [ExecuteInEditMode]
    public class SlicingPlane : MonoBehaviour
    {
        private bool handleMouseMovement = false;
        private MeshRenderer meshRenderer;
        public Material mat;
        private Rect[] bgRects=new Rect[3];
        public int MainAxis=-1;
        private float[,] coord4rects={{0.0f, Screen.height/2},{Screen.width/2, Screen.height/2},{Screen.width/2, 0.0f}} ;
        private Vector2 prevMousePos=new Vector2();
        public bool isRendering;
        
        private void Start()
        {
            isRendering=false;
            meshRenderer = GetComponent<MeshRenderer>();
            //bgRect = new Rect(0.0f, Screen.height/2, Screen.width/2, Screen.height/2);
            for (int i = 0; i < 3; i++)
            {
                bgRects[i]=new Rect(coord4rects[i,0], coord4rects[i,1], Screen.width/2, Screen.height/2);
            }
            //AxisScales={transform.parent.lossyScale};
        }

        private void Update()
        {
            meshRenderer.sharedMaterial.SetMatrix("_parentInverseMat", transform.parent.worldToLocalMatrix);
            meshRenderer.sharedMaterial.SetMatrix("_planeMat", Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one)); // TODO: allow changing scale
        }
        void OnGUI ()
    {
        if (isRendering)
        {
            if (MainAxis>-1 && MainAxis<3)
            {
                mat = this.GetComponent<MeshRenderer>().sharedMaterial;
                Graphics.DrawTexture(bgRects[MainAxis], mat.GetTexture("_DataTex"), mat);    
            }
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && bgRects[MainAxis].Contains(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y)))
            {
                handleMouseMovement = true;
                prevMousePos = Event.current.mousePosition;
                this.GetComponent<MeshRenderer>().enabled=true;
            }

                // Handle mouse movement (move the plane)
            if (handleMouseMovement)
            {
                Vector2 mouseOffset = (Event.current.mousePosition - prevMousePos) / new Vector2(bgRects[MainAxis].width, bgRects[MainAxis].height);
                if (Mathf.Abs(mouseOffset.y) > 0.00001f)
                {
                    transform.Translate(Vector3.up * mouseOffset.y);
                        
                    prevMousePos = Event.current.mousePosition;
                }
            if (Event.current.type == EventType.MouseUp)
                handleMouseMovement = false;
                this.GetComponent<MeshRenderer>().enabled=false;
            }
            /*if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
            {
                
                //Vector2 mouseOffset = (Event.current.mousePosition - prevMousePos) / new Vector2(bgRects[MainAxis].width, bgRects[MainAxis].height);
                Vector2 mouseOffset = (Event.current.mousePosition - prevMousePos);
                Debug.Log(mouseOffset.y);
                Debug.Log(Screen.height/2);
                    if (Mathf.Abs(mouseOffset.y) > 0.00001f||Mathf.Abs(mouseOffset.y) <Screen.height/2)
                        {
                            transform.Translate(Vector3.up * mouseOffset.y);
                        }
                prevMousePos = Event.current.mousePosition;
            }*/
        }

    }
    }
}
