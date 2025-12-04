using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProjectileUIBinder : MonoBehaviour
{
    [Header("UI References")]
    public Slider angleSlider;
    public Slider velocitySlider;
    public TMP_Dropdown gravityDropdown;
    public Button launchButton;
    public Button closeButton;

    public TextMeshProUGUI angleLabel;
    public TextMeshProUGUI velocityLabel;
    public TextMeshProUGUI gravityLabel;

    [Header("Cannon Detection")]
    public string launcherTag = "Cannon"; // Tag assigned to cannon prefab

    private ProjectileLauncher boundLauncher;
    private bool isBound = false;

    void Update()
    {
        if (isBound) return;

        // Auto-detect spawned cannon prefab in the scene
        GameObject launcherObj = GameObject.FindWithTag(launcherTag);
        if (launcherObj != null)
        {
            ProjectileLauncher launcherComp = launcherObj.GetComponent<ProjectileLauncher>();
            if (launcherComp != null)
            {
                BindLauncherAtRuntime(launcherComp);
                isBound = true;
                Debug.Log("Projectile UI successfully bound to spawned launcher!");
            }
        }
    }

    // -----------------------------
    // Bind a launcher prefab instance at runtime
    // -----------------------------
    public void BindLauncherAtRuntime(ProjectileLauncher launcher)
{
    boundLauncher = launcher;

    if (boundLauncher == null)
    {
        Debug.LogError("ProjectileLauncher prefab instance not assigned!");
        return;
    }

    // Assign UI references
    boundLauncher.angleSlider = angleSlider;
    boundLauncher.velocitySlider = velocitySlider;
    boundLauncher.gravityDropdown = gravityDropdown;
    boundLauncher.launchButton = launchButton;

    boundLauncher.angleLabel = angleLabel;
    boundLauncher.velocityLabel = velocityLabel;
    boundLauncher.gravityLabel = gravityLabel;

    // Initialize UI
    boundLauncher.OnAngleChanged(angleSlider.value);
    boundLauncher.OnVelocityChanged(velocitySlider.value);
    boundLauncher.OnGravityChanged(gravityDropdown.value);

    // Hook main UI events
    angleSlider.onValueChanged.AddListener(boundLauncher.OnAngleChanged);
    velocitySlider.onValueChanged.AddListener(boundLauncher.OnVelocityChanged);
    gravityDropdown.onValueChanged.AddListener(boundLauncher.OnGravityChanged);
    launchButton.onClick.AddListener(boundLauncher.OnLaunchClicked);

    // Hook close button
    if (closeButton != null)
    {
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            this.gameObject.SetActive(false);
        });
    }
}
}
