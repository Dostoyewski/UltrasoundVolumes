using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityVolumeRendering
{
 public class SpotCapsule : MonoBehaviour
{
    Renderer rend;
    public MeshRenderer meshRenderer;
    private bool isSet;
    private float spotScaleY = 0.05f;
    private float spotScaleStep = 0.5f;
    private float spotMaxScale = 0.5f;
    private float spotScale = 1f;
    private float spotMinScale = 0.1f;
    private float spotMaxScaleY = 0.05f;
    private float currentLevel = 0.0f;

    private int mode = 0;
    public VolumeRenderedObject targetObject;

    public CutoutType cutoutType = CutoutType.Exclusive;
    
    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = false;
        spotScale = spotMaxScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSet)
        {
            gameObject.transform.localScale = new Vector3(spotScale, spotScaleY, spotScale);
            if (mode != 0) Cutout();
        }
    }

    public float GetScale()
    {
        return spotScale;
    }
    
    public float GetScaleY()
    {
        return spotScaleY;
    }

    public void SetRenderMode(int renderMode)
    {
        mode = renderMode;
        if (mode == 1) cutoutType = CutoutType.Inclusive;
        else if (mode == 2) cutoutType = CutoutType.Exclusive;
        else if (mode == 0) OnDisableCutout();
    }

    public int GetRenderMode()
    {
        return mode;
    }

    public float GetHeight()
    {
        return spotMaxScaleY;
    }
    
    private void OnDisableCutout()
    {
        if (targetObject != null)
        {
            targetObject.meshRenderer.sharedMaterial.DisableKeyword("CUTOUT_BOX_INCL");
            targetObject.meshRenderer.sharedMaterial.DisableKeyword("CUTOUT_BOX_EXCL");
        }
    }
    
    private void Cutout()
    {
        if (targetObject == null)
            return;

        Material mat = targetObject.meshRenderer.sharedMaterial;

        mat.DisableKeyword(cutoutType == CutoutType.Inclusive ? "CUTOUT_BOX_EXCL" : "CUTOUT_BOX_INCL");
        mat.EnableKeyword(cutoutType == CutoutType.Exclusive ? "CUTOUT_BOX_EXCL" : "CUTOUT_BOX_INCL");
        mat.SetMatrix("_CrossSectionMatrix", transform.worldToLocalMatrix * targetObject.transform.localToWorldMatrix);
    }

    public void NextLevel()
    {
        currentLevel += spotMaxScaleY;
        gameObject.transform.localPosition = new Vector3(0, 0, currentLevel);
    }
    
    public void PrevLevel()
    {
        currentLevel -= spotMaxScaleY;
        gameObject.transform.localPosition = new Vector3(0, 0, currentLevel);
    }

    public decimal GetCurrentLevel()
    {
        return Math.Round((decimal) currentLevel * 10, 2);
    }

    public float GetMaxRadius()
    {
        return spotMaxScale;
    }
    public float GetMinRadius()
    {
        return spotMinScale;
    }
    public void SetMaxRadius(float val)
    {
        spotMaxScale = val;
        if (spotScale > spotMaxScale) spotScale = spotMaxScale;
    }
    public void SetMinRadius(float val)
    {
        spotMinScale = val;
        if (spotScale < spotMinScale) spotScale = spotMinScale;
    }
    public float GetRadius()
    {
        return spotScale;
    }

    public void IncreaseRadius(float dt)
    {
        if (spotScale < spotMaxScale)
            spotScale += spotScaleStep * dt;
    }
    public void DecreaseRadius(float dt)
    {
        if (spotScale > spotMinScale)
            spotScale -= spotScaleStep * dt;
    }

    public bool GetMode()
    {
        return isSet;
    }

    public void SetActive(bool isActive)
    {
        isSet = isActive;
        rend.enabled = isActive;
    }
    
    public float GetMeshSize()
    {
        float radius = rend.bounds.extents.magnitude;
        Debug.Log("Cylinder: " + Convert.ToString(radius));
        return radius;
    }
}   
}
