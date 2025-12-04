using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class ProjectileBehavior : MonoBehaviour
{
    [Header("Landing & Detection")]
    public UnityEvent onLanded;             // Event to notify landing
    public string groundTag = "Barrier";    // Tag for ground or plane
    public float sleepVelocityThreshold = 0.2f;  // Threshold for stopping detection
    public float checkSleepDelay = 0.25f;        // Delay before checking rest state

    [Header("Flight Tracking")]
    public float timeOfFlight { get; private set; } = 0f;  // Time since launch
    public float maxHeight { get; private set; } = 0f;     // Maximum height reached
    public float range { get; private set; } = 0f;         // Horizontal distance traveled

    private Vector3 launchPosition;
    private bool launched = false;
    private bool hasLanded = false;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogError("ProjectileBehavior requires a Rigidbody component.");
    }

    void Start()
    {
        // Initial launch position
        launchPosition = transform.position;
        maxHeight = launchPosition.y;
    }

    void FixedUpdate()
    {
        if (launched && !hasLanded)
        {
            // Track time
            timeOfFlight += Time.fixedDeltaTime;

            // Track max height
            if (transform.position.y > maxHeight)
                maxHeight = transform.position.y;
        }
    }

    /// <summary>
    /// Call this method to launch the projectile.
    /// </summary>
    /// <param name="velocity">Initial velocity vector</param>
    public void Launch(Vector3 velocity)
    {
        if (rb == null) return;

        rb.linearVelocity = velocity;
        rb.angularVelocity = Vector3.zero;
        launched = true;
        hasLanded = false;

        launchPosition = transform.position;
        timeOfFlight = 0f;
        maxHeight = launchPosition.y;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasLanded) return;

        // Immediate landing if hit ground-tagged object
        if (!string.IsNullOrEmpty(groundTag) && collision.collider.CompareTag(groundTag))
        {
            Land();
            return;
        }

        // Otherwise, schedule a sleep check
        if (!hasLanded)
            Invoke(nameof(CheckIfSleeping), checkSleepDelay);
    }

    void CheckIfSleeping()
    {
        if (hasLanded || rb == null) return;

        if (rb.IsSleeping() || rb.linearVelocity.magnitude <= sleepVelocityThreshold)
            Land();
    }

    void Land()
    {
        if (hasLanded) return;

        hasLanded = true;
        launched = false;

        // Compute horizontal range
        Vector3 landingPos = transform.position;
        range = Vector3.Distance(
            new Vector3(launchPosition.x, 0, launchPosition.z),
            new Vector3(landingPos.x, 0, landingPos.z)
        );

        Debug.Log($"Projectile Landed → Time: {timeOfFlight:F2}s | Max Height: {maxHeight:F2}m | Range: {range:F2}m");

        // Call your UI popup (ensure ResultUI exists in your project)
        ResultUI.Instance?.Show(timeOfFlight, range, maxHeight);

        // Invoke any additional listeners
        onLanded?.Invoke();
    }
}
