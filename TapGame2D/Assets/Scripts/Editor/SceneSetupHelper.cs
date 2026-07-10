using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// タップゲームに必要なPrefabの自動生成、シーンの自動構築、
/// およびBuild Settingsへの登録を一括で行うUnityエディタ拡張スクリプト
/// </summary>
public class SceneSetupHelper : EditorWindow
{
    private const string PrefabsFolderPath = "Assets/Prefabs";
    private const string ScenesFolderPath = "Assets/Scenes";

    [MenuItem("Tools/Setup Tap Game")]
    public static void SetupGame()
    {
        Debug.Log("Starting Tap Game Setup Process...");

        // 1. フォルダの作成
        CreateRequiredFolders();

        // 2. プレハブの生成
        GameObject floatingTextPrefab = CreateFloatingTextPrefab();
        GameObject tapEffectPrefab = CreateTapEffectPrefab();
        GameObject normalObjPrefab = CreateSpawnedObjectPrefab(ObjectType.Normal, 1.5f, 100, Color.cyan, "prefab_spawned_normal");
        GameObject rareObjPrefab = CreateSpawnedObjectPrefab(ObjectType.Rare, 1.0f, 300, new Color(1.0f, 0.85f, 0f), "prefab_spawned_rare");

        // 3. シーンの構築
        CreateTitleScene();
        CreateGameScene(normalObjPrefab, rareObjPrefab, floatingTextPrefab, tapEffectPrefab);
        CreateResultScene();

        // 4. Build Settingsへの登録
        RegisterScenesInBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Setup Complete", "All scenes, prefabs and build settings have been configured successfully!", "OK");
        Debug.Log("Tap Game Setup Process Completed Successfully.");
    }

