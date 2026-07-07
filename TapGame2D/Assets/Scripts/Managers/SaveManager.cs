using UnityEngine;

/// <summary>
/// データの保存と読み込み（セーブ＆ロード）を処理するクラス
/// </summary>
public static class SaveManager
{
    // ハイスコア保存用のキー定数
    private const string HighScoreKey = "HighScore";

    /// <summary>
    /// ハイスコアを保存する
    /// </summary>
    /// <param name="score">保存するスコア</param>
    public static void SaveHighScore(int score)
    {
        // 現在のハイスコアをロードして比較する
        int currentHighScore = LoadHighScore();
        
        // 取得したスコアが既存のハイスコアを上回っている場合のみ上書きする
        if (score > currentHighScore)
        {
            PlayerPrefs.SetInt(HighScoreKey, score);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// ハイスコアをロードする
    /// </summary>
    /// <returns>読み込まれたハイスコア。未保存の場合は0</returns>
    public static int LoadHighScore()
    {
        // キーが存在しない場合はデフォルト値として0を返す
        return PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    /// <summary>
    /// セーブデータをリセットする
    /// </summary>
    public static void DeleteSaveData()
    {
        PlayerPrefs.DeleteKey(HighScoreKey);
        PlayerPrefs.Save();
    }
}