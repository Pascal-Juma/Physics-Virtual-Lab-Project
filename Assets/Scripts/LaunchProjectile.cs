using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProjectileLauncher : MonoBehaviour
{
    [Header("References")]
    public Transform pivot;
    public Transform barrel;
    public Transform barrelTip;
    public GameObject projectilePrefab;

    [Header("UI Controls")]
    public Slider angleSlider;
    public Slider velocitySlider;
    public TMP_Dropdown gravityDropdown;
    public Button launchButton;

    [Header("UI Display References")]
    public TextMeshProUGUI angleLabel;         
    public TextMeshProUGUI velocityLabel;      
    public TextMeshProUGUI gravityLabel;        // Optional - displays the current gravity name

    [Header("Launch Settings")]
    public bool useInstantiate = true;
    public float projectileAutoDestroy = 10f;

    private readonly float[] gravityValues = { 9.81f, 1.62f, 3.71f, 24.79f, 0f };

    private readonly string[] gravityNames = 
        { "Earth (9.81)", "Moon (1.62)", "Mars (3.71)", "Jupiter (24.79)", "Zero (0.00)" };

    void Start()
    {
        // Safety checks
        if (pivot == null || barrel == null || barrelTip == null)
            Debug.LogWarning("Basic references missing!");

        if (angleSlider == null || velocitySlider == null || gravityDropdown == null || launchButton == null)
            Debug.LogWarning("UI references missing!");

        // Hook UI
        angleSlider.onValueChanged.AddListener(OnAngleChanged);
        velocitySlider.onValueChanged.AddListener(OnVelocityChanged);
        gravityDropdown.onValueChanged.AddListener(OnGravityChanged);
        launchButton.onClick.AddListener(OnLaunchClicked);

        OnAngleChanged(angleSlider.value);
        OnVelocityChanged(velocitySlider.value);
        OnGravityChanged(gravityDropdown.value);
    }

    void OnDestroy()
    {
        angleSlider.onValueChanged.RemoveAllListeners();
        velocitySlider.onValueChanged.RemoveAllListeners();
        gravityDropdown.onValueChanged.RemoveAllListeners();
        launchButton.onClick.RemoveAllListeners();
    }

    public void OnAngleChanged(float val)
    {
        pivot.localEulerAngles = new Vector3(0f, 0f, -val);

        if (angleLabel != null)
            angleLabel.text = $"{val:F1}°";
    }

    public void OnVelocityChanged(float val)
    {
        if (velocityLabel != null)
            velocityLabel.text = $"{val:F1} m/s";
    }

    public void OnGravityChanged(int index)
    {
        if (index < 0 || index >= gravityValues.Length) return;

        float g = gravityValues[index];
        Physics.gravity = new Vector3(0f, -g, 0f);

        if (gravityLabel != null)
            gravityLabel.text = "Gravity: " + gravityNames[index];
    }

    public void OnLaunchClicked()
    {
        // launchButton.interactable = false;
        FireProjectile();
    }

void FireProjectile()
{
    Vector3 dir = barrel.transform.up.normalized;
    float speed = velocitySlider.value;

    // Instantiate projectile prefab
    GameObject projGO = Instantiate(projectilePrefab, barrelTip.position, Quaternion.identity);

    // Get ProjectileBehavior
    ProjectileBehavior pb = projGO.GetComponent<ProjectileBehavior>();
    if (pb != null)
    {
        // Launch the projectile using Launch method
        Vector3 launchVelocity = dir * speed;
        pb.Launch(launchVelocity);

        // Listen for landing event
        pb.onLanded.RemoveAllListeners();
        pb.onLanded.AddListener(() =>
        {
            launchButton.interactable = true;

            // Calculate and display results
            float timeOfFlight = pb.timeOfFlight;
            float maxHeight = pb.maxHeight;
            float range = pb.range;

            Debug.Log($"Time of flight: {timeOfFlight:F2} s");
            Debug.Log($"Max height: {maxHeight:F2} m");
            Debug.Log($"Range: {range:F2} m");

            // TODO: Show results in UI panel if desired
        });
    }
    else
    {
        Debug.LogError("Projectile prefab missing ProjectileBehavior component!");
        StartCoroutine(ReenableLaunchAfter(projectileAutoDestroy + 0.1f));
    }

    // Destroy after set duration
    Destroy(projGO, projectileAutoDestroy);
}

System.Collections.IEnumerator ReenableLaunchAfter(float seconds)
{
    yield return new WaitForSeconds(seconds);
    launchButton.interactable = true;
}

}
