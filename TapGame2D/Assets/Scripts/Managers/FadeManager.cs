using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移時のフェードトランジション演出を管理するシングルトンクラス
/// </summary>
public class FadeManager : MonoBehaviour
{
    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static FadeManager Instance { get; private set; }

    // フェードにかける時間（秒）
    private const float FadeDuration = 0.5f;

    private Canvas fadeCanvas;
    private Image fadeImage;
    private bool isFading = false;

    private void Awake()
    {
        // シングルトンの初期化と重複排除
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoadはルートオブジェクトである必要があるため、親から切り離す
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            CreateFadeCanvas();
        }

        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// コード上でフェード演出用のCanvasとImageを動的に作成する
    /// </summary>
    private void CreateFadeCanvas()
    {
        // Canvasの生成
        GameObject canvasObject = new GameObject("FadeCanvas");
        canvasObject.transform.SetParent(transform);
        fadeCanvas = canvasObject.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // 最前面に表示されるようにソート順を設定
        fadeCanvas.sortingOrder = 999;

        // UIスケール設定
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1980);

        // レイキャスターの設定
        canvasObject.AddComponent<GraphicRaycaster>();

        // 全画面黒画像の生成
        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(canvasObject.transform, false);
        fadeImage = imageObject.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // 初期状態は透明

        // アンカーを全画面にストレッチ
        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        // 初期状態では描画を無効化しておく
        canvasObject.SetActive(false);
    }

    /// <summary>
    /// 指定されたシーンへのトランジションを開始する
    /// </summary>
    /// <param name="sceneName">読み込むシーンの名前</param>
    public void TransitionToScene(string sceneName)
    {
        // すでにフェード中であれば多重実行を防ぐ
        if (isFading)
        {
            return;
        }

        StartCoroutine(TransitionSequence(sceneName));
    }

    /// <summary>
    /// フェードアウト、シーン遷移、フェードインの一連の流れを実行するコルーチン
    /// </summary>
    /// <param name="sceneName">遷移先シーン名</param>
    private IEnumerator TransitionSequence(string sceneName)
    {
        isFading = true;
        fadeCanvas.gameObject.SetActive(true);

        // 1. フェードアウト (透明 -> 黒)
        float elapsedTime = 0f;

        while (elapsedTime < FadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / FadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = Color.black;

        // 2. シーンの読み込み
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // シーン遷移直後は一時的に待つ
        yield return new WaitForSeconds(0.1f);

        // 3. フェードイン (黒 -> 透明)
        elapsedTime = 0f;

        while (elapsedTime < FadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1.0f - (elapsedTime / FadeDuration));
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0);

        fadeCanvas.gameObject.SetActive(false);
        isFading = false;
    }
}