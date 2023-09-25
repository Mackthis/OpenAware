using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class PlaceSpacedObject : MonoBehaviour
{
    private ARRaycastManager aRRaycastManager;
    private ARPlaneManager aRPlaneManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private EventSystem eventSystem;    
    

    private void Awake()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        aRPlaneManager = GetComponent<ARPlaneManager>();
        eventSystem = FindObjectOfType<EventSystem>();
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable");
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
    }

    private void OnDisable()
    {
        Debug.Log("OnDisable");
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
    }

    //* Interact using touch screen (mobile)
    private void FingerDown(EnhancedTouch.Finger finger)
    {
        if (finger.index != 0) return;

        //TODO Hij interact nog steeds door de UI heen dus fix dit :)
        //* Don't interact with world when clicking on a UI element
        if(eventSystem.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;

        Interact(finger.currentTouch.screenPosition);
    }

    //* Interact using mouse button (pc)
    private void Update()
    {
        //* Don't interact with world when clicking on a UI element
        if (eventSystem.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
            Interact(Input.mousePosition);
    }

    private void Interact(Vector2 screenPoint)
    {
        if (aRRaycastManager.Raycast(screenPoint, hits, TrackableType.PlaneWithinPolygon))
        {
            foreach (ARRaycastHit hit in hits)
            {
                Pose pose = hit.pose;

                // Temporarily instantiate the object to get the BoxCollider size
                GameObject tempObj = Instantiate(ObjectPrefabs.I.prefabs[ObjectPrefabs.I.prefabIndex], new Vector3(0, 0, 0), Quaternion.identity);
                BoxCollider tempCollider = tempObj.AddComponent<BoxCollider>();
                Vector3 halfExtents = tempCollider.size / 2;
                float center_Height = tempCollider.size.y / 2;
                Destroy(tempObj);

                // Create the position where the new object should be placed (+ add slight hover to prevent floor collisions)
                Vector3 newPosition = new Vector3(pose.position.x, pose.position.y + center_Height + 0.1f, pose.position.z);
    
                // Create a list of colliders that prevent an object from being placed
                Collider[] overlappingColliders;
                
                // Check if the box overlaps with any other colliders
                if (!Physics.CheckBox(newPosition, halfExtents, pose.rotation))
                {
                    GameObject obj = Instantiate(ObjectPrefabs.I.prefabs[ObjectPrefabs.I.prefabIndex], pose.position, pose.rotation);
                    BoxCollider boxCollider = obj.AddComponent<BoxCollider>();

                    if (aRPlaneManager.GetPlane(hit.trackableId).alignment == PlaneAlignment.HorizontalUp)
                    {
                        Vector3 position = obj.transform.position;
                        Vector3 cameraPosition = Camera.main.transform.position;
                        Vector3 direction = cameraPosition - position;
                        Vector3 targetRotationEuler = Quaternion.LookRotation(direction).eulerAngles;
                        Vector3 scaledEuler = Vector3.Scale(targetRotationEuler, obj.transform.up.normalized);
                        Quaternion targetRotation = Quaternion.Euler(scaledEuler);
                        obj.transform.rotation *= targetRotation;
                    }
                }
                else
                {
                    // Log a message if you want to know when an object can't be placed
                    Debug.Log("Can't place object. It would overlap with another.");

                    // Get all overlapping colliders + draw a fading cube around each overlapping collider for visualization
                    overlappingColliders = Physics.OverlapBox(newPosition, halfExtents, pose.rotation);
                    foreach (var collider in overlappingColliders)
                    {
                        BoxCollider box = collider as BoxCollider; 
                        if (box)
                        {
                            CreateVisualBox(box);
                        }
                    }
                }
            }
        }
    }

    public void CreateVisualBox(BoxCollider boxCollider)
    {
        // Create a new GameObject
        GameObject visualBox = new GameObject("Visual Box");

        // Set visual box properties
        MeshRenderer meshRenderer = visualBox.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = visualBox.AddComponent<MeshFilter>();
        meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.color = new Color(1.0f, 0.0f, 0.0f, 1.0f); 

        // Set the size, position, and rotation
        visualBox.transform.position = boxCollider.transform.TransformPoint(boxCollider.center);
        visualBox.transform.rotation = boxCollider.transform.rotation;
        visualBox.transform.localScale = Vector3.Scale(boxCollider.transform.lossyScale, boxCollider.size);

        // Transparent material
        meshRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        meshRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        meshRenderer.material.SetInt("_ZWrite", 0);
        meshRenderer.material.DisableKeyword("_ALPHATEST_ON");
        meshRenderer.material.DisableKeyword("_ALPHABLEND_ON");
        meshRenderer.material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        meshRenderer.material.renderQueue = 3000;

        // Start fading out the visualBox
        StartCoroutine(FadeOutAndDestroy(visualBox, 3f));
    }
    
    public IEnumerator FadeOutAndDestroy(GameObject target, float duration)

    {
        float counter = 0;
        MeshRenderer meshRenderer = target.GetComponent<MeshRenderer>();
        Color startColor = meshRenderer.material.color;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, counter / duration);
            meshRenderer.material.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(target);
    }
}



