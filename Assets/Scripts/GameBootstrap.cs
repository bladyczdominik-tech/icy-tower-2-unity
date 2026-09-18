using UnityEngine;
using TMPro;

public class GameBootstrap : MonoBehaviour
{
    private PlayerController2D player;
    private Camera mainCamera;
    private GameObject worldRoot;
    private GameObject uiRoot;
    private TMP_Text scoreText;
    private TMP_Text timerText;
    private TMP_Text comboText;

    private float highestY;
    private float startY;
    private float score;
    private float timer;
    private int combo;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (FindAnyObjectByType<GameBootstrap>(FindObjectsInactive.Include) == null)
        {
            var bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent<GameBootstrap>();
        }
    }

    private void Awake()
    {
        BuildWorld();
    }

    private void Update()
    {
        if (player == null)
            return;

        timer += Time.deltaTime;
        score = Mathf.Max(score, player.transform.position.y - startY);
        combo = Mathf.Max(0, Mathf.FloorToInt(score / 8f));

        if (scoreText != null)
            scoreText.text = $"Score: {Mathf.FloorToInt(score)}";

        if (timerText != null)
            timerText.text = $"Time: {timer:0.0}s";

        if (comboText != null)
            comboText.text = $"Combo x{combo}";

        if (player.transform.position.y > highestY - 12f)
        {
            SpawnPlatformRing();
        }
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
        mainCamera.backgroundColor = new Color(0.42f, 0.75f, 0.95f);
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);

        worldRoot = new GameObject("World");
        worldRoot.transform.SetParent(transform);

        var bg = new GameObject("Background");
        bg.transform.SetParent(worldRoot.transform);
        var bgRenderer = bg.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = ProceduralSpriteFactory.CreateSkySprite();
        bg.transform.localScale = new Vector3(18f, 16f, 1f);
        bg.transform.position = new Vector3(0f, 0f, 10f);

        var ground = CreatePlatform(new Vector3(0f, -5.5f, 0f), new Vector2(22f, 1.1f), new Color32(255, 214, 128, 255), new Color32(126, 84, 64, 255));
        ground.name = "Ground";
        ground.layer = LayerMask.NameToLayer("Default");

        startY = -4.5f;
        highestY = startY;

        for (int i = 0; i < 10; i++)
        {
            float x = Random.Range(-5.5f, 5.5f);
            float y = startY + 1.5f + i * 1.5f;
            float width = Random.Range(2.2f, 4.2f);
            CreatePlatform(new Vector3(x, y, 0f), new Vector2(width, 1f), new Color32(168, 155, 205, 255), new Color32(90, 82, 136, 255));
            highestY = Mathf.Max(highestY, y);
        }

        CreatePlayer();
        CreateUi();
        SpawnPlatformRing();
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

        var rb = platformGO.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        platformGO.layer = LayerMask.NameToLayer("Default");
        return platformGO;
    }

    private void SpawnPlatformRing()
    {
        for (int i = 0; i < 6; i++)
        {
            float x = Random.Range(-6.2f, 6.2f);
            float y = highestY + 1.6f + i * 1.35f;
            float width = Random.Range(2.5f, 4.5f);
            CreatePlatform(new Vector3(x, y, 0f), new Vector2(width, 1f), new Color32(163, 142, 194, 255), new Color32(92, 65, 130, 255));
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

        var scoreGO = new GameObject("ScoreText");
        scoreGO.transform.SetParent(uiRoot.transform);
        scoreText = scoreGO.AddComponent<TextMeshProUGUI>();
        scoreText.fontSize = 26;
        scoreText.color = new Color32(255, 245, 150, 255);
        scoreText.alignment = TextAlignmentOptions.Left;
        scoreText.rectTransform.anchorMin = new Vector2(0f, 1f);
        scoreText.rectTransform.anchorMax = new Vector2(0f, 1f);
        scoreText.rectTransform.pivot = new Vector2(0f, 1f);
        scoreText.rectTransform.anchoredPosition = new Vector2(30f, -30f);
        scoreText.text = "Score: 0";

        var timerGO = new GameObject("TimerText");
        timerGO.transform.SetParent(uiRoot.transform);
        timerText = timerGO.AddComponent<TextMeshProUGUI>();
        timerText.fontSize = 26;
        timerText.color = new Color32(255, 245, 150, 255);
        timerText.alignment = TextAlignmentOptions.Right;
        timerText.rectTransform.anchorMin = new Vector2(1f, 1f);
        timerText.rectTransform.anchorMax = new Vector2(1f, 1f);
        timerText.rectTransform.pivot = new Vector2(1f, 1f);
        timerText.rectTransform.anchoredPosition = new Vector2(-30f, -30f);
        timerText.text = "Time: 0.0s";

        var comboGO = new GameObject("ComboText");
        comboGO.transform.SetParent(uiRoot.transform);
        comboText = comboGO.AddComponent<TextMeshProUGUI>();
        comboText.fontSize = 22;
        comboText.color = new Color32(255, 112, 180, 255);
        comboText.alignment = TextAlignmentOptions.Center;
        comboText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        comboText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        comboText.rectTransform.pivot = new Vector2(0.5f, 1f);
        comboText.rectTransform.anchoredPosition = new Vector2(0f, -30f);
        comboText.text = "Combo x0";
    }
}
