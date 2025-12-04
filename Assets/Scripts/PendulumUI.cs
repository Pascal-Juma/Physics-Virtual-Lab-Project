using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PendulumUI : MonoBehaviour
{
    [Header("References")]
    public Pendulum3D pendulum;
    public Slider angleSlider;
    public Slider lengthSlider;
    public Button playButton;
    public Button stopButton;
    public Button resetButton;

    public TextMeshProUGUI oscillationsText;
    public TextMeshProUGUI timerText;

    void Start()
    {
        // UI changes apply only if pendulum exists
        angleSlider.onValueChanged.AddListener(val =>
        {
            if (pendulum != null)
                pendulum.Configure(val, lengthSlider.value);
        });

        lengthSlider.onValueChanged.AddListener(val =>
        {
            if (pendulum != null)
                pendulum.Configure(angleSlider.value, val);
        });

        playButton.onClick.AddListener(() =>
        {
            if (pendulum != null)
                pendulum.Play();
        });

        stopButton.onClick.AddListener(() =>
        {
            if (pendulum != null)
                pendulum.Stop();
        });

        resetButton.onClick.AddListener(() =>
        {
            if (pendulum != null)
            {
                pendulum.Reset();
                UpdateUI();
            }
        });

        UpdateUI();
    }

    void Update()
    {
        if (pendulum != null && pendulum.isRunning)
            UpdateUI();
    }

    // Called automatically when prefab is placed
    public void SetPendulum(Pendulum3D newPendulum)
    {
        pendulum = newPendulum;

        if (pendulum == null) return;

        pendulum.Configure(angleSlider.value, lengthSlider.value);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (pendulum == null) return;

        oscillationsText.text = $"Oscillation Count: {pendulum.oscillationCount}";

        int minutes = Mathf.FloorToInt(pendulum.timer / 60f);
        int seconds = Mathf.FloorToInt(pendulum.timer % 60f);

        timerText.text = $"Time Count: {minutes:00}:{seconds:00}";
    }
}
