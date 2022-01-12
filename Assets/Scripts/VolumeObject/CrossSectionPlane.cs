using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// Cross section plane.
    /// Used for cutting a model (cross section view).
    /// </summary>
    [ExecuteInEditMode]
    public class CrossSectionPlane : MonoBehaviour
    {
        /// <summary>
        /// Volume dataset to cross section.
        /// </summary>
        public VolumeRenderedObject targetObject;

        public SpotCapsule spot;

        private void OnDisable()
        {
            if (targetObject != null)
            {
                targetObject.meshRenderer.sharedMaterial.DisableKeyword("CUTOUT_PLANE");
                spot.meshRenderer.sharedMaterial.DisableKeyword("CUTOUT_PLANE");
            }
        }

        private void Update()
        {
            if (targetObject == null)
                return;

            Material mat = targetObject.meshRenderer.sharedMaterial;

            mat.EnableKeyword("CUTOUT_PLANE");
            mat.SetMatrix("_CrossSectionMatrix", transform.worldToLocalMatrix * targetObject.transform.localToWorldMatrix);
            
            Material mat2 = spot.meshRenderer.sharedMaterial;

            mat2.EnableKeyword("CUTOUT_PLANE");
            mat2.SetMatrix("_CrossSectionMatrix", transform.worldToLocalMatrix * targetObject.transform.localToWorldMatrix);
        }
    }
}
