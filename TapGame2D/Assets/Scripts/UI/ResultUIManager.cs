using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 結果画面（Result）のUI表示およびシーン遷移を制御するクラス
/// </summary>
public class ResultUIManager : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField]
    private Text finalScoreText;

    [SerializeField]
    private Text highScoreText;

    [SerializeField]
    private GameObject newRecordPanel;

    [Header("Button References")]
    [SerializeField]
    private Button retryButton;

    [SerializeField]
    private Button titleButton;

    private void Start()
    {
        // 1. スコアデータの読み込みと表示
        int finalScore = PlayerPrefs.GetInt("LastScore", 0);
        int highScore = SaveManager.LoadHighScore();
        bool isNewRecord = PlayerPrefs.GetInt("IsNewRecord", 0) == 1;

        if (finalScoreText != null)
        {
            finalScoreText.text = "SCORE: " + finalScore.ToString("N0");
        }

        if (highScoreText != null)
        {
            highScoreText.text = "HIGH SCORE: " + highScore.ToString("N0");
        }

        // 新記録フラグがある場合、お祝い用のバッジやテキストを有効化
        if (newRecordPanel != null)
        {
            newRecordPanel.SetActive(isNewRecord);
        }

        // 2. ボタンイベントの登録
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryButtonClicked);
        }

        if (titleButton != null)
        {
            titleButton.onClick.AddListener(OnTitleButtonClicked);
        }
    }

    /// <summary>
    /// もう一度遊ぶ（RETRY）ボタンが押された時の処理
    /// </summary>
    private void OnRetryButtonClicked()
    {
        DisableButtons();
        PlayClickSound();

        // フェード遷移でGameシーンへ
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.TransitionToScene("Game");
        }
    }

    /// <summary>
    /// タイトルに戻る（TITLE）ボタンが押された時の処理
    /// </summary>
    private void OnTitleButtonClicked()
    {
        DisableButtons();
        PlayClickSound();

        // フェード遷移でTitleシーンへ
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.TransitionToScene("Title");
        }
    }

    /// <summary>
    /// ボタンの多重クリックを防止する
    /// </summary>
    private void DisableButtons()
    {
        if (retryButton != null)
        {
            retryButton.interactable = false;
        }

        if (titleButton != null)
        {
            titleButton.interactable = false;
        }
    }

    /// <summary>
    /// ボタンクリック時の簡易SE再生
    /// </summary>
    private void PlayClickSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE("se_game_start");
        }
    }
}