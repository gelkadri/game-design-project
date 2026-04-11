using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// In-game pause overlay (ESC). Freezes gameplay via Time.timeScale and SkyExplorerController.isPaused.
/// </summary>
public class SkyRealmPauseMenu : MonoBehaviour
{
    [SerializeField] private SkyExplorerController explorer;
    [SerializeField] private SkyRealmUIManager uiManager;

    private Canvas rootCanvas;
    private GameObject pauseRoot;
    private bool paused;
    private GameObject levelExitPanel;

    private void Awake()
    {
        if (explorer == null)
            explorer = FindFirstObjectByType<SkyExplorerController>();
        if (uiManager == null)
            uiManager = SkyRealmUIManager.instance;
        rootCanvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        levelExitPanel = GameObject.Find("LevelExitPanel");
        BuildPauseUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (levelExitPanel != null && levelExitPanel.activeSelf)
                return;
            TogglePause();
        }
    }

    private void BuildPauseUI()
    {
        if (rootCanvas == null) return;

        pauseRoot = new GameObject("PauseMenu");
        pauseRoot.transform.SetParent(transform, false);
        var rt = pauseRoot.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var dim = new GameObject("Dim");
        dim.transform.SetParent(pauseRoot.transform, false);
        var dimRt = dim.AddComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = Vector2.zero;
        dimRt.offsetMax = Vector2.zero;
        var dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0.12f, 0.04f, 0.1f, 0.82f);
        dimImg.raycastTarget = true;

        var panel = new GameObject("Panel");
        panel.transform.SetParent(pauseRoot.transform, false);
        var panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(420, 360);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.98f, 0.72f, 0.88f, 0.98f);

        var title = CreateTmp("Title", panel.transform, "PAUSED", 42, FontStyles.Bold,
            new Color(0.35f, 0.08f, 0.22f, 1f), new Vector2(0, 110), new Vector2(380, 56));

        CreateButton(panel.transform, "Resume", "RESUME", new Vector2(0, 30), () => SetPaused(false));
        CreateButton(panel.transform, "Restart", "RESTART", new Vector2(0, -50), RestartLevel);
        CreateButton(panel.transform, "MainMenu", "MAIN MENU", new Vector2(0, -130), GoMainMenu);

        pauseRoot.SetActive(false);
    }

    private TextMeshProUGUI CreateTmp(string name, Transform parent, string text, float size, FontStyles style, Color c, Vector2 anchored, Vector2 sizeDelta)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchored;
        rt.sizeDelta = sizeDelta;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = c;
        tmp.alignment = TextAlignmentOptions.Center;
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
        return tmp;
    }

    private void CreateButton(Transform parent, string name, string label, Vector2 anchored, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchored;
        rt.sizeDelta = new Vector2(280, 52);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.92f, 0.38f, 0.58f, 1f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.highlightedColor = new Color(1f, 0.55f, 0.72f, 1f);
        colors.pressedColor = new Color(0.75f, 0.22f, 0.45f, 1f);
        btn.colors = colors;
        btn.onClick.AddListener(onClick);

        var tgo = new GameObject("Label");
        tgo.transform.SetParent(go.transform, false);
        var trt = tgo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        var tmp = tgo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 26;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
    }

    public void TogglePause()
    {
        SetPaused(!paused);
    }

    public void SetPaused(bool value)
    {
        paused = value;
        if (pauseRoot != null)
            pauseRoot.SetActive(paused);

        Time.timeScale = paused ? 0f : 1f;

        if (explorer != null)
            explorer.isPaused = paused;

        if (uiManager != null)
        {
            if (paused)
                uiManager.DisableMobileControls();
            else if (explorer != null && explorer.travelMode == SkyTravelMode.mobile)
                uiManager.EnableMobileControls();
        }
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f;
        paused = false;
        if (explorer != null)
            explorer.isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void GoMainMenu()
    {
        Time.timeScale = 1f;
        paused = false;
        SceneManager.LoadScene(0);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
