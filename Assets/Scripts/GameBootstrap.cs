using UnityEngine;
using UnityEngine.UI;

public class GameBootstrap : MonoBehaviour
{
    private PlayerController2D player;
    private Camera mainCamera;
    private GameObject worldRoot;
    private GameObject uiRoot;
    private Text scoreText;
    private Text timerText;
    private Text comboText;
    private Text stateText;
    private GameObject pausePanel;

    private float highestY;
    private float startY;
    private float score;
    private float timer;
    private int combo;
    private bool gameOver;
    private bool paused;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (FindAnyObjectByType<GameBootstrap>(FindObjectsInactive.Include) == null)
        {
            var bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent<GameBootstrap>();
        }
    }

    private void Awake() => BuildWorld();

    private void Update()
    {
        if (player == null) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            TogglePause();

        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return))
                Restart();
            return;
        }

        if (paused) return;

        timer += Time.deltaTime;
        score = Mathf.Max(score, player.transform.position.y - startY);
        combo = Mathf.Max(0, Mathf.FloorToInt(score / 8f));

        scoreText.text = $"SCORE  {Mathf.FloorToInt(score):00000}";
        timerText.text = $"TIME  {timer:0.0}s";
        comboText.text = $"COMBO  x{combo}";

        if (player.transform.position.y > highestY - 14f)
            SpawnPlatformRing();

        if (player.transform.position.y < mainCamera.transform.position.y - 10f)
            EndGame();

        Vector3 target = mainCamera.transform.position;
        target.y = Mathf.Max(target.y, player.transform.position.y + 1.5f);
        target.x = 0f;
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, target, Time.deltaTime * 4f);
    }

    private void BuildWorld()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            mainCamera = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
        }

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 7.5f;
        mainCamera.backgroundColor = new Color(0.22f, 0.45f, 0.68f);
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);

        worldRoot = new GameObject("World");
        worldRoot.transform.SetParent(transform);

        var bg = new GameObject("Background");
        bg.transform.SetParent(worldRoot.transform);
        var bgRenderer = bg.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = ProceduralSpriteFactory.CreateSkySprite();
        bgRenderer.sortingOrder = -20;
        bg.transform.localScale = new Vector3(18f, 16f, 1f);
        bg.transform.position = new Vector3(0f, 0f, 10f);

        var ground = CreatePlatform(new Vector3(0f, -5.5f, 0f), new Vector2(22f, 1.1f), new Color32(255, 214, 128, 255), new Color32(126, 84, 64, 255));
        ground.name = "Ground";
        startY = -4.5f;
        highestY = startY;
        Random.InitState(System.Environment.TickCount);

        for (int i = 0; i < 12; i++)
        {
            float x = Random.Range(-5.5f, 5.5f);
            float y = startY + 1.5f + i * 1.5f;
            float width = Random.Range(2.2f, 4.2f);
            CreatePlatform(new Vector3(x, y, 0f), new Vector2(width, 1f), new Color32(168, 155, 205, 255), new Color32(90, 82, 136, 255));
            highestY = Mathf.Max(highestY, y);
        }

        CreatePlayer();
        CreateUi();
    }

    private void CreatePlayer()
    {
        var playerGO = new GameObject("Player");
        playerGO.transform.SetParent(worldRoot.transform);
        playerGO.transform.position = new Vector3(0f, -2.8f, 0f);
        var sr = playerGO.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSpriteFactory.CreatePlayerSprite();
        sr.sortingOrder = 10;
        var rb = playerGO.AddComponent<Rigidbody2D>();
        rb.gravityScale = 4.2f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var col = playerGO.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1.25f);
        col.offset = new Vector2(0f, -0.05f);
        player = playerGO.AddComponent<PlayerController2D>();
        player.Initialize(rb);
    }

    private GameObject CreatePlatform(Vector3 position, Vector2 size, Color fill, Color edge)
    {
        var platformGO = new GameObject("Platform");
        platformGO.transform.SetParent(worldRoot.transform);
        platformGO.transform.position = position;
        var sr = platformGO.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSpriteFactory.CreatePlatformSprite(size.x, size.y, fill, edge);
        sr.sortingOrder = 3;
        var col = platformGO.AddComponent<BoxCollider2D>();
        col.size = new Vector2(size.x, size.y);
        platformGO.layer = LayerMask.NameToLayer("Default");
        return platformGO;
    }

    private void SpawnPlatformRing()
    {
        for (int i = 0; i < 6; i++)
        {
            float x = Random.Range(-6.2f, 6.2f);
            float y = highestY + 1.6f + i * 1.35f;
            CreatePlatform(new Vector3(x, y, 0f), new Vector2(Random.Range(2.5f, 4.5f), 1f), new Color32(163, 142, 194, 255), new Color32(92, 65, 130, 255));
            highestY = Mathf.Max(highestY, y);
        }
    }

    private void CreateUi()
    {
        uiRoot = new GameObject("Canvas");
        uiRoot.transform.SetParent(transform);
        var canvas = uiRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = uiRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        uiRoot.AddComponent<GraphicRaycaster>();

        scoreText = MakeText("ScoreText", "SCORE  00000", 30, TextAnchor.UpperLeft, new Vector2(30f, -30f), new Color32(255, 245, 150, 255), new Vector2(0f, 1f));
        timerText = MakeText("TimerText", "TIME  0.0s", 30, TextAnchor.UpperRight, new Vector2(-30f, -30f), new Color32(255, 245, 150, 255), new Vector2(1f, 1f));
        comboText = MakeText("ComboText", "COMBO  x0", 26, TextAnchor.UpperCenter, new Vector2(0f, -30f), new Color32(255, 112, 180, 255), new Vector2(0.5f, 1f));
        stateText = MakeText("StateText", "", 42, TextAnchor.MiddleCenter, new Vector2(0f, 40f), Color.white, new Vector2(0.5f, 0.5f));

        pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(uiRoot.transform);
        var image = pausePanel.AddComponent<Image>();
        image.color = new Color(0.03f, 0.05f, 0.15f, 0.82f);
        var panelRect = pausePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        pausePanel.SetActive(false);
    }

    private Text MakeText(string name, string value, int size, TextAnchor alignment, Vector2 position, Color color, Vector2 anchor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(uiRoot.transform);
        var text = go.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.fontStyle = FontStyle.Bold;
        var rect = text.rectTransform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(500f, 100f);
        return text;
    }

    private void TogglePause()
    {
        if (gameOver) return;
        paused = !paused;
        Time.timeScale = paused ? 0f : 1f;
        pausePanel.SetActive(paused);
        stateText.text = paused ? "PAUSED\nPress P or Esc to continue" : "";
    }

    private void EndGame()
    {
        gameOver = true;
        stateText.text = $"GAME OVER\nScore: {Mathf.FloorToInt(score)}\nPress R to restart";
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy() => Time.timeScale = 1f;
}
