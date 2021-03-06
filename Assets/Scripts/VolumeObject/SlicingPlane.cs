using System;
using UnityEngine;

namespace UnityVolumeRendering
{
    [ExecuteInEditMode]
    public class SlicingPlane : MonoBehaviour
    {
        private bool handleMouseMovement = false;
        private MeshRenderer meshRenderer;
        public Material mat;
        Texture texture;
        private SpotCapsule spot;
        private Rect[] bgRects = new Rect[3];
        public int MainAxis = -1;
        private float[,] coord4rects={{0.0f, Screen.height/2},{Screen.width/2, Screen.height/2},{Screen.width/2, 0.0f}} ;
        private Vector2 prevMousePos=new Vector2();
        public bool isRendering;
        private float prevAngleZ = 0;
        private VolumeRenderedObject volume = null;
        Vector3 pos;
        
        private void Start()
        {
            pos = transform.localPosition;
            isRendering = false;
            volume = GameObject.FindObjectOfType<VolumeRenderedObject>();
            spot = GameObject.FindObjectOfType<SpotCapsule>();
            meshRenderer = GetComponent<MeshRenderer>();
            for (int i = 0; i < 3; i++)
            {
                bgRects[i] = new Rect(coord4rects[i,0], coord4rects[i,1], Screen.width/2, Screen.height/2);
            }
        }

        private void Update()
        {
            var angles = volume.transform.rotation.eulerAngles;
            if (MainAxis == 0)
            {
                var quat = Quaternion.LookRotation(Vector3.forward, volume.transform.InverseTransformDirection(Vector3.forward));
                transform.localRotation = quat;
                transform.localPosition = quat * pos;
            }
            else if (MainAxis == 1)
            {
                if (spot.GetMode())
                {
                    transform.position = new Vector3(0, (float) -spot.GetCurrentLevel() / 10, 0);
                }
                else
                {
                    var quat = Quaternion.LookRotation(Vector3.up, volume.transform.InverseTransformDirection(Vector3.up));
                    transform.localRotation = quat;
                    transform.localPosition = quat*pos;
                }
            }
            else if (MainAxis == 2)
            {
                var quat = Quaternion.LookRotation(Vector3.forward, volume.transform.InverseTransformDirection(Vector3.right));
                transform.localRotation = quat;
                transform.localPosition = quat*pos;
            }
            meshRenderer.sharedMaterial.SetMatrix("_parentInverseMat", transform.parent.worldToLocalMatrix);
            meshRenderer.sharedMaterial.SetMatrix("_planeMat", Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one)); // TODO: allow changing scale
            meshRenderer.sharedMaterial.SetFloat("_Spotes", System.Convert.ToSingle(spot.GetMode()));
            meshRenderer.sharedMaterial.SetFloat("_PosZ", (float) (spot.GetCurrentLevel() / 10));
            meshRenderer.sharedMaterial.SetFloat("_ScaleX",  spot.GetScale());
            meshRenderer.sharedMaterial.SetFloat("_ScaleY",  spot.GetScaleY());
            meshRenderer.sharedMaterial.SetFloat("_ScaleZ",  spot.GetScale());
            meshRenderer.sharedMaterial.SetFloat("_Ablation",  System.Convert.ToSingle(spot.GetAblation()));
        }
        
        void OnGUI ()
        {
            var angles = volume.transform.eulerAngles;
            var controller = GameObject.FindObjectOfType<TransformController>();
            if (isRendering)
            {
                if (MainAxis > -1 && MainAxis < 3)
                {
                    mat = this.GetComponent<MeshRenderer>().sharedMaterial;
                    texture = mat.GetTexture("_DataTex");
                    Graphics.DrawTexture(bgRects[MainAxis], texture, mat);
                }
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && bgRects[MainAxis].Contains(new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y)))
                {
                    handleMouseMovement = true;
                    prevMousePos = Event.current.mousePosition;
                    controller.DisableRotation();
                }

                    // Handle mouse movement (move the plane)
                if (handleMouseMovement)
                {
                    Vector2 mouseOffset = (Event.current.mousePosition - prevMousePos) / new Vector2(bgRects[MainAxis].width, bgRects[MainAxis].height);
                    if (Mathf.Abs(mouseOffset.y) > 0.00001f)
                    {
                        pos+= Vector3.up * mouseOffset.y;
                        prevMousePos = Event.current.mousePosition;
                    }

                    if (Event.current.type == EventType.MouseUp)
                    {
                        handleMouseMovement = false;
                        controller.EnableRotation();
                    }
                }

                if (Math.Abs(angles.z - prevAngleZ) > 2)
                {
                    //prevAngleZ = angles.z;
                    //transform.position = Vector3.zero;
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
