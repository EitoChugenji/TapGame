using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ゲームの状態を表す列挙型
/// </summary>
public enum GameState
{
    Title,
    Playing,
    Result
}

/// <summary>
/// ゲーム全体の進行管理、30秒制限時間の監視、およびオブジェクトの自動生成を行うクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// 現在のゲーム状態
    /// </summary>
    public GameState CurrentState { get; private set; } = GameState.Title;

    /// <summary>
    /// 残り時間 (秒)
    /// </summary>
    public float RemainingTime { get; private set; }

    [Header("Managers")]
    [SerializeField]
    private ScoreManager scoreManager;

    [SerializeField]
    private GameUIManager gameUIManager;

    [Header("Spawn Settings")]
    [SerializeField]
    private GameObject[] objectPrefabs; // 0: 通常(青), 1: レア(金)

    [SerializeField]
    private RectTransform spawnContainer; // オブジェクトが生成される親パネル

    // ゲームの制限時間（30秒固定）
    private const float PlayDuration = 30.0f;

    // オブジェクトの生成間隔（秒）
    private const float SpawnInterval = 1.0f;

    private float spawnTimer = 0.0f;
    private bool isGameActive = false;

    private void Start()
    {
        // ゲーム画面ロード時、自動的にゲームを開始する
        InitializeGame();
    }

    /// <summary>
    /// ゲームの初期化とカウントダウン開始
    /// </summary>
    private void InitializeGame()
    {
        CurrentState = GameState.Playing;
        RemainingTime = PlayDuration;
        
        if (scoreManager != null)
        {
            scoreManager.ResetScore();
        }

        if (gameUIManager != null)
        {
            gameUIManager.UpdateUI(0, RemainingTime, 0);
        }

        // BGMを再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM();
            SoundManager.Instance.PlaySE("se_game_start");
        }

        isGameActive = true;
    }

    private void Update()
    {
        // ゲームが非アクティブ、または一時停止中の場合は入力を受け付けない
        if (!isGameActive || Time.timeScale == 0f)
        {
            return;
        }

        // 1. タイマー更新
        RemainingTime -= Time.deltaTime;
        if (RemainingTime <= 0f)
        {
            RemainingTime = 0f;
            EndGame();
        }

        // 2. UIの更新
        if (gameUIManager != null && scoreManager != null)
        {
            gameUIManager.UpdateUI(scoreManager.CurrentScore, RemainingTime, scoreManager.ComboCount);
        }

        // 3. オブジェクトのスポーン管理
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= SpawnInterval)
        {
            spawnTimer = 0.0f;
            SpawnRandomPopupObject();
        }

        // 4. タップ入力検知（重なったオブジェクトも同時に処理するため、グローバルに検知）
        if (Input.GetMouseButtonDown(0))
        {
            HandleScreenTap();
        }
    }

    /// <summary>
    /// 画面のタップ入力を検知し、重なっているものも含めてすべてのオブジェクトを処理する
    /// </summary>
    private void HandleScreenTap()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        // ポインターのイベントデータを作成
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        // レイキャスト結果のリスト
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // レイキャストされたUI要素を順に確認
        foreach (RaycastResult result in results)
        {
            // UIのボタンやパネルなど、オブジェクト以外のクリックをブロックすべき要素に当たった場合は処理を中断
            if (result.gameObject.GetComponentInParent<Button>() != null ||
                result.gameObject.name.Contains("Panel") ||
                result.gameObject.name.Contains("Button"))
            {
                break;
            }

            // オブジェクトがあればタップ処理を実行
            SpawnedObject spawned = result.gameObject.GetComponent<SpawnedObject>();
            if (spawned != null)
            {
                spawned.Tap();
            }
        }
    }

    /// <summary>
    /// 画面のランダムな位置にオブジェクトをポップアップ生成する
    /// </summary>
    private void SpawnRandomPopupObject()
    {
        if (objectPrefabs == null || objectPrefabs.Length == 0 || spawnContainer == null)
        {
            return;
        }

        // 生成するオブジェクトの種類を抽選 (通常80%、金20%)
        int prefabIndex = 0;
        float randomValue = Random.value;

        if (randomValue < 0.80f)
        {
            prefabIndex = 0; // 通常
        }

        else
        {
            prefabIndex = 1; // レア
        }

        // 画面のセーフエリア内に収まるように座標を決定 (Canvas 1080x1920 想定)
        // 左右マージン、上下のUI（HUDや一時停止ボタン）への被りを考慮
        float randomX = Random.Range(-450f, 450f);
        float randomY = Random.Range(-750f, 650f);

        GameObject prefab = objectPrefabs[prefabIndex];
        GameObject spawnedObj = Instantiate(prefab, spawnContainer);

        // RectTransformの位置を設定
        RectTransform rectTrans = spawnedObj.GetComponent<RectTransform>();
        if (rectTrans != null)
        {
            rectTrans.anchoredPosition = new Vector2(randomX, randomY);
        }
    }

    /// <summary>
    /// ゲームの強制終了と結果画面への遷移
    /// </summary>
    private void EndGame()
    {
        isGameActive = false;
        CurrentState = GameState.Result;

        // BGMを止めて終了音を再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlaySE("se_game_over");
        }

        // スコアを保存
        if (scoreManager != null)
        {
            // ハイスコア更新判定を行い、値を一時保存（ResultScene表示用）
            bool isNewRecord = scoreManager.CheckAndSaveHighScore();
            PlayerPrefs.SetInt("LastScore", scoreManager.CurrentScore);
            PlayerPrefs.SetInt("IsNewRecord", isNewRecord ? 1 : 0);
            PlayerPrefs.Save();
        }

        // ゲームオーバー演出を表示したのち、フェード遷移
        if (gameUIManager != null)
        {
            gameUIManager.ShowGameOver();
        }

        StartCoroutine(TransitionToResultDelay());
    }

    /// <summary>
    /// ゲーム終了UI表示後、一定時間待ってからResultシーンに遷移する
    /// </summary>
    private IEnumerator TransitionToResultDelay()
    {
        yield return new WaitForSeconds(1.5f);
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.TransitionToScene("Result");
        }
    }
}