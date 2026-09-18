using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    private Camera cam;
    private GameObject world;
    private GameObject menuCanvas;
    private GameObject hudCanvas;
    private GameObject pausePanel;
    private PlayerController2D player;
    private Text scoreText;
    private Text timerText;
    private Text comboText;
    private Text selectionText;
    private Text stateText;

    private float highestY;
    private float startY;
    private float score;
    private float timer;
    private int combo;
    private int characterIndex;
    private int towerIndex;
    private bool started;
    private bool paused;
    private bool gameOver;

    private readonly Color[] towerColors =
    {
        new Color32(168, 155, 205, 255),
        new Color32(205, 180, 110, 255),
        new Color32(115, 190, 170, 255)
    };

    private readonly Color[] characterColors =
    {
        new Color32(35, 105, 240, 255),
        new Color32(55, 190, 70, 255),
        new Color32(230, 70, 65, 255)
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (FindAnyObjectByType<GameBootstrap>() == null)
        {
            new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
        }
    }

    private void Awake()
    {
        cam = Camera.main;
        if (cam == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cam = cameraObject.AddComponent<Camera>();
        }

        cam.orthographic = true;
        cam.orthographicSize = 7.5f;
        cam.transform.position = new Vector3(0f, 0f, -10f);

        BuildWorld();
        BuildHud();
        BuildMenu();
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (!started || player == null)
            return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            TogglePause();

        if (paused)
            return;

        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
                Restart();
            return;
        }

        timer += Time.deltaTime;
        score = Mathf.Max(score, player.transform.position.y - startY);
        combo = Mathf.FloorToInt(score / 8f);

        scoreText.text = $"SCORE  {Mathf.FloorToInt(score):00000}";
        timerText.text = $"TIME  {timer:0.0}s";
        comboText.text = $"COMBO  x{combo}";

        if (player.transform.position.y > highestY - 13f)
            SpawnPlatforms();

        if (player.transform.position.y < cam.transform.position.y - 10f)
            EndGame();

        Vector3 target = new Vector3(
            0f,
            Mathf.Max(cam.transform.position.y, player.transform.position.y + 1.5f),
            -10f);
        cam.transform.position = Vector3.Lerp(cam.transform.position, target, Time.deltaTime * 4f);
    }

    private void BuildWorld()
    {
        world = new GameObject("TowerWorld");

        GameObject sky = new GameObject("Sky");
        sky.transform.SetParent(world.transform);
        SpriteRenderer skyRenderer = sky.AddComponent<SpriteRenderer>();
        skyRenderer.sprite = ProceduralSpriteFactory.CreateSkySprite();
        skyRenderer.sortingOrder = -20;
        sky.transform.localScale = new Vector3(18f, 16f, 1f);
        sky.transform.position = new Vector3(0f, 0f, 10f);

        CreatePlatform(
            new Vector3(0f, -5.5f, 0f),
            new Vector2(22f, 1.1f),
            new Color32(255, 214, 128, 255),
            new Color32(126, 84, 64, 255));

        startY = -4.5f;
        highestY = startY;
        Random.InitState(System.Environment.TickCount);

        for (int i = 0; i < 13; i++)
        {
            float y = startY + 1.5f + i * 1.5f;
            CreatePlatform(
                new Vector3(Random.Range(-5.2f, 5.2f), y, 0f),
                new Vector2(Random.Range(2.5f, 4.5f), 1f),
                towerColors[towerIndex],
                new Color32(70, 55, 110, 255));
            highestY = y;
        }

        CreatePlayer();
    }

    private void CreatePlayer()
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.transform.SetParent(world.transform);
        playerObject.transform.position = new Vector3(0f, -2.8f, 0f);

        SpriteRenderer renderer = playerObject.AddComponent<SpriteRenderer>();
        renderer.sprite = ProceduralSpriteFactory.CreatePlayerSprite();
        renderer.color = characterColors[characterIndex];
        renderer.sortingOrder = 10;

        Rigidbody2D body = playerObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 4.2f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        BoxCollider2D collider = playerObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1f, 1.25f);
        collider.offset = new Vector2(0f, -0.05f);

        player = playerObject.AddComponent<PlayerController2D>();
        player.Initialize(body);
        player.SetAudioProfile(characterIndex);
    }

    private GameObject CreatePlatform(Vector3 position, Vector2 size, Color fill, Color edge)
    {
        GameObject platform = new GameObject("TowerPlatform");
        platform.transform.SetParent(world.transform);
        platform.transform.position = position;

        SpriteRenderer renderer = platform.AddComponent<SpriteRenderer>();
        renderer.sprite = ProceduralSpriteFactory.CreatePlatformSprite(size.x, size.y, fill, edge);
        renderer.sortingOrder = 3;

        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        collider.size = size;
        platform.layer = LayerMask.NameToLayer("Default");
        return platform;
    }

    private void SpawnPlatforms()
    {
        for (int i = 0; i < 6; i++)
        {
            float y = highestY + 1.45f + i * 1.35f;
            CreatePlatform(
                new Vector3(Random.Range(-6.1f, 6.1f), y, 0f),
                new Vector2(Random.Range(2.5f, 4.5f), 1f),
                towerColors[towerIndex],
                new Color32(70, 55, 110, 255));
            highestY = y;
        }
    }

    private void BuildMenu()
    {
        menuCanvas = MakeCanvas("MainMenu", 100);
        Image root = MakeImage(menuCanvas.transform, "Background", new Color(0.08f, 0.28f, 0.58f, 0.96f));
        Stretch(root.rectTransform);

        AddText(root.transform, "Title", "ICY\nTOWER 2", 78, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.78f), Vector2.zero, new Vector2(650f, 220f));
        AddText(root.transform, "Subtitle", "WIECZNA WIEŻA", 22, new Color32(190, 240, 255, 255), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.64f), Vector2.zero, new Vector2(500f, 50f));

        MakeButton(root.transform, "GRAJ", new Vector2(0.5f, 0.51f), new Color32(246, 195, 55, 255), StartGame);
        MakeButton(root.transform, "POSTAĆ: TOM", new Vector2(0.5f, 0.40f), new Color32(45, 130, 220, 255), () => SelectCharacter(0));
        MakeButton(root.transform, "POSTAĆ: LUNA", new Vector2(0.5f, 0.30f), new Color32(45, 130, 220, 255), () => SelectCharacter(1));
        MakeButton(root.transform, "POSTAĆ: REX", new Vector2(0.5f, 0.20f), new Color32(45, 130, 220, 255), () => SelectCharacter(2));
        MakeButton(root.transform, "WIEŻA: CLASSIC", new Vector2(1f, 0.58f), new Color32(45, 100, 175, 255), () => SelectTower(0));
        MakeButton(root.transform, "WIEŻA: RUINY", new Vector2(1f, 0.46f), new Color32(45, 100, 175, 255), () => SelectTower(1));
        MakeButton(root.transform, "WIEŻA: OGRÓD", new Vector2(1f, 0.34f), new Color32(45, 100, 175, 255), () => SelectTower(2));

        selectionText = AddText(root.transform, "Selection", "", 20, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.08f), Vector2.zero, new Vector2(700f, 50f));
        RefreshSelection();
    }

    private void BuildHud()
    {
        hudCanvas = MakeCanvas("HUD", 110);
        hudCanvas.SetActive(false);

        scoreText = AddText(hudCanvas.transform, "Score", "SCORE 00000", 28, new Color32(255, 245, 150, 255), TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(500f, 80f));
        timerText = AddText(hudCanvas.transform, "Timer", "TIME 0.0s", 28, new Color32(255, 245, 150, 255), TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(500f, 80f));
        comboText = AddText(hudCanvas.transform, "Combo", "COMBO x0", 25, new Color32(255, 112, 180, 255), TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(500f, 80f));
        stateText = AddText(hudCanvas.transform, "State", "", 38, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(700f, 180f));

        pausePanel = MakeImage(hudCanvas.transform, "PausePanel", new Color(0.02f, 0.04f, 0.15f, 0.85f)).gameObject;
        Stretch(pausePanel.GetComponent<RectTransform>());
        pausePanel.SetActive(false);
    }

    private void StartGame()
    {
        started = true;
        Time.timeScale = 1f;
        menuCanvas.SetActive(false);
        hudCanvas.SetActive(true);
        AudioFactory.PlayStart();
    }

    private void SelectCharacter(int index)
    {
        characterIndex = index;
        if (player != null)
            player.GetComponent<SpriteRenderer>().color = characterColors[index];
        RefreshSelection();
    }

    private void SelectTower(int index)
    {
        towerIndex = index;
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        if (selectionText == null)
            return;

        string character = characterIndex == 0 ? "TOM" : characterIndex == 1 ? "LUNA" : "REX";
        string tower = towerIndex == 0 ? "CLASSIC" : towerIndex == 1 ? "RUINY" : "OGRÓD";
        selectionText.text = $"POSTAĆ: {character}    |    WIEŻA: {tower}";
    }

    private void TogglePause()
    {
        if (gameOver)
            return;

        paused = !paused;
        Time.timeScale = paused ? 0f : 1f;
        pausePanel.SetActive(paused);
        stateText.text = paused ? "PAUZA\nP / ESC - kontynuuj" : "";
    }

    private void EndGame()
    {
        gameOver = true;
        stateText.text = $"KONIEC GRY\nWynik: {Mathf.FloorToInt(score)}\nR - restart";
        AudioFactory.PlayGameOver();
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private GameObject MakeCanvas(string name, int order)
    {
        GameObject canvasObject = new GameObject(name);
        canvasObject.transform.SetParent(transform);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = order;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvasObject;
    }

    private Image MakeImage(Transform parent, string name, Color color)
    {
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(parent);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private Text AddText(Transform parent, string name, string value, int size, Color color, TextAnchor alignment, Vector2 anchor, Vector2 position, Vector2 sizeDelta)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent);
        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = size;
        text.fontStyle = FontStyle.Bold;
        text.color = color;
        text.alignment = alignment;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = sizeDelta;
        return text;
    }

    private void MakeButton(Transform parent, string label, Vector2 anchor, Color color, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(label + "Button");
        buttonObject.transform.SetParent(parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = color;
        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(action);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchor.x > 0.75f ? new Vector2(-250f, 0f) : new Vector2(-250f, 0f);
        rect.sizeDelta = new Vector2(480f, 75f);
        AddText(buttonObject.transform, "Label", label, 25, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(470f, 70f));
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
