using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// タップ時にスコア値（例: "+100"）などを浮き上がらせながらフェードアウトさせる演出クラス
/// </summary>
[RequireComponent(typeof(Text))]
public class FloatingText : MonoBehaviour
{
    [Header("Movement Setting")]
    [SerializeField]
    private float moveSpeed = 120.0f; // 1秒間の上昇量

    [SerializeField]
    private float lifeTime = 0.6f; // 表示される時間 (秒)

    private Text targetText;
    private RectTransform rectTransform;
    private Color originalColor;
    private float elapsedTime = 0.0f;

    private void Awake()
    {
        targetText = GetComponent<Text>();
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 表示する文字とカラーを初期化するメソッド
    /// </summary>
    /// <param name="text">表示文字列 (例: "+100", "-200")</param>
    /// <param name="color">テキストのカラー</param>
    public void Initialize(string text, Color color)
    {
        if (targetText != null)
        {
            targetText.text = text;
            targetText.color = color;
            originalColor = color;
        }
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        // 1. 上方向へ移動させる
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;
        }

        // 2. フェードアウト処理
        if (targetText != null)
        {
            float alpha = Mathf.Clamp01(1.0f - (elapsedTime / lifeTime));
            targetText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }

        // 3. 時間が立ったら破棄する
        if (elapsedTime >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}