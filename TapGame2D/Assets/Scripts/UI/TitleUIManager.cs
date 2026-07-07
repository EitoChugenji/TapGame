using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// タイトル画面のUI操作および遷移イベントを制御するクラス
/// </summary>
public class TitleUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private Button startButton;

    [SerializeField]
    private Button resetScoreButton;

    [SerializeField]
    private Text highScoreText;

    private void Start()
    {
        // 1. ハイスコアの読み込みと表示
        int highScore = SaveManager.LoadHighScore();
        if (highScoreText != null)
        {
            highScoreText.text = "HIGH SCORE: " + highScore.ToString("N0");
        }

        // 2. ボタンイベントの登録
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (resetScoreButton != null)
        {
            resetScoreButton.onClick.AddListener(OnResetScoreButtonClicked);
        }
    }

    /// <summary>
    /// スタートボタンが押された際の処理
    /// </summary>
    private void OnStartButtonClicked()
    {
        // 多重タップ防止のためにボタンを無効化
        if (startButton != null)
        {
            startButton.interactable = false;
        }

        // 開始効果音を再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE("se_game_start");
        }

        // フェードを伴うシーン遷移（Gameシーンへ）
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.TransitionToScene("Game");
        }
    }

    /// <summary>
    /// ハイスコアリセットボタンが押された際の処理
    /// </summary>
    private void OnResetScoreButtonClicked()
    {
        // セーブデータの削除
        SaveManager.DeleteSaveData();

        // UI表示の更新
        if (highScoreText != null)
        {
            highScoreText.text = "HIGH SCORE: 0";
        }

        // タップ効果音を再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE("se_tap");
        }
    }
}