using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    private GameObject menu;
    private Canvas canvas;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateMenu()
    {
        var existing = FindAnyObjectByType<MainMenuUI>(FindObjectsInactive.Include);
        if (existing != null) return;

        var go = new GameObject("MainMenuUI");
        go.AddComponent<MainMenuUI>();
    }

    private void Awake()
    {
        BuildMenu();
    }

    private void BuildMenu()
    {
        var canvasObject = new GameObject("MainMenuCanvas");
        canvasObject.transform.SetParent(transform);
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();

        menu = new GameObject("MenuPanel");
        menu.transform.SetParent(canvasObject.transform);
        var panel = menu.AddComponent<Image>();
        panel.color = new Color(0.08f, 0.25f, 0.55f, 0.97f);
        Stretch(menu.GetComponent<RectTransform>());

        AddText(menu.transform, "ICY\nTOWER 2", 86, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.78f), new Vector2(0f, 0f), new Vector2(700f, 220f));
        AddText(menu.transform, "WIECZNA WIEŻA", 24, new Color32(185, 240, 255, 255), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.64f), Vector2.zero, new Vector2(500f, 60f));

        CreateButton(menu.transform, "GRAJ", new Vector2(0.5f, 0.52f), new Color32(245, 196, 58, 255), StartGame);
        CreateButton(menu.transform, "POSTACIE", new Vector2(0.5f, 0.40f), new Color32(47, 126, 215, 255), ShowInfo);
        CreateButton(menu.transform, "MAPY", new Vector2(0.5f, 0.28f), new Color32(47, 126, 215, 255), ShowInfo);
        CreateButton(menu.transform, "USTAWIENIA", new Vector2(0.5f, 0.16f), new Color32(47, 126, 215, 255), ShowInfo);

        var card = new GameObject("CharacterCard");
        card.transform.SetParent(menu.transform);
        var cardImage = card.AddComponent<Image>();
        cardImage.color = new Color32(20, 66, 125, 255);
        var cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(1f, 0.5f);
        cardRect.anchorMax = new Vector2(1f, 0.5f);
        cardRect.pivot = new Vector2(1f, 0.5f);
        cardRect.anchoredPosition = new Vector2(-80f, 0f);
        cardRect.sizeDelta = new Vector2(360f, 470f);

        AddText(card.transform, "WYBRANA POSTAĆ", 25, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.87f), Vector2.zero, new Vector2(330f, 60f));
        var avatar = new GameObject("Avatar");
        avatar.transform.SetParent(card.transform);
        var avatarImage = avatar.AddComponent<Image>();
        avatarImage.color = new Color32(50, 190, 245, 255);
        var avatarRect = avatar.GetComponent<RectTransform>();
        avatarRect.anchorMin = new Vector2(0.5f, 0.5f);
        avatarRect.anchorMax = new Vector2(0.5f, 0.5f);
        avatarRect.sizeDelta = new Vector2(150f, 150f);
        AddText(card.transform, "TOM", 32, new Color32(255, 220, 92, 255), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.19f), Vector2.zero, new Vector2(330f, 60f));

        AddText(menu.transform, "BEST SCORES   00000", 26, new Color32(255, 226, 82, 255), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.055f), Vector2.zero, new Vector2(550f, 60f));
    }

    private void StartGame()
    {
        menu.SetActive(false);
        Time.timeScale = 1f;
    }

    private void ShowInfo()
    {
        Debug.Log("Menu option selected. Play button starts the game.");
    }

    private void CreateButton(Transform parent, string label, Vector2 anchor, Color color, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject(label + "Button");
        go.transform.SetParent(parent);
        var image = go.AddComponent<Image>();
        image.color = color;
        var button = go.AddComponent<Button>();
        button.onClick.AddListener(action);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(-250f, 0f);
        rect.sizeDelta = new Vector2(480f, 82f);
        AddText(go.transform, label, 30, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, true);
    }

    private Text AddText(Transform parent, string value, int size, Color color, TextAnchor alignment, Vector2 anchor, Vector2 position, Vector2 sizeDelta, bool stretch = false)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent);
        var text = go.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.fontStyle = FontStyle.Bold;
        text.color = color;
        text.alignment = alignment;
        var rect = text.rectTransform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = sizeDelta;
        if (stretch)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        return text;
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
