using UnityEngine;

/// <summary>
/// スコア、コンボ、およびハイスコアのデータを管理するクラス
/// </summary>
public class ScoreManager : MonoBehaviour
{
    /// <summary>
    /// 現在のプレイのスコア
    /// </summary>
    public int CurrentScore { get; private set; }

    /// <summary>
    /// 現在の連続タップコンボ数
    /// </summary>
    public int ComboCount { get; private set; }

    /// <summary>
    /// 自己最高スコア
    /// </summary>
    public int HighScore { get; private set; }

    private void Awake()
    {
        // 起動時に既存のハイスコアデータをロードする
        HighScore = SaveManager.LoadHighScore();
    }

    /// <summary>
    /// ゲーム開始時にスコアとコンボ数を初期値に戻す
    /// </summary>
    public void ResetScore()
    {
        CurrentScore = 0;
        ComboCount = 0;
    }

    /// <summary>
    /// オブジェクトがタップされた際にスコアを加算し、コンボ数を増加させる
    /// </summary>
    /// <param name="baseAmount">オブジェクトの基本得点</param>
    public void AddScore(int baseAmount)
    {
        // 減点アイテム（爆弾など）の処理
        if (baseAmount < 0)
        {
            CurrentScore += baseAmount;
            
            // スコアがマイナスにならないように0に丸める
            if (CurrentScore < 0)
            {
                CurrentScore = 0;
            }
            
            // 減点時はコンボをリセットする
            ResetCombo();
            return;
        }

        // 通常・レアの加算処理
        ComboCount++;
        
        // コンボボーナス（コンボ数に応じて追加点、最大+50）
        int comboBonus = Mathf.Min(ComboCount, 50);
        int finalScore = baseAmount + comboBonus;
        
        CurrentScore += finalScore;
    }

    /// <summary>
    /// 制限時間内にタップできずに自動消滅した際など、コンボが途切れた時にコンボ数をリセットする
    /// </summary>
    public void ResetCombo()
    {
        ComboCount = 0;
    }

    /// <summary>
    /// ゲーム終了時にハイスコア判定を行い、更新があれば保存する
    /// </summary>
    /// <returns>ハイスコアが更新された場合はtrue</returns>
    public bool CheckAndSaveHighScore()
    {
        if (CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
            SaveManager.SaveHighScore(HighScore);
            return true;
        }

        return false;
    }
}