    /// <summary>
    /// アセット管理に必要なフォルダを自動生成する
    /// </summary>
    private static void CreateRequiredFolders()
    {
        if (!AssetDatabase.IsValidFolder(PrefabsFolderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder(ScenesFolderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
    }

    /// <summary>
    /// 得点ポップアップ用（FloatingText）Prefabの自動生成
    /// </summary>
    private static GameObject CreateFloatingTextPrefab()
    {
        GameObject textObj = new GameObject("FloatingText", typeof(RectTransform));

        // テキストが切れないよう十分なサイズを確保する
        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300f, 100f);

        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 65;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.Bold;
        // "+100" / "+300" が切れないようオーバーフローを許可する
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        // 文字の視認性を高めるためのシャドウ効果を追加
        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(3f, -3f);

        textObj.AddComponent<FloatingText>();

        string path = Path.Combine(PrefabsFolderPath, "prefab_floating_text.prefab");
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(textObj, path);
        DestroyImmediate(textObj);
        return prefab;
    }

    /// <summary>
    /// タップ時のエフェクト（ScaleFadeEffect）Prefabの自動生成
    /// </summary>
    private static GameObject CreateTapEffectPrefab()
    {
        GameObject effectObj = new GameObject("TapEffect", typeof(RectTransform));

        // 簡易的な白丸をImageとして生成
        Image image = effectObj.AddComponent<Image>();
        image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        image.color = Color.white;

        RectTransform rect = effectObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100f, 100f);

        effectObj.AddComponent<ScaleFadeEffect>();

        string path = Path.Combine(PrefabsFolderPath, "prefab_tap_effect.prefab");
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(effectObj, path);
        DestroyImmediate(effectObj);
        return prefab;
    }

    /// <summary>
    /// ポップアップ出現する各種オブジェクトのPrefabを生成する
    /// </summary>
    private static GameObject CreateSpawnedObjectPrefab(ObjectType type, float lifeDuration, int score, Color color, string assetName)
    {
        GameObject obj = new GameObject(assetName, typeof(RectTransform));
        Image image = obj.AddComponent<Image>();

        // 円形スプライトを適用して丸にする
        image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        image.color = color;

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(150f, 150f);

        // タップ検知用コンポーネントの設定
        SpawnedObject spawnedObj = obj.AddComponent<SpawnedObject>();

        // リフレクションを使用してシリアライズフィールドの値を設定する
        var typeField = typeof(SpawnedObject).GetField("objectType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var durationField = typeof(SpawnedObject).GetField("lifeDuration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var scoreField = typeof(SpawnedObject).GetField("scoreValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var colorField = typeof(SpawnedObject).GetField("themeColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (typeField != null) typeField.SetValue(spawnedObj, type);
        if (durationField != null) durationField.SetValue(spawnedObj, lifeDuration);
        if (scoreField != null) scoreField.SetValue(spawnedObj, score);
        if (colorField != null) colorField.SetValue(spawnedObj, color);

        string path = Path.Combine(PrefabsFolderPath, assetName + ".prefab");
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
        DestroyImmediate(obj);
        return prefab;
    }

    /// <summary>
    /// 基本的なUI Canvasを作成し、1080x1920縦画面用のScalerを設定する
    /// </summary>
    private static (GameObject canvasObj, RectTransform rect) CreateBaseCanvas(string name, GameObject parent)
    {
        GameObject canvasObj = new GameObject(name, typeof(RectTransform));
        canvasObj.transform.SetParent(parent.transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        canvasObj.AddComponent<GraphicRaycaster>();

        RectTransform rect = canvasObj.GetComponent<RectTransform>();
        return (canvasObj, rect);
    }

    /// <summary>
    /// 全画面を覆う背景パネルを作成する
    /// </summary>
    private static GameObject CreateBackground(string name, Color color, Transform parent)
    {
        GameObject bgObj = new GameObject(name, typeof(RectTransform));
        bgObj.transform.SetParent(parent, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = color;

        RectTransform rect = bgObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        return bgObj;
    }

    /// <summary>
    /// テキストUIを生成する
    /// </summary>
    private static Text CreateText(string name, string content, int fontSize, Color color, Transform parent, Vector2 anchoredPos, Vector2 size)
    {
        GameObject textObj = new GameObject(name, typeof(RectTransform));
        textObj.transform.SetParent(parent, false);

        Text txt = textObj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.text = content;
        txt.fontSize = fontSize;
        txt.color = color;
        txt.alignment = TextAnchor.MiddleCenter;

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        return txt;
    }

    /// <summary>
    /// ボタンUIを生成する
    /// </summary>
    private static Button CreateButton(string name, string labelText, int fontSize, Color btnColor, Transform parent, Vector2 anchoredPos, Vector2 size)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform));
        btnObj.transform.SetParent(parent, false);

        Image img = btnObj.AddComponent<Image>();
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
        img.type = Image.Type.Sliced;
        img.color = btnColor;

        Button btn = btnObj.AddComponent<Button>();

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        CreateText(name + "Text", labelText, fontSize, Color.white, btnObj.transform, Vector2.zero, size);

        return btn;
    }

    /// <summary>
    /// Titleシーンの構築
    /// </summary>
    private static void CreateTitleScene()
    {
        Scene titleScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        GameObject rootObj = new GameObject("SceneRoot");

        // EventSystemの追加（ボタンのクリック・タップ検知に必要）
        GameObject titleEventSystem = new GameObject("EventSystem");
        titleEventSystem.transform.SetParent(rootObj.transform);
        titleEventSystem.AddComponent<EventSystem>();
        titleEventSystem.AddComponent<StandaloneInputModule>();

        // グローバルマネージャーの追加（SoundManager、FadeManager）
        GameObject managersObj = new GameObject("GlobalManagers");
        managersObj.AddComponent<SoundManager>();
        managersObj.AddComponent<FadeManager>();

        // UI Canvasの構築
        var (canvasObj, canvasRect) = CreateBaseCanvas("TitleCanvas", rootObj);

        // 背景
        CreateBackground("Background", new Color(0.1f, 0.08f, 0.15f), canvasObj.transform);

        // タイトルテキスト
        CreateText("TitleText", "CRYSTAL\nPOP", 110, new Color(0.9f, 0.8f, 1f), canvasObj.transform, new Vector2(0, 500f), new Vector2(800, 300));

        // ハイスコアテキスト
        Text highScoreTxt = CreateText("HighScoreText", "HIGH SCORE: 0", 45, new Color(1.0f, 0.85f, 0f), canvasObj.transform, new Vector2(0, 150f), new Vector2(800, 100));

        // 遊び方説明
        CreateText("HowToPlayText",
            "HOW TO PLAY\n\nCrystals will appear on screen!\nTap them before they disappear!\nGold crystal: BONUS score!",
            35, new Color(0.7f, 0.7f, 0.8f), canvasObj.transform, new Vector2(0, -150f), new Vector2(900, 350));

        // 開始ボタン
        Button startBtn = CreateButton("StartButton", "START", 45, new Color(0.45f, 0.2f, 0.8f), canvasObj.transform, new Vector2(0, -600f), new Vector2(450, 130));

        // ハイスコアリセットボタン
        Button resetBtn = CreateButton("ResetButton", "RESET SCORE", 30, new Color(0.6f, 0.2f, 0.2f), canvasObj.transform, new Vector2(0, -780f), new Vector2(350, 100));

        // TitleUIManagerの作成と参照バインド
        GameObject uiManagerObj = new GameObject("TitleUIManager");
        uiManagerObj.transform.SetParent(rootObj.transform);
        TitleUIManager titleUI = uiManagerObj.AddComponent<TitleUIManager>();

        typeof(TitleUIManager).GetField("startButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(titleUI, startBtn);
        typeof(TitleUIManager).GetField("resetScoreButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(titleUI, resetBtn);
        typeof(TitleUIManager).GetField("highScoreText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(titleUI, highScoreTxt);

        EditorSceneManager.SaveScene(titleScene, Path.Combine(ScenesFolderPath, "Title.unity"));
    }

    /// <summary>
    /// Gameシーンの構築
    /// </summary>
    private static void CreateGameScene(GameObject normalPrefab, GameObject rarePrefab, GameObject textPrefab, GameObject effectPrefab)
    {
        Scene gameScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        GameObject rootObj = new GameObject("SceneRoot");

        // EventSystemの追加（マウスクリック・タップ入力を検知するために必須）
        // StandaloneInputModuleはマウスとタッチ両方に対応している
        GameObject gameEventSystem = new GameObject("EventSystem");
        gameEventSystem.transform.SetParent(rootObj.transform);
        gameEventSystem.AddComponent<EventSystem>();
        gameEventSystem.AddComponent<StandaloneInputModule>();

        // グローバルマネージャーの追加
        // DontDestroyOnLoadシングルトンのため、Titleから来た場合は自動的に破棄されるが、
        // Gameシーンを直接再生したときにもFadeManager/SoundManagerが存在するよう必ず配置する
        GameObject gameManagersObj = new GameObject("GlobalManagers");
        gameManagersObj.AddComponent<SoundManager>();
        gameManagersObj.AddComponent<FadeManager>();

        // UI Canvasの構築
        var (canvasObj, canvasRect) = CreateBaseCanvas("GameCanvas", rootObj);

        // 背景
        CreateBackground("Background", new Color(0.08f, 0.08f, 0.1f), canvasObj.transform);

        // スポーン兼エフェクト描画コンテナ（クリスタルやエフェクトが生成される親）
        GameObject spawnContainerObj = new GameObject("SpawnContainer", typeof(RectTransform));
        spawnContainerObj.transform.SetParent(canvasObj.transform, false);
        RectTransform spawnContainerRect = spawnContainerObj.GetComponent<RectTransform>();
        spawnContainerRect.anchorMin = Vector2.zero;
        spawnContainerRect.anchorMax = Vector2.one;
        spawnContainerRect.sizeDelta = Vector2.zero;

        // HUD テキスト（タイマー、スコア、コンボ）
        Text timerTxt = CreateText("TimerText", "TIME\n30.0s", 45, Color.white, canvasObj.transform, new Vector2(-330f, 750f), new Vector2(300, 150));
        Text scoreTxt = CreateText("ScoreText", "SCORE\n0", 45, Color.white, canvasObj.transform, new Vector2(0f, 750f), new Vector2(300, 150));
        Text comboTxt = CreateText("ComboText", "COMBO!", 55, new Color(1.0f, 0.85f, 0f), canvasObj.transform, new Vector2(330f, 750f), new Vector2(300, 150));
        comboTxt.gameObject.SetActive(false);

        // 一時停止ボタン
        Button pauseBtn = CreateButton("PauseButton", "PAUSE", 28, new Color(0.3f, 0.3f, 0.35f), canvasObj.transform, new Vector2(430f, 900f), new Vector2(160, 80));

        // 一時停止オーバーレイ
        GameObject pausePanel = CreateBackground("PausePanel", new Color(0, 0, 0, 0.75f), canvasObj.transform);
        CreateText("PauseTitleText", "PAUSED", 90, Color.white, pausePanel.transform, new Vector2(0, 200f), new Vector2(600, 150));
        Button resumeBtn = CreateButton("ResumeButton", "RESUME", 40, new Color(0.2f, 0.6f, 0.3f), pausePanel.transform, new Vector2(0, -200f), new Vector2(380, 110));
        pausePanel.SetActive(false);

        // ゲームオーバーオーバーレイ
        GameObject gameOverPanel = CreateBackground("GameOverPanel", new Color(0, 0, 0, 0.8f), canvasObj.transform);
        CreateText("GameOverTitleText", "TIME UP!", 95, Color.red, gameOverPanel.transform, new Vector2(0, 0), new Vector2(800, 200));
        gameOverPanel.SetActive(false);

        // GameUIManagerの作成と参照バインド
        GameObject uiManagerObj = new GameObject("GameUIManager");
        uiManagerObj.transform.SetParent(rootObj.transform);
        GameUIManager gameUI = uiManagerObj.AddComponent<GameUIManager>();

        typeof(GameUIManager).GetField("scoreText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gameUI, scoreTxt);
        typeof(GameUIManager).GetField("timerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gameUI, timerTxt);
        typeof(GameUIManager).GetField("comboText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gameUI, comboTxt);
        typeof(GameUIManager).GetField("gameOverPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gameUI, gameOverPanel);
        typeof(GameUIManager).GetField("pausePanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gameUI, pausePanel);
        typeof(GameUIManager).GetField("pauseButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gameUI, pauseBtn);
        typeof(GameUIManager).GetField("resumeButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gameUI, resumeBtn);

        // ScoreManagerの作成
        GameObject scoreManagerObj = new GameObject("ScoreManager");
        scoreManagerObj.transform.SetParent(rootObj.transform);
        ScoreManager scoreManager = scoreManagerObj.AddComponent<ScoreManager>();

        // EffectManagerの作成と参照バインド
        GameObject effectManagerObj = new GameObject("EffectManager");
        effectManagerObj.transform.SetParent(rootObj.transform);
        EffectManager effectManager = effectManagerObj.AddComponent<EffectManager>();
        typeof(EffectManager).GetField("floatingTextPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(effectManager, textPrefab);
        typeof(EffectManager).GetField("tapEffectPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(effectManager, effectPrefab);
        typeof(EffectManager).GetField("effectContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(effectManager, spawnContainerRect);

        // GameManagerの作成と参照バインド
        GameObject gameManagerObj = new GameObject("GameManager");
        gameManagerObj.transform.SetParent(rootObj.transform);
        GameManager gameManager = gameManagerObj.AddComponent<GameManager>();

        typeof(GameManager).GetField("scoreManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gameManager, scoreManager);
        typeof(GameManager).GetField("gameUIManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gameManager, gameUI);
        typeof(GameManager).GetField("spawnContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gameManager, spawnContainerRect);

        GameObject[] prefabsArr = new GameObject[] { normalPrefab, rarePrefab };
        typeof(GameManager).GetField("objectPrefabs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gameManager, prefabsArr);

        EditorSceneManager.SaveScene(gameScene, Path.Combine(ScenesFolderPath, "Game.unity"));
    }

    /// <summary>
    /// Resultシーンの構築
    /// </summary>
    private static void CreateResultScene()
    {
        Scene resultScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        GameObject rootObj = new GameObject("SceneRoot");

        // EventSystemの追加（ボタンのクリック・タップ検知に必要）
        GameObject resultEventSystem = new GameObject("EventSystem");
        resultEventSystem.transform.SetParent(rootObj.transform);
        resultEventSystem.AddComponent<EventSystem>();
        resultEventSystem.AddComponent<StandaloneInputModule>();

        // グローバルマネージャーの追加
        GameObject managersObj = new GameObject("GlobalManagers");
        managersObj.AddComponent<SoundManager>();
        managersObj.AddComponent<FadeManager>();

        // UI Canvasの構築
        var (canvasObj, canvasRect) = CreateBaseCanvas("ResultCanvas", rootObj);

        // 背景
        CreateBackground("Background", new Color(0.12f, 0.08f, 0.15f), canvasObj.transform);

        // リザルトヘッダー
        CreateText("ResultHeader", "RESULT", 100, Color.white, canvasObj.transform, new Vector2(0, 550f), new Vector2(800, 150));

        // 最終スコア
        Text scoreTxt = CreateText("ScoreText", "SCORE: 0", 75, new Color(1.0f, 0.85f, 0f), canvasObj.transform, new Vector2(0, 250f), new Vector2(800, 200));

        // ハイスコア
        Text highScoreTxt = CreateText("HighScoreText", "HIGH SCORE: 0", 45, Color.white, canvasObj.transform, new Vector2(0, 50f), new Vector2(800, 100));

        // 新記録パネル
        GameObject newRecordObj = new GameObject("NewRecordPanel", typeof(RectTransform));
        newRecordObj.transform.SetParent(canvasObj.transform, false);
        RectTransform newRecordRect = newRecordObj.GetComponent<RectTransform>();
        newRecordRect.anchoredPosition = new Vector2(0, -60f);
        newRecordRect.sizeDelta = new Vector2(600, 80);
        CreateText("NewRecordText", "NEW RECORD!", 50, Color.green, newRecordObj.transform, Vector2.zero, new Vector2(600, 80));
        newRecordObj.SetActive(false);

        // ボタン（リトライ、タイトル）
        Button retryBtn = CreateButton("RetryButton", "RETRY", 45, new Color(0.2f, 0.5f, 0.8f), canvasObj.transform, new Vector2(0, -320f), new Vector2(450, 130));
        Button titleBtn = CreateButton("TitleButton", "TITLE", 45, new Color(0.35f, 0.35f, 0.4f), canvasObj.transform, new Vector2(0, -520f), new Vector2(450, 130));

        // ResultUIManagerの作成と参照バインド
        GameObject uiManagerObj = new GameObject("ResultUIManager");
        uiManagerObj.transform.SetParent(rootObj.transform);
        ResultUIManager resultUI = uiManagerObj.AddComponent<ResultUIManager>();

        typeof(ResultUIManager).GetField("finalScoreText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(resultUI, scoreTxt);
        typeof(ResultUIManager).GetField("highScoreText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(resultUI, highScoreTxt);
        typeof(ResultUIManager).GetField("newRecordPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(resultUI, newRecordObj);
        typeof(ResultUIManager).GetField("retryButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(resultUI, retryBtn);
        typeof(ResultUIManager).GetField("titleButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(resultUI, titleBtn);

        EditorSceneManager.SaveScene(resultScene, Path.Combine(ScenesFolderPath, "Result.unity"));
    }

    /// <summary>
    /// 生成した3つのシーンをBuild Settingsに自動登録する
    /// </summary>
    private static void RegisterScenesInBuildSettings()
    {
        string[] scenePaths = new string[]
        {
            Path.Combine(ScenesFolderPath, "Title.unity"),
            Path.Combine(ScenesFolderPath, "Game.unity"),
            Path.Combine(ScenesFolderPath, "Result.unity")
        };

        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[scenePaths.Length];
        for (int i = 0; i < scenePaths.Length; i++)
        {
            buildScenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
        }

        EditorBuildSettings.scenes = buildScenes;
        Debug.Log("Build Settings updated with 3 scenes: Title, Game, Result.");
    }
}