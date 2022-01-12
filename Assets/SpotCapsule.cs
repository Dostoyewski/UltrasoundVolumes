using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotCapsule : MonoBehaviour
{
    Renderer rend;
    public MeshRenderer meshRenderer;
    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetActive(bool isActive)
    {
        rend.enabled = isActive;
    }
    
    public float GetMeshSize()
    {
        float radius = rend.bounds.extents.magnitude;
        Debug.Log("Cylinder: " + Convert.ToString(radius));
        return radius;
    }
}
