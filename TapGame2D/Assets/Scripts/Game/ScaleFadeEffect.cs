using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// タップ時またはオブジェクト出現時に、波紋のように拡大しながらフェードアウトするエフェクトクラス
/// </summary>
[RequireComponent(typeof(Image))]
public class ScaleFadeEffect : MonoBehaviour
{
    [Header("Effect Setting")]
    [SerializeField]
    private Vector3 targetScale = new Vector3(2.5f, 2.5f, 1.0f); // 最終的な拡大スケール

    [SerializeField]
    private float duration = 0.4f; // エフェクトの持続時間 (秒)

    private Image targetImage;
    private Vector3 initialScale;
    private Color originalColor;
    private float elapsedTime = 0.0f;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        initialScale = transform.localScale;
    }

    private void Start()
    {
        if (targetImage != null)
        {
            originalColor = targetImage.color;
        }
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedTime / duration);

        // 1. 徐々に拡大する
        transform.localScale = Vector3.Lerp(initialScale, targetScale, progress);

        // 2. 徐々にフェードアウトする
        if (targetImage != null)
        {
            float alpha = Mathf.Clamp01(1.0f - progress);
            targetImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }

        // 3. 演出が終了したらオブジェクトを破棄する
        if (progress >= 1.0f)
        {
            Destroy(gameObject);
        }
    }
}