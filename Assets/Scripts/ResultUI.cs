using UnityEngine;
using TMPro;

public class ResultUI : MonoBehaviour
{
    public static ResultUI Instance;

    public GameObject panel;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI rangeText;
    public TextMeshProUGUI heightText;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Show(float time, float range, float height)
    {
        panel.SetActive(true);
        timeText.text = $"Time of Flight: {time:F2} s";
        rangeText.text = $"Range: {range:F2} m";
        heightText.text = $"Max Height: {height:F2} m";
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
