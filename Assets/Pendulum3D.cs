using UnityEngine;

public class Pendulum3D : MonoBehaviour
{
    [Header("Pendulum Settings")]
    public float gravity = 9.81f;
    public float damping = 0.995f;

    [Header("References")]
    public Transform stringCylinder;   // Assign Cylinder here
    public Transform bob;              // Assign Sphere here

    private float angle;
    private float angularVelocity;
    private float angularAcceleration;

    private float configuredAngle;
    private float configuredLength;

    private Transform pivot;
    private bool isRunning = false;

    void Start()
    {
        pivot = transform.parent;   // This object (Pendulum) is child of Pivot
        Configure(20f, 2f);         // Default: 20° angle, 2m length
    }

    void FixedUpdate()
    {
        if (!isRunning) return;

        // Physics for angular motion
        angularAcceleration = (-gravity / configuredLength) * Mathf.Sin(angle);
        angularVelocity += angularAcceleration * Time.deltaTime;
        angularVelocity *= damping;             // slow down over time
        angle += angularVelocity * Time.deltaTime;

        UpdatePendulum();
    }

    void UpdatePendulum()
    {
        // 🔥 Rotate the WHOLE pendulum (string + bob)
        transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

        // ---------------- BOB POSITION ----------------
        if (bob != null)
        {
            // Bob always sits at (-length)
            bob.localPosition = new Vector3(0f, -configuredLength, 0f);
        }

        // ---------------- STRING (CYLINDER) ----------------
        if (stringCylinder != null)
        {
            // Cylinder height = half of total length
            stringCylinder.localScale = new Vector3(
                stringCylinder.localScale.x,
                configuredLength / 2f,
                stringCylinder.localScale.z
            );

            // Move cylinder so its TOP stays at pivot
            stringCylinder.localPosition = new Vector3(
                0f,
                -configuredLength / 2f,
                0f
            );
        }
    }

    // ---------------- PUBLIC API ----------------

    public void Configure(float angleDeg, float length)
    {
        configuredAngle = angleDeg;
        configuredLength = Mathf.Max(0.1f, length);  // avoid zero length

        angle = configuredAngle * Mathf.Deg2Rad;
        angularVelocity = 0f;
        angularAcceleration = 0f;

        UpdatePendulum();  // refresh UI before playing
    }

    public void Play()
    {
        isRunning = true;
    }

    public void Stop()
    {
        isRunning = false;
        angularVelocity = 0f;

        Configure(configuredAngle, configuredLength);
    }
}
