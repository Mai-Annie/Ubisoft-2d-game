using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    private TextMeshProUGUI timerText;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI messageText;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        BuildCanvas();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void BuildCanvas()
    {
        GameObject canvasGO = new GameObject("HUDCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        timerText = CreateTMPText(canvasGO, "TimerText",
            new Vector2(0, -30), new Vector2(300, 50),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

        scoreText = CreateTMPText(canvasGO, "ScoreText",
            new Vector2(0, -90), new Vector2(300, 50),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));

        messageText = CreateTMPText(canvasGO, "MessageText",
            new Vector2(0, 0), new Vector2(600, 80),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        messageText.fontSize = 48;
        messageText.fontStyle = FontStyles.Bold;
        messageText.gameObject.SetActive(false);
    }

    private TextMeshProUGUI CreateTMPText(GameObject parent, string name,
        Vector2 anchoredPos, Vector2 sizeDelta,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return tmp;
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        timerText.text = $"Time: {GameManager.Instance.TimeRemaining:F1}s";
        scoreText.text = $"Score: {GameManager.Instance.Score}";
    }

    public void ShowMessage(string message)
    {
        if (messageText == null) return;
        messageText.text = message;
        messageText.gameObject.SetActive(true);
    }
}
