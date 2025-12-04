using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceProjectileOnPlane : MonoBehaviour
{
    [Header("References")]
    public GameObject projectilePrefab;       // Prefab of cannon/projectile launcher
    public ARRaycastManager raycastManager;   // ARRaycastManager

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject spawnedObject;         // Reference to placed projectile prefab
    private bool isSelected = false;          // Whether user selected the object

    // Manipulation
    private float initialDistance;
    private Vector3 initialScale;
    private Vector2 lastTouchPos0;
    private Vector2 lastTouchPos1;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandleSelection();
        HandlePlacement();
        HandleManipulation();
    }

    // ----------------------------
    // 0. SELECT OBJECT BY TOUCH / CLICK
    // ----------------------------
    void HandleSelection()
    {
        if (spawnedObject == null) return;

        // Phone
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray r = cam.ScreenPointToRay(Input.GetTouch(0).position);
            if (Physics.Raycast(r, out RaycastHit hit))
            {
                if (hit.transform.gameObject == spawnedObject || 
                    hit.transform.IsChildOf(spawnedObject.transform))
                {
                    isSelected = true;
                    return;
                }
            }
            isSelected = false;
        }

        // Mouse
        if (Input.GetMouseButtonDown(0))
        {
            Ray r = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(r, out RaycastHit hit))
            {
                if (hit.transform.gameObject == spawnedObject ||
                    hit.transform.IsChildOf(spawnedObject.transform))
                {
                    isSelected = true;
                    return;
                }
            }
            isSelected = false;
        }
    }

    // ----------------------------
    // 1. PLACE OBJECT (only if not spawned OR not selected)
    // ----------------------------
    void HandlePlacement()
    {
        if (isSelected) return;   // prevent moving object by tapping plane

        // Phone tap
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            TryPlaceObject(Input.GetTouch(0).position);

        // Mouse click
        if (Input.GetMouseButtonDown(0))
            TryPlaceObject(Input.mousePosition);
    }

    void TryPlaceObject(Vector2 screenPos)
    {
        if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            if (spawnedObject == null)
            {
                // Spawn the projectile prefab
                spawnedObject = Instantiate(projectilePrefab, hitPose.position, hitPose.rotation);

                // Connect the ProjectileLauncher automatically (optional)
                ProjectileLauncher launcher = spawnedObject.GetComponentInChildren<ProjectileLauncher>();
                ProjectileUIBinder binder = Object.FindFirstObjectByType<ProjectileUIBinder>();
                if (binder != null && launcher != null)
                {
                    binder.BindLauncherAtRuntime(launcher);
                    Debug.Log("ProjectileLauncher connected to UI binder!");
                }
            }
            else if (!isSelected)
            {
                // Move object only if not selected
                spawnedObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            }
        }
    }

    // ----------------------------
    // 2. MOVE / ROTATE / SCALE — ONLY IF SELECTED
    // ----------------------------
    void HandleManipulation()
    {
        if (spawnedObject == null || !isSelected) return;

        // --------- PHONE MULTITOUCH ----------
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved)
                MoveObject(t.deltaPosition * 0.0015f);
        }
        else if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            // Scale
            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                initialDistance = Vector2.Distance(t0.position, t1.position);
                initialScale = spawnedObject.transform.localScale;
            }
            else
            {
                float currDist = Vector2.Distance(t0.position, t1.position);
                float scaleFactor = currDist / initialDistance;
                spawnedObject.transform.localScale = initialScale * scaleFactor;

                // Rotate based on twist
                float rotationDelta =
                    Vector2.SignedAngle(t1.position - lastTouchPos1, t1.position - t0.position);
                spawnedObject.transform.Rotate(Vector3.up, rotationDelta);
            }

            lastTouchPos0 = t0.position;
            lastTouchPos1 = t1.position;
        }

        // --------- MOUSE / PC CONTROLS ----------
        if (Input.GetMouseButton(0))
        {
            Vector3 move = new Vector3(Input.GetAxis("Mouse X"), 0, Input.GetAxis("Mouse Y"));
            MoveObject(move * 0.05f);
        }

        if (Input.GetMouseButton(1))
        {
            float rotX = Input.GetAxis("Mouse X") * 5f;
            spawnedObject.transform.Rotate(Vector3.up, rotX);
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            Vector3 direction = cam.transform.forward;
            spawnedObject.transform.position += direction * Input.mouseScrollDelta.y * 0.1f;
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButton(0))
        {
            float delta = Input.GetAxis("Mouse Y") * 0.01f;
            spawnedObject.transform.localScale += Vector3.one * delta;
        }
    }

    void MoveObject(Vector2 delta)
    {
        Vector3 move = cam.transform.right * delta.x + cam.transform.up * delta.y;
        spawnedObject.transform.position += move;
    }
}
