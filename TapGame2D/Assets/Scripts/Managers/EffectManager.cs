using UnityEngine;

/// <summary>
/// タップ時のエフェクトや得点ポップアップテキストの生成を管理するクラス
/// </summary>
public class EffectManager : MonoBehaviour
{
    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static EffectManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField]
    private GameObject floatingTextPrefab;

    [SerializeField]
    private GameObject tapEffectPrefab;

    [Header("Container")]
    [SerializeField]
    private RectTransform effectContainer;

    private void Awake()
    {
        // シーン内のインスタンス設定
        if (Instance == null)
        {
            Instance = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// タップされた位置に得点ポップアップテキスト（FloatingText）を生成する
    /// </summary>
    /// <param name="position">生成するキャンバス上のローカル座標</param>
    /// <param name="text">表示する文字列（例: "+100"）</param>
    /// <param name="color">テキストのカラー</param>
    public void SpawnFloatingText(Vector2 position, string text, Color color)
    {
        if (floatingTextPrefab == null || effectContainer == null)
        {
            return;
        }

        // ポップアップテキストの生成
        GameObject textObj = Instantiate(floatingTextPrefab, effectContainer);
        RectTransform rectTrans = textObj.GetComponent<RectTransform>();

        if (rectTrans != null)
        {
            rectTrans.anchoredPosition = position;
        }

        // FloatingTextコンポーネントの設定
        FloatingText floatingText = textObj.GetComponent<FloatingText>();

        if (floatingText != null)
        {
            floatingText.Initialize(text, color);
        }
    }

    /// <summary>
    /// タップされた位置にきらめくタップエフェクト（波紋やスパークル）を生成する
    /// </summary>
    /// <param name="position">生成するキャンバス上のローカル座標</param>
    /// <param name="color">エフェクトのテーマカラー</param>
    public void SpawnTapEffect(Vector2 position, Color color)
    {
        if (tapEffectPrefab == null || effectContainer == null)
        {
            return;
        }

        // タップエフェクトオブジェクトの生成
        GameObject effectObj = Instantiate(tapEffectPrefab, effectContainer);
        RectTransform rectTrans = effectObj.GetComponent<RectTransform>();

        if (rectTrans != null)
        {
            rectTrans.anchoredPosition = position;
        }

        // エフェクトの色の適用（もしパーティクルまたはイメージの色を変更可能な場合）
        ParticleSystem particle = effectObj.GetComponent<ParticleSystem>();

        if (particle != null)
        {
            var main = particle.main;
            main.startColor = color;
        }

        else
        {
            UnityEngine.UI.Image img = effectObj.GetComponent<UnityEngine.UI.Image>();

            if (img != null)
            {
                img.color = color;
            }
        }

        // 一定時間後に自動破壊
        Destroy(effectObj, 0.8f);
    }
}