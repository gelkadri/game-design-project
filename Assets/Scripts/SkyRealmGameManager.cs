using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class SkyRealmGameManager : MonoBehaviour
{
    public static SkyRealmGameManager instance;


    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text timerText;

    [FormerlySerializedAs("playerController")]
    [SerializeField] private SkyExplorerController skyExplorerController;

    private int coinCount = 0;
    private int gemCount = 0;
    private bool isGameOver = false;
    private bool levelComplete = false;
    private bool levelExiting = false;
    private Vector3 playerPosition;

    //Level Complete

    [SerializeField] GameObject levelCompletePanel;
    [SerializeField] TMP_Text leveCompletePanelTitle;
    [SerializeField] TMP_Text levelCompleteCoins;
    [SerializeField] TMP_Text levelCompleteTime;

    private int totalCoins = 0;
    [SerializeField] private float levelTime = 15f;
    private float remainingTime;
    private Image timerBgImage;

    // Game Over
    [SerializeField] private GameObject gameOverPanel;
    private TMP_Text gameOverTitle;
    private TMP_Text gameOverMessage;

    [Header("Audio")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip rewardSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip levelCompleteSound;
    [SerializeField] private float backgroundMusicVolume = 1f;
    [SerializeField] private float jumpSoundVolume = 1f;
    [SerializeField] private float rewardSoundVolume = 1f;
    [SerializeField] private float gameOverSoundVolume = 1f;
    [SerializeField] private float levelCompleteSoundVolume = 1f;

    private AudioSource musicAudioSource;
    private AudioSource sfxAudioSource;
    private bool hasPlayedGameOverSound;



    private void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        levelTime = 20f;
        SetupAudioSources();
        TryLoadAudioFromResources();
        PlayBackgroundMusic();

        if (SkyHeartManager.instance == null)
            gameObject.AddComponent<SkyHeartManager>();

        Canvas canvas = coinText != null ? coinText.GetComponentInParent<Canvas>() : FindObjectOfType<Canvas>();
        if (canvas != null && canvas.GetComponent<SkyRealmPauseMenu>() == null)
            canvas.gameObject.AddComponent<SkyRealmPauseMenu>();

        if (timerText == null)
        {
            CreateTimerUI();
        }

        if (levelCompleteTime == null && levelCompletePanel != null)
        {
            CreateLevelCompleteTimeUI();
        }

        remainingTime = levelTime;

        if (gameOverPanel == null)
        {
            CreateGameOverPanel();
        }

        UpdateGUI();
        SkyRealmUIManager.instance.fadeFromBlack = true;
        playerPosition = skyExplorerController.transform.position;

        FindTotalPickups();
    }

    private void SetupAudioSources()
    {
        musicAudioSource = gameObject.AddComponent<AudioSource>();
        musicAudioSource.loop = true;
        musicAudioSource.playOnAwake = false;
        musicAudioSource.spatialBlend = 0f;
        musicAudioSource.ignoreListenerPause = true;
        musicAudioSource.volume = backgroundMusicVolume;

        sfxAudioSource = gameObject.AddComponent<AudioSource>();
        sfxAudioSource.loop = false;
        sfxAudioSource.playOnAwake = false;
    }

    private void TryLoadAudioFromResources()
    {
        if (backgroundMusic == null)
            backgroundMusic = Resources.Load<AudioClip>("Audio/background");
        if (jumpSound == null)
            jumpSound = Resources.Load<AudioClip>("Audio/jump");
        if (rewardSound == null)
            rewardSound = Resources.Load<AudioClip>("Audio/reward");
        if (gameOverSound == null)
            gameOverSound = Resources.Load<AudioClip>("Audio/gameover");
        if (levelCompleteSound == null)
            levelCompleteSound = Resources.Load<AudioClip>("Audio/levelcomplete");
    }

    private void PlayBackgroundMusic()
    {
        if (musicAudioSource == null || backgroundMusic == null) return;
        musicAudioSource.clip = backgroundMusic;
        musicAudioSource.volume = backgroundMusicVolume;
        if (!musicAudioSource.isPlaying)
            musicAudioSource.Play();
    }

    public void PlayJumpSound()
    {
        if (sfxAudioSource == null || jumpSound == null) return;
        sfxAudioSource.PlayOneShot(jumpSound, jumpSoundVolume);
    }

    public void PlayRewardSound()
    {
        if (sfxAudioSource == null || rewardSound == null) return;
        sfxAudioSource.PlayOneShot(rewardSound, rewardSoundVolume);
    }

    private void PlayLevelCompleteSound()
    {
        if (sfxAudioSource == null || levelCompleteSound == null) return;
        sfxAudioSource.PlayOneShot(levelCompleteSound, levelCompleteSoundVolume);
    }

    private void HandleGameOverAudio()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
            musicAudioSource.Stop();

        if (!hasPlayedGameOverSound && sfxAudioSource != null && gameOverSound != null)
        {
            sfxAudioSource.PlayOneShot(gameOverSound, gameOverSoundVolume);
            hasPlayedGameOverSound = true;
        }
    }

    private void CreateGameOverPanel()
    {
        Canvas canvas = coinText.GetComponentInParent<Canvas>();

        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRt = gameOverPanel.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        Image panelBg = gameOverPanel.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.85f);

        // Title
        GameObject titleObj = new GameObject("GameOverTitle");
        titleObj.transform.SetParent(gameOverPanel.transform, false);
        gameOverTitle = titleObj.AddComponent<TextMeshProUGUI>();
        gameOverTitle.text = "GAME OVER";
        gameOverTitle.fontSize = 72;
        gameOverTitle.font = coinText.font;
        gameOverTitle.color = Color.red;
        gameOverTitle.alignment = TextAlignmentOptions.Center;
        gameOverTitle.fontStyle = FontStyles.Bold;

        RectTransform titleRt = gameOverTitle.rectTransform;
        titleRt.anchorMin = new Vector2(0.5f, 0.5f);
        titleRt.anchorMax = new Vector2(0.5f, 0.5f);
        titleRt.pivot = new Vector2(0.5f, 0.5f);
        titleRt.anchoredPosition = new Vector2(0, 80);
        titleRt.sizeDelta = new Vector2(600, 100);

        // Message
        GameObject msgObj = new GameObject("GameOverMessage");
        msgObj.transform.SetParent(gameOverPanel.transform, false);
        gameOverMessage = msgObj.AddComponent<TextMeshProUGUI>();
        gameOverMessage.text = "TIME'S UP!";
        gameOverMessage.fontSize = 36;
        gameOverMessage.font = coinText.font;
        gameOverMessage.color = Color.white;
        gameOverMessage.alignment = TextAlignmentOptions.Center;

        RectTransform msgRt = gameOverMessage.rectTransform;
        msgRt.anchorMin = new Vector2(0.5f, 0.5f);
        msgRt.anchorMax = new Vector2(0.5f, 0.5f);
        msgRt.pivot = new Vector2(0.5f, 0.5f);
        msgRt.anchoredPosition = new Vector2(0, 10);
        msgRt.sizeDelta = new Vector2(600, 50);

        CreateGameOverButton("RetryButton", "RETRY", new Vector2(-100, -60), () => {
            SkyHeartManager.ResetHealth();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });

        CreateGameOverButton("MenuButton", "MENU", new Vector2(100, -60), () => {
            SkyHeartManager.ResetHealth();
            SceneManager.LoadScene(0);
        });

        gameOverPanel.SetActive(false);
    }

    private void CreateGameOverButton(string name, string label, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(gameOverPanel.transform, false);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.anchoredPosition = position;
        btnRt.sizeDelta = new Vector2(160, 50);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        colors.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        btn.colors = colors;
        btn.onClick.AddListener(action);

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI btnText = txtObj.AddComponent<TextMeshProUGUI>();
        btnText.text = label;
        btnText.fontSize = 28;
        btnText.font = coinText.font;
        btnText.color = Color.white;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.fontStyle = FontStyles.Bold;

        RectTransform txtRt = btnText.rectTransform;
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;
    }

    private void CreateTimerUI()
    {
        Canvas canvas = coinText.GetComponentInParent<Canvas>();

        GameObject bgObj = new GameObject("TimerBackground");
        bgObj.transform.SetParent(canvas.transform, false);
        timerBgImage = bgObj.AddComponent<Image>();
        timerBgImage.color = new Color(0f, 0f, 0f, 0.45f);

        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0.5f, 1f);
        bgRt.anchorMax = new Vector2(0.5f, 1f);
        bgRt.pivot = new Vector2(0.5f, 1f);
        bgRt.anchoredPosition = new Vector2(0, -10);
        bgRt.sizeDelta = new Vector2(220, 55);

        GameObject timerObj = new GameObject("TimerText");
        timerObj.transform.SetParent(bgObj.transform, false);

        timerText = timerObj.AddComponent<TextMeshProUGUI>();
        timerText.text = "00:15";
        timerText.fontSize = 42;
        timerText.font = coinText.font;
        timerText.color = Color.white;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.fontStyle = FontStyles.Bold;

        RectTransform rt = timerText.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void CreateLevelCompleteTimeUI()
    {
        GameObject timeObj = new GameObject("LevelCompleteTime");
        timeObj.transform.SetParent(levelCompleteCoins.transform.parent, false);

        levelCompleteTime = timeObj.AddComponent<TextMeshProUGUI>();
        levelCompleteTime.text = "";
        levelCompleteTime.fontSize = levelCompleteCoins.fontSize;
        levelCompleteTime.font = levelCompleteCoins.font;
        levelCompleteTime.color = levelCompleteCoins.color;
        levelCompleteTime.alignment = levelCompleteCoins.alignment;

        RectTransform rt = levelCompleteTime.rectTransform;
        RectTransform coinsRt = levelCompleteCoins.rectTransform;
        rt.anchorMin = coinsRt.anchorMin;
        rt.anchorMax = coinsRt.anchorMax;
        rt.pivot = coinsRt.pivot;
        rt.sizeDelta = coinsRt.sizeDelta;
        rt.anchoredPosition = coinsRt.anchoredPosition + new Vector2(0, -40);
    }

    public void IncrementCoinCount()
    {
        coinCount++;

        if (coinCount % 3 == 0 && SkyHeartManager.instance != null)
            SkyHeartManager.instance.HealPlayer();

        UpdateGUI();
    }
    public void IncrementGemCount()
    {
        gemCount++;
        UpdateGUI();
    }

    private void Update()
    {
        if (!isGameOver && !levelComplete && !levelExiting)
        {
            PlayBackgroundMusic();
        }

        if (!isGameOver && !levelComplete)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                UpdateTimerDisplay();
                TimeUp();
                return;
            }

            UpdateTimerDisplay();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (remainingTime <= 5f)
        {
            timerText.color = Color.red;
            if (timerBgImage != null)
                timerBgImage.color = new Color(0.3f, 0f, 0f, 0.6f);
        }
        else
        {
            timerText.color = Color.white;
            if (timerBgImage != null)
                timerBgImage.color = new Color(0f, 0f, 0f, 0.45f);
        }
    }

    private void UpdateGUI()
    {
        coinText.text = coinCount.ToString();
    }

    public void SetLevelExiting()
    {
        levelExiting = true;
    }

    /// <summary>Hazards like falling asteroids should only run while the level is actively playable.</summary>
    public bool IsLevelPlayable => !isGameOver && !levelComplete && !levelExiting;

    private void TimeUp()
    {
        if (isGameOver || levelComplete || levelExiting) return;
        isGameOver = true;
        HandleGameOverAudio();

        SkyRealmUIManager.instance.DisableMobileControls();
        SkyRealmUIManager.instance.fadeToBlack = true;
        skyExplorerController.gameObject.SetActive(false);

        if (SkyHeartManager.instance != null)
        {
            SkyHeartManager.instance.LoseHeart();

            if (SkyHeartManager.instance.IsAlive())
            {
                StartCoroutine(RestartLevel());
                return;
            }
        }

        StartCoroutine(ShowGameOver());
    }

    private IEnumerator ShowGameOver()
    {
        SkyRealmUIManager.instance.fadeToBlack = true;
        yield return new WaitForSeconds(1.5f);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        SkyRealmUIManager.instance.fadeFromBlack = true;
    }

    public void Death()
    {
        if (isGameOver || levelComplete || levelExiting) return;
        isGameOver = true;
        HandleGameOverAudio();

        SkyRealmUIManager.instance.DisableMobileControls();
        SkyRealmUIManager.instance.fadeToBlack = true;
        skyExplorerController.gameObject.SetActive(false);

        if (SkyHeartManager.instance != null)
        {
            SkyHeartManager.instance.LoseHeart();

            if (SkyHeartManager.instance.IsAlive())
            {
                StartCoroutine(RestartLevel());
                return;
            }
        }

        StartCoroutine(ShowGameOver());
    }
 
    public void FindTotalPickups()
    {

        SkyCollectible[] collectibles = GameObject.FindObjectsOfType<SkyCollectible>();

        foreach (SkyCollectible collectible in collectibles)
        {
            if (collectible.collectibleKind == SkyCollectible.CollectibleKind.coin)
            {
                totalCoins += 1;
            }
           
        }


      
    }
    public void LevelComplete()
    {
        levelComplete = true;
        if (musicAudioSource != null && musicAudioSource.isPlaying)
            musicAudioSource.Stop();
        PlayLevelCompleteSound();
        SkyHeartManager.ResetHealth();

        float timeTaken = levelTime - remainingTime;
        int minutes = Mathf.FloorToInt(timeTaken / 60f);
        int seconds = Mathf.FloorToInt(timeTaken % 60f);

        levelCompletePanel.SetActive(true);
        // Build order: 0 Menu, 1 Level, 2 Level 2
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (sceneIndex == 1)
            leveCompletePanelTitle.text = "LEVEL COMPLETE\n<size=40>Beginning the next level...</size>";
        else if (sceneIndex == 2)
        {
            leveCompletePanelTitle.text = "GAME COMPLETE";
            EnsurePlayAgainButtonOnLevelCompletePanel();
        }
        else
            leveCompletePanelTitle.text = "LEVEL COMPLETE";

        if (levelCompleteCoins != null)
            levelCompleteCoins.text = "COINS COLLECTED: " + coinCount.ToString() + " / " + totalCoins.ToString();
        if (levelCompleteTime != null)
            levelCompleteTime.text = "TIME: " + string.Format("{0:00}:{1:00}", minutes, seconds) + " / " + string.Format("{0:00}:{1:00}", Mathf.FloorToInt(levelTime / 60f), Mathf.FloorToInt(levelTime % 60f));

        ApplyLevelExitPanelLayout(sceneIndex);

        SkyRealmUIManager.instance.fadeFromBlack = true;

        StartCoroutine(AdvanceAfterLevelCompleteIfNeeded());
    }

    /// <summary>
    /// Keep stats vertically centered as a block (not hugging the top); stack buttons from the bottom so nothing overlaps.
    /// </summary>
    private void ApplyLevelExitPanelLayout(int sceneIndex)
    {
        if (levelCompletePanel == null)
            return;
        Transform panel = levelCompletePanel.transform;

        if (leveCompletePanelTitle != null)
        {
            RectTransform rt = leveCompletePanelTitle.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 118f);
            rt.sizeDelta = new Vector2(1100f, 200f);
        }

        if (levelCompleteCoins != null)
        {
            RectTransform rt = levelCompleteCoins.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 18f);
            rt.sizeDelta = new Vector2(1000f, 80f);
            levelCompleteCoins.enableAutoSizing = false;
            levelCompleteCoins.fontSize = 40;
            levelCompleteCoins.raycastTarget = false;
        }

        if (levelCompleteTime != null)
        {
            RectTransform rt = levelCompleteTime.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, -52f);
            rt.sizeDelta = new Vector2(1000f, 64f);
            levelCompleteTime.enableAutoSizing = false;
            levelCompleteTime.fontSize = 34;
            levelCompleteTime.raycastTarget = false;
        }

        if (sceneIndex == 2)
        {
            SetBottomAnchoredButtonY(panel, "PlayAgainButton", 95f);
            SetBottomAnchoredButtonY(panel, "QuitButton", 205f);
            SetBottomAnchoredButtonY(panel, "MenuButton", 318f);
            StylePlayAgainButtonLabel(panel);
        }
        else
        {
            SetBottomAnchoredButtonY(panel, "QuitButton", 155f);
            SetBottomAnchoredButtonY(panel, "MenuButton", 275f);
        }
    }

    private static void StylePlayAgainButtonLabel(Transform panel)
    {
        Transform playAgain = panel.Find("PlayAgainButton");
        if (playAgain == null)
            return;
        TextMeshProUGUI tmp = playAgain.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp == null)
            return;
        tmp.fontSize = 60;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(0.19607843f, 0.19607843f, 0.19607843f, 1f);
    }

    private static void SetBottomAnchoredButtonY(Transform panel, string childName, float yFromBottom)
    {
        Transform t = panel.Find(childName);
        if (t == null)
            return;
        if (t is not RectTransform rt)
            return;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, yFromBottom);
    }

    private void EnsurePlayAgainButtonOnLevelCompletePanel()
    {
        if (levelCompletePanel == null)
            return;
        Transform panel = levelCompletePanel.transform;
        if (panel.Find("PlayAgainButton") != null)
            return;

        TMP_Text styleFrom = levelCompleteCoins != null ? levelCompleteCoins : leveCompletePanelTitle;

        GameObject btnObj = new GameObject("PlayAgainButton");
        btnObj.transform.SetParent(panel, false);

        RectTransform btnRt = btnObj.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0f);
        btnRt.anchorMax = new Vector2(0.5f, 0f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.anchoredPosition = new Vector2(0, 95f);
        btnRt.sizeDelta = new Vector2(450, 90);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = Color.white;
        Transform menuRef = panel.Find("MenuButton");
        if (menuRef != null)
        {
            Image refImg = menuRef.GetComponent<Image>();
            if (refImg != null && refImg.sprite != null)
            {
                btnImage.sprite = refImg.sprite;
                btnImage.type = refImg.type;
            }
        }

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
        colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
        btn.colors = colors;

        Events events = FindFirstObjectByType<Events>();
        if (events != null)
            btn.onClick.AddListener(events.PlayAgain);
        else
            btn.onClick.AddListener(() =>
            {
                SkyHeartManager.ResetHealth();
                SceneManager.LoadScene(1);
            });

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "PLAY AGAIN";
        btnText.fontSize = 60;
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        if (styleFrom != null)
            btnText.font = styleFrom.font;
        btnText.color = new Color(0.19607843f, 0.19607843f, 0.19607843f, 1f);
        btnText.raycastTarget = false;

        RectTransform textRt = btnText.rectTransform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Build order: 0 Menu, 1 Level, 2 Level 2. After Level 1, load Level 2 automatically.
    /// </summary>
    private IEnumerator AdvanceAfterLevelCompleteIfNeeded()
    {
        yield return new WaitForSeconds(3f);
        if (!levelComplete)
            yield break;
        int index = SceneManager.GetActiveScene().buildIndex;
        if (index == 1)
            SceneManager.LoadScene(2);
    }
   
    private IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
