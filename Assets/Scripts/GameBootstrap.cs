using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    private Camera cam;
    private GameObject world;
    private GameObject menuCanvas;
    private GameObject hudCanvas;
    private PlayerController2D player;
    private Text scoreText, timerText, comboText, selectionText, stateText;
    private GameObject pausePanel;
    private float highestY, startY, score, timer;
    private int combo, characterIndex, towerIndex;
    private bool started, paused, gameOver;
    private readonly Color[] towerColors = { new Color32(168,155,205,255), new Color32(205,180,110,255), new Color32(115,190,170,255) };
    private readonly Color[] characterColors = { new Color32(35,105,240,255), new Color32(55,190,70,255), new Color32(230,70,65,255) };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot() { if (FindAnyObjectByType<GameBootstrap>() == null) new GameObject("GameBootstrap").AddComponent<GameBootstrap>(); }

    private void Awake()
    {
        cam = Camera.main;
        if (cam == null) { var go = new GameObject("Main Camera"); go.tag = "MainCamera"; cam = go.AddComponent<Camera>(); }
        cam.orthographic = true; cam.orthographicSize = 7.5f; cam.transform.position = new Vector3(0,0,-10);
        BuildWorld(); BuildHud(); BuildMenu();
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (!started || player == null) return;
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) TogglePause();
        if (paused) return;
        if (gameOver) { if (Input.GetKeyDown(KeyCode.R)) Restart(); return; }
        timer += Time.deltaTime; score = Mathf.Max(score, player.transform.position.y - startY); combo = Mathf.FloorToInt(score / 8f);
        scoreText.text = $"SCORE  {Mathf.FloorToInt(score):00000}"; timerText.text = $"TIME  {timer:0.0}s"; comboText.text = $"COMBO  x{combo}";
        if (player.transform.position.y > highestY - 13f) SpawnPlatforms();
        if (player.transform.position.y < cam.transform.position.y - 10f) EndGame();
        cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(0, Mathf.Max(cam.transform.position.y, player.transform.position.y + 1.5f), -10), Time.deltaTime * 4f);
    }

    private void BuildWorld()
    {
        world = new GameObject("TowerWorld");
        var bg = new GameObject("Sky"); bg.transform.SetParent(world.transform); var b = bg.AddComponent<SpriteRenderer>(); b.sprite = ProceduralSpriteFactory.CreateSkySprite(); b.sortingOrder = -20; bg.transform.localScale = new Vector3(18,16,1); bg.transform.position = new Vector3(0,0,10);
        CreatePlatform(new Vector3(0,-5.5f), new Vector2(22,1.1f), new Color32(255,214,128,255), new Color32(126,84,64,255));
        startY = -4.5f; highestY = startY;
        for (int i=0;i<13;i++) { float y=startY+1.5f+i*1.5f; CreatePlatform(new Vector3(Random.Range(-5.2f,5.2f),y), new Vector2(Random.Range(2.5f,4.5f),1), towerColors[towerIndex], new Color32(70,55,110,255)); highestY=y; }
        CreatePlayer();
    }

    private void CreatePlayer()
    {
        var go = new GameObject("Player"); go.transform.SetParent(world.transform); go.transform.position = new Vector3(0,-2.8f);
        var sr=go.AddComponent<SpriteRenderer>(); sr.sprite=ProceduralSpriteFactory.CreatePlayerSprite(); sr.color=characterColors[characterIndex]; sr.sortingOrder=10;
        var rb=go.AddComponent<Rigidbody2D>(); rb.gravityScale=4.2f; rb.freezeRotation=true; rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous; rb.interpolation=RigidbodyInterpolation2D.Interpolate;
        var col=go.AddComponent<BoxCollider2D>(); col.size=new Vector2(1,1.25f); col.offset=new Vector2(0,-.05f);
        player=go.AddComponent<PlayerController2D>(); player.Initialize(rb); player.SetAudioProfile(characterIndex);
    }

    private GameObject CreatePlatform(Vector3 pos, Vector2 size, Color fill, Color edge)
    { var go=new GameObject("TowerPlatform"); go.transform.SetParent(world.transform); go.transform.position=pos; var sr=go.AddComponent<SpriteRenderer>(); sr.sprite=ProceduralSpriteFactory.CreatePlatformSprite(size.x,size.y,fill,edge); sr.sortingOrder=3; var c=go.AddComponent<BoxCollider2D>(); c.size=size; go.layer=LayerMask.NameToLayer("Default"); return go; }
    private void SpawnPlatforms() { for(int i=0;i<6;i++){ float y=highestY+1.45f+i*1.35f; CreatePlatform(new Vector3(Random.Range(-6.1f,6.1f),y),new Vector2(Random.Range(2.5f,4.5f),1),towerColors[towerIndex],new Color32(70,55,110,255)); highestY=y; } }

    private void BuildMenu()
    {
        menuCanvas=MakeCanvas("MainMenu",100); var root=MakeImage(menuCanvas.transform,"Background",new Color(0.08f,0.28f,0.58f,.96f)); Stretch(root.rectTransform);
        AddText(root.transform,"ICY\nTOWER 2",78,Color.white,TextAnchor.MiddleCenter,new Vector2(.5f,.78f),new Vector2(0,0),new Vector2(650,220));
        AddText(root.transform,"WIECZNA WIEŻA",22,new Color32(190,240,255,255),TextAnchor.MiddleCenter,new Vector2(.5f,.64f),Vector2.zero,new Vector2(500,50));
        MakeButton(root.transform,"GRAJ",new Vector2(.5f,.51f),new Color32(246,195,55,255),StartGame);
        MakeButton(root.transform,"POSTAĆ: TOM",new Vector2(.5f,.40f),new Color32(45,130,220,255),()=>SelectCharacter(0));
        MakeButton(root.transform,"POSTAĆ: LUNA",new Vector2(.5f,.30f),new Color32(45,130,220,255),()=>SelectCharacter(1));
        MakeButton(root.transform,"POSTAĆ: REX",new Vector2(.5f,.20f),new Color32(45,130,220,255),()=>SelectCharacter(2));
        MakeButton(root.transform,"WIEŻA: CLASSIC",new Vector2(1f,.58f),new Color32(45,100,175,255),()=>SelectTower(0));
        MakeButton(root.transform,"WIEŻA: RUINY",new Vector2(1f,.46f),new Color32(45,100,175,255),()=>SelectTower(1));
        MakeButton(root.transform,"WIEŻA: OGRÓD",new Vector2(1f,.34f),new Color32(45,100,175,255),()=>SelectTower(2));
        selectionText=AddText(root.transform,"Selection",20,Color.white,TextAnchor.MiddleCenter,new Vector2(.5f,.08f),Vector2.zero,new Vector2(700,50));
        RefreshSelection();
    }

    private void BuildHud()
    {
        hudCanvas=MakeCanvas("HUD",110); hudCanvas.SetActive(false); scoreText=AddText(hudCanvas.transform,"Score","SCORE 00000",28,new Color32(255,245,150,255),TextAnchor.UpperLeft,new Vector2(0,1),new Vector2(30,-30),new Vector2(500,80)); timerText=AddText(hudCanvas.transform,"Timer","TIME 0.0s",28,new Color32(255,245,150,255),TextAnchor.UpperRight,new Vector2(1,1),new Vector2(-30,-30),new Vector2(500,80)); comboText=AddText(hudCanvas.transform,"Combo","COMBO x0",25,new Color32(255,112,180,255),TextAnchor.UpperCenter,new Vector2(.5f,1),new Vector2(0,-30),new Vector2(500,80)); stateText=AddText(hudCanvas.transform,"State","",38,Color.white,TextAnchor.MiddleCenter,new Vector2(.5f,.5f),new Vector2(0,80),new Vector2(700,180)); pausePanel=MakeImage(hudCanvas.transform,"PausePanel",new Color(0.02f,.04f,.15f,.85f)); Stretch(pausePanel.rectTransform); pausePanel.SetActive(false);
    }

    private void StartGame(){ started=true; Time.timeScale=1; menuCanvas.SetActive(false); hudCanvas.SetActive(true); AudioFactory.PlayStart(); }
    private void SelectCharacter(int i){ characterIndex=i; RefreshSelection(); if(player!=null)player.GetComponent<SpriteRenderer>().color=characterColors[i]; }
    private void SelectTower(int i){ towerIndex=i; RefreshSelection(); }
    private void RefreshSelection(){ if(selectionText!=null)selectionText.text=$"POSTAĆ: {(characterIndex==0?"TOM":characterIndex==1?"LUNA":"REX")}    |    WIEŻA: {(towerIndex==0?"CLASSIC":towerIndex==1?"RUINY":"OGRÓD")}"; }
    private void TogglePause(){ if(gameOver)return; paused=!paused; Time.timeScale=paused?0:1; pausePanel.SetActive(paused); stateText.text=paused?"PAUZA\nP / ESC - kontynuuj":""; }
    private void EndGame(){gameOver=true; stateText.text=$"KONIEC GRY\nWynik: {Mathf.FloorToInt(score)}\nR - restart"; AudioFactory.PlayGameOver();}
    private void Restart(){Time.timeScale=1;SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);}
    private GameObject MakeCanvas(string name,int order){var go=new GameObject(name);go.transform.SetParent(transform);var c=go.AddComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=order;var s=go.AddComponent<CanvasScaler>();s.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;s.referenceResolution=new Vector2(1920,1080);go.AddComponent<GraphicRaycaster>();return go;}
    private Image MakeImage(Transform p,string n,Color c){var go=new GameObject(n);go.transform.SetParent(p);var i=go.AddComponent<Image>();i.color=c;return i;}
    private Text AddText(Transform p,string n,string v,int size,Color c,TextAnchor a,Vector2 anchor,Vector2 pos,Vector2 sd){var go=new GameObject(n);go.transform.SetParent(p);var t=go.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=v;t.fontSize=size;t.fontStyle=FontStyle.Bold;t.color=c;t.alignment=a;var r=t.rectTransform;r.anchorMin=anchor;r.anchorMax=anchor;r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=sd;return t;}
    private void MakeButton(Transform p,string label,Vector2 anchor,Color c,UnityEngine.Events.UnityAction action){var go=new GameObject(label);go.transform.SetParent(p);var i=go.AddComponent<Image>();i.color=c;var b=go.AddComponent<Button>();b.onClick.AddListener(action);var r=go.GetComponent<RectTransform>();r.anchorMin=anchor;r.anchorMax=anchor;r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=anchor.x>.75f?new Vector2(-250,0):new Vector2(-250,0);r.sizeDelta=new Vector2(480,75);AddText(go.transform,"Label",label,25,Color.white,TextAnchor.MiddleCenter,new Vector2(.5f,.5f),Vector2.zero,new Vector2(470,70));}
    private void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}
    private void OnDestroy(){Time.timeScale=1;}
}
