using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityVolumeRendering
{

    public class TransformController : MonoBehaviour {

        static readonly string kMouseX = "Mouse X";
        static readonly string kMouseY = "Mouse Y";
        static readonly string kMouseScroll = "Mouse ScrollWheel";

        [SerializeField, Range(1f, 10f)] protected float zoomSpeed = 7.5f, zoomDelta = 5f;
        [SerializeField, Range(1f, 15f)] protected float zoomMin = 5f, zoomMax = 15f;

        [SerializeField, Range(1f, 10f)] protected float rotateSpeed = 7.5f, rotateDelta = 5f;
        private float scaleX = 1f;
        private float scaleY = 1f;
        private float scaleZ = 1f;
        private float spotScaleY = 0.05f;
        private float spotScale = 1f;
        private float posX = 0f;
        private float posY = 0f;
        private float posZ = 0f;
        private float scaleStep = 0.0007f;
        private float spotScaleStep = 0.5f;
        private float moveStep = 0.007f;
        private float currentDir = 0f;

        protected Camera cam;
        protected Vector3 targetCamPosition;
        protected Quaternion targetRotation;
        public VolumeRenderedObject targetObject;

        protected void Start () {
            cam = Camera.main;
            targetCamPosition = cam.transform.position;
            targetRotation = transform.rotation;
        }
        
        protected void Update () {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            var dt = Time.deltaTime;
            this.targetObject = GameObject.FindObjectOfType<VolumeRenderedObject>();
            Zoom(dt);
            Rotate(dt);
            MoveCross(dt);
            UpdateScaling(dt);
            UpdateCubePosition(dt);
        }
        public void SetBoxPosandSc(Vector3 pos, Vector3 scale)
        {
            posX = pos.x;
            posY = pos.y;
            posZ = pos.z;
            scaleX = scale.x;
            scaleY = scale.y;
            scaleZ = scale.z;   
        }

        protected void MoveCross(float dt)
        {
            var box = GameObject.FindObjectOfType<CutoutBox>();
            var plane = GameObject.FindObjectOfType<CrossSectionPlane>();
            var spot = GameObject.FindObjectOfType<SpotCapsule>();
            if (spot.GetMode())
            {
                spot.transform.localScale = new Vector3(spotScale, spotScaleY, spotScale);
            }
            if (box != null)
            {
                box.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
                box.transform.localPosition = new Vector3(posX, posY, posZ);
            }
            else if (plane != null)
            {
                plane.transform.localPosition = new Vector3(posX, posY, posZ);
                plane.transform.localScale = new Vector3(1, 1, 1);
                if (currentDir == 0)
                {
                    plane.transform.localRotation = Quaternion.Euler(90, 0, 0);
                }
                else if (currentDir == 1)
                {
                    plane.transform.localRotation = Quaternion.Euler(0, 90, 0);
                }
                else if (currentDir == 2)
                {
                    plane.transform.localRotation = Quaternion.Euler(0, 0, 90);
                }
            }
        }

        protected void UpdateCubePosition(float dt)
        {
            var delta = moveStep * dt; 
            if (Input.GetKey(KeyCode.UpArrow))
            {
                posY += delta;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                posY -= delta;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                posX += delta;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                posX -= delta;
            }
            if (Input.GetKey(KeyCode.Space))
            {
                posZ += delta;
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                posZ -= delta;
            }

            if (Input.GetKeyUp(KeyCode.R))
            {
                if (currentDir == 2)
                {
                    currentDir = 0;
                }
                else currentDir += 1;
            }
        }

        protected void UpdateScaling(float dt)
        {
            var delta1 = scaleStep * dt;
            var delta2 = spotScaleStep * dt;
            if (Input.GetKey(KeyCode.Q) && scaleX < 1)
            {
                scaleX += delta1;
            }
            if (Input.GetKey(KeyCode.E) && scaleX > 0)
            {
                scaleX -= delta1;
            }
            if (Input.GetKey(KeyCode.A) && scaleY < 1)
            {
                scaleY += delta1;
            }
            if (Input.GetKey(KeyCode.D) && scaleY > 0)
            {
                scaleY -= delta1;
            }
            if (Input.GetKey(KeyCode.Z) && scaleZ < 1)
            {
                scaleZ += delta1;
            }
            if (Input.GetKey(KeyCode.C) && scaleZ > 0)
            {
                scaleZ -= delta1;
            }
            if (Input.GetKey(KeyCode.Alpha2) && spotScale < 1)
            {
                spotScale += delta2;
                Debug.Log(spotScale);
            }
            if (Input.GetKey(KeyCode.Alpha1) && spotScale > 0)
            {
                spotScale -= delta2;
                Debug.Log(spotScale);
            }
        }

        protected void Zoom(float dt)
        {
            var amount = Input.GetAxis(kMouseScroll);
            if(Mathf.Abs(amount) > 0f)
            {
                targetCamPosition += cam.transform.forward * zoomSpeed * amount;
                targetCamPosition = targetCamPosition.normalized * Mathf.Clamp(targetCamPosition.magnitude, zoomMin, zoomMax);
            }
            //TODO: fix scaling
            // cam.transform.position = Vector3.Lerp(cam.transform.position, targetCamPosition, dt * zoomDelta);
        }

        protected void Rotate(float dt)
        {
            if (Input.GetMouseButton(0))
            {
                var mouseX = Input.GetAxis(kMouseX) * rotateSpeed;
                var mouseY = Input.GetAxis(kMouseY) * rotateSpeed;

                var up = transform.InverseTransformDirection(cam.transform.up);
                targetRotation *= Quaternion.AngleAxis(-mouseX, up);

                var right = transform.InverseTransformDirection(cam.transform.right);
                targetRotation *= Quaternion.AngleAxis(mouseY, right);
            }

            if (targetObject != null)
            {
                targetObject.transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, dt * rotateDelta);
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, dt * rotateDelta);
        }

    }

}


