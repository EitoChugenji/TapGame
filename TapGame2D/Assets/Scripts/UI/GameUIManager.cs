using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲームプレイ画面のUI表示（スコア、タイマー、コンボ、一時停止など）を更新・制御するクラス
/// </summary>
public class GameUIManager : MonoBehaviour
{
    [Header("Main HUD")]
    [SerializeField]
    private Text scoreText;

    [SerializeField]
    private Text timerText;

    [SerializeField]
    private Text comboText;

    [Header("GameOver Overlay")]
    [SerializeField]
    private GameObject gameOverPanel;

    [Header("Pause Overlay")]
    [SerializeField]
    private GameObject pausePanel;

    [SerializeField]
    private Button pauseButton;

    [SerializeField]
    private Button resumeButton;

    private bool isPaused = false;

    private void Start()
    {
        // 初期UI状態の設定
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // ボタンイベントの登録
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeButtonClicked);
        }
    }

    /// <summary>
    /// GameManagerから毎フレーム呼び出され、HUDテキストを最新状態にする
    /// </summary>
    /// <param name="score">現在のスコア</param>
    /// <param name="time">残り時間 (秒)</param>
    /// <param name="combo">現在のコンボ数</param>
    public void UpdateUI(int score, float time, int combo)
    {
        // スコア表示の更新
        if (scoreText != null)
        {
            scoreText.text = "SCORE\n" + score.ToString("N0");
        }

        // 残り時間表示の更新 (小数点以下1桁まで表示)
        if (timerText != null)
        {
            timerText.text = "TIME\n" + time.ToString("F1") + "s";
            
            // 残り時間が少なくなったらテキストを赤くする演出
            if (time <= 5.0f)
            {
                timerText.color = Color.red;
            }

            else
            {
                timerText.color = Color.white;
            }
        }

        // コンボ表示の更新
        if (comboText != null)
        {
            if (combo > 1)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = combo.ToString() + " COMBO!";
            }

            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 30秒終了時にゲームオーバーパネルを表示する
    /// </summary>
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 一時停止ボタンを押せなくする
        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 一時停止ボタンが押された時の処理
    /// </summary>
    private void OnPauseButtonClicked()
    {
        if (isPaused)
        {
            return;
        }

        isPaused = true;
        Time.timeScale = 0f; // 時間の流れを停止

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    /// <summary>
    /// 再開ボタンが押された時の処理
    /// </summary>
    private void OnResumeButtonClicked()
    {
        if (!isPaused)
        {
            return;
        }

        isPaused = false;
        Time.timeScale = 1f; // 時間の流れを通常に戻す

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }
}