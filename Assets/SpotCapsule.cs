using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            gameObject.transform.localScale = new Vector3(spotScale, spotScaleY, spotScale);
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
