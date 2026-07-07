using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 出現するクリスタルオブジェクトの種類
/// </summary>
public enum ObjectType
{
    Normal, // 通常 (青)
    Rare,   // レア (金)
}

/// <summary>
/// 画面のランダムな位置にポップアップ出現し、エフェクトとともにスケールインし、
/// 一定時間内にタップされないと自動消滅するクリスタルオブジェクトのクラス
/// </summary>
public class SpawnedObject : MonoBehaviour
{
    [Header("Object Property")]
    [SerializeField]
    private ObjectType objectType = ObjectType.Normal;

    [SerializeField]
    private float lifeDuration = 1.5f; // 表示されてから自動消滅するまでの時間 (秒)

    [SerializeField]
    private int scoreValue = 100; // タップ時の得点加算値（減点の場合はマイナス）

    [SerializeField]
    private Color themeColor = Color.cyan; // テーマカラー

    private RectTransform rectTransform;
    private Image objectImage;
    private float timer = 0.0f;
    private bool isTapped = false;
    private bool isExpired = false;

    // 出現時のスケールインアニメーション時間（秒）
    private const float ScaleInDuration = 0.2f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        objectImage = GetComponent<Image>();
        
        // 出現時はまずサイズを0（非表示状態）にする
        transform.localScale = Vector3.zero;
    }

    private void Start()
    {
        // 1. スケールインアニメーションの開始
        StartCoroutine(ScaleInSequence());

        // 2. 出現時エフェクトの発生
        if (EffectManager.Instance != null && rectTransform != null)
        {
            // 出現位置に淡い光（テーマカラーを少し透明にしたもの）を生成
            Color spawnColor = new Color(themeColor.r, themeColor.g, themeColor.b, 0.4f);
            EffectManager.Instance.SpawnTapEffect(rectTransform.anchoredPosition, spawnColor);
        }
    }

    /// <summary>
    /// 出現時にスケールを 0 から 1 へ滑らかに拡大するコルーチン
    /// </summary>
    private IEnumerator ScaleInSequence()
    {
        float elapsed = 0.0f;
        while (elapsed < ScaleInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / ScaleInDuration);
            
            // イージング（SmoothStep）をかけて滑らかにする
            float scale = progress * progress * (3f - 2f * progress);
            transform.localScale = new Vector3(scale, scale, 1.0f);
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    private void Update()
    {
        if (isTapped || isExpired)
        {
            return;
        }

        // ライフタイムのカウントダウン
        timer += Time.deltaTime;
        
        // 残り時間が少なくなったら点滅させる視覚的フィードバック
        if (lifeDuration - timer <= 0.4f)
        {
            if (objectImage != null)
            {
                // 残り0.4秒以下で高速点滅
                float flash = Mathf.PingPong(Time.time * 15f, 1.0f);
                objectImage.color = new Color(themeColor.r, themeColor.g, themeColor.b, 0.4f + flash * 0.6f);
            }
        }

        if (timer >= lifeDuration)
        {
            OnLifeExpired();
        }
    }

    /// <summary>
    /// 制限時間内にタップされず、ライフタイムが尽きて自動消滅した際の処理
    /// </summary>
    private void OnLifeExpired()
    {
        isExpired = true;

        // 通常またはレアオブジェクトを逃した（自動消滅した）場合はコンボをリセットする
        if (objectType == ObjectType.Normal || objectType == ObjectType.Rare)
        {
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.ResetCombo();
            }
        }

        // 消滅時アニメーション（徐々に小さくする）後にオブジェクトを破棄
        StartCoroutine(ScaleOutAndDestroy());
    }

    /// <summary>
    /// 自動消滅時にスケールを 1 から 0 へ小さくして消去するコルーチン
    /// </summary>
    private IEnumerator ScaleOutAndDestroy()
    {
        float elapsed = 0.0f;
        const float ScaleOutDuration = 0.12f;
        Vector3 initialScale = transform.localScale;

        while (elapsed < ScaleOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / ScaleOutDuration);
            float scale = Mathf.Lerp(initialScale.x, 0.0f, progress);
            transform.localScale = new Vector3(scale, scale, 1.0f);
            yield return null;
        }
        Destroy(gameObject);
    }


    /// <summary>
    /// クリスタルオブジェクトがタップされた時の処理
    /// </summary>
    public void Tap()
    {
        // すでにタップ済み、または消滅直前なら多重実行を防ぐ
        if (isTapped || isExpired)
        {
            return;
        }
        isTapped = true;

        // 1. スコア・コンボ加算処理
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.AddScore(scoreValue);
        }

        // 2. タップ成功時のビジュアルエフェクトおよび得点表示
        if (EffectManager.Instance != null && rectTransform != null)
        {
            Vector2 tapPos = rectTransform.anchoredPosition;

            // タップ波紋エフェクト
            EffectManager.Instance.SpawnTapEffect(tapPos, themeColor);

            // ポップアップ数値テキスト
            string popupText = scoreValue >= 0 ? "+" + scoreValue.ToString() : scoreValue.ToString();
            EffectManager.Instance.SpawnFloatingText(tapPos, popupText, themeColor);
        }

        // 3. 効果音の再生
        if (SoundManager.Instance != null)
        {
            string seName = "se_tap";
            if (objectType == ObjectType.Rare)
            {
                seName = "se_rare_tap";
            }

            SoundManager.Instance.PlaySE(seName);
        }

        // 4. 自身の消滅
        Destroy(gameObject);
    }
}