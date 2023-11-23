using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.XR.ARFoundation;

public class PolygonScan : MonoBehaviour
{
    [SerializeField] private GameObject pointer;
    [SerializeField] private PolygonDrawer polygonDrawer;

    public void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Physics.Raycast(ray, out RaycastHit hitData);

        if (hitData.transform != null && hitData.transform.gameObject.name.Contains("ARPlane") &&
                (hitData.normal - Vector3.up).magnitude < 0.05f)
        {
            // Check if hitpoint is on a horizontal ar plane.
            pointer.transform.position = hitData.point;

            polygonDrawer.SetPointer(pointer.transform.position);
        }
        else
        {
            // Check if plane and ray are not parrallel.
            if (ray.direction.y != 0)
            {
                float l = (-1.5f - ray.origin.y) / ray.direction.y;
                // Check if ray is not reversed
                if (l > 0f)
                {
                    pointer.transform.position = ray.origin + ray.direction * l;

                    polygonDrawer.SetPointer(pointer.transform.position);
                }
            }
        }


        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
        {
            polygonDrawer.AddPoint();
        }
    }

}
