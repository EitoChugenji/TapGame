using UnityEngine;

/// <summary>
/// ゲーム全体のBGMおよびSE（効果音）の再生を管理するシングルトンクラス
/// </summary>
public class SoundManager : MonoBehaviour
{
    /// <summary>
    /// シングルトンインスタンス
    /// </summary>
    public static SoundManager Instance { get; private set; }

    [SerializeField]
    private AudioSource bgmSource;

    [SerializeField]
    private AudioSource seSource;

    // 仮の効果音・BGM用オーディオクリップ
    private AudioClip tapSoundClip;
    private AudioClip rareTapSoundClip;
    private AudioClip bombSoundClip;
    private AudioClip gameOverSoundClip;
    private AudioClip gameStartSoundClip;
    private AudioClip bgmSoundClip;

    private void Awake()
    {
        // シングルトンの初期化と重複排除
        if (Instance == null)
        {
            Instance = this;

            // DontDestroyOnLoadはルートオブジェクトである必要があるため、親から切り離す
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            GenerateProceduralSounds();
        }

        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// AudioSourceコンポーネントの初期化と設定
    /// </summary>
    private void InitializeAudioSources()
    {
        // コンポーネントがアタッチされていない場合は自動追加
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }

        if (seSource == null)
        {
            seSource = gameObject.AddComponent<AudioSource>();
        }

        // BGMはループ再生を有効にする
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        seSource.playOnAwake = false;
    }

    /// <summary>
    /// 外部の音声ファイルがなくても動作するように、プログラム上で効果音とBGM用のサイン波クリップを生成する
    /// </summary>
    private void GenerateProceduralSounds()
    {
        tapSoundClip = CreateToneClip(523.25f, 0.1f);        // C5 (ド) - 通常タップ音
        rareTapSoundClip = CreateToneClip(1046.50f, 0.15f);  // C6 (高いド) - レアタップ音
        bombSoundClip = CreateToneClip(150.0f, 0.25f);       // 低音ノイズ風 - 爆弾タップ音
        gameOverSoundClip = CreateToneClip(220.0f, 0.8f);    // A3 (ラ) - ゲーム終了音
        gameStartSoundClip = CreateToneClip(880.0f, 0.4f);   // A5 - ゲーム開始音

        // 簡易的なループBGM（サイン波のメロディ）
        bgmSoundClip = CreateProceduralBgmClip();
    }

    /// <summary>
    /// 指定された周波数と時間のサイン波オーディオクリップを作成する
    /// </summary>
    /// <param name="frequency">周波数 (Hz)</param>
    /// <param name="duration">再生時間 (秒)</param>
    /// <returns>生成されたAudioClip</returns>
    private AudioClip CreateToneClip(float frequency, float duration)
    {
        const int SampleRate = 44100;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / SampleRate;
            // 単純なサイン波を生成し、フェードアウトをかけることでプチプチ音を防止
            float envelope = Mathf.Clamp01(1.0f - (time / duration));
            samples[i] = Mathf.Sin(2.0f * Mathf.PI * frequency * time) * 0.5f * envelope;
        }

        AudioClip clip = AudioClip.Create("ProceduralTone", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// 簡易的なBGM用のサイン波メロディクリップを作成する
    /// </summary>
    /// <returns>生成されたAudioClip</returns>
    private AudioClip CreateProceduralBgmClip()
    {
        const int SampleRate = 44100;
        const float Tempo = 120.0f; // BPM
        float beatDuration = 60.0f / Tempo; // 1拍の時間
        float[] notes = new float[] { 261.63f, 329.63f, 392.00f, 523.25f, 440.00f, 392.00f, 349.23f, 329.63f }; // ドミソドラソファミ
        float duration = beatDuration * notes.Length;
        int sampleCount = (int)(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / SampleRate;
            int noteIndex = (int)(time / beatDuration) % notes.Length;
            float frequency = notes[noteIndex];
            float noteTime = time % beatDuration;
            
            // 各音符にエンベロープを適用して滑らかにつなぐ
            float envelope = Mathf.Clamp01(1.0f - (noteTime / beatDuration) * 0.2f);
            samples[i] = Mathf.Sin(2.0f * Mathf.PI * frequency * time) * 0.15f * envelope;
        }

        AudioClip clip = AudioClip.Create("ProceduralBgm", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// 効果音を再生する
    /// </summary>
    /// <param name="seName">効果音の種類を示す名前 (se_tap, se_rare_tap, se_bomb, se_game_over, se_game_start)</param>
    public void PlaySE(string seName)
    {
        AudioClip clip = null;

        // 指定された名前に対応するクリップを選択
        switch (seName)
        {
            case "se_tap":
                clip = tapSoundClip;
                break;

            case "se_rare_tap":
                clip = rareTapSoundClip;
                break;

            case "se_bomb":
                clip = bombSoundClip;
                break;

            case "se_game_over":
                clip = gameOverSoundClip;
                break;

            case "se_game_start":
                clip = gameStartSoundClip;
                break;
        }

        if (clip != null && seSource != null)
        {
            seSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// BGMを再生する
    /// </summary>
    public void PlayBGM()
    {
        if (bgmSource != null && bgmSoundClip != null)
        {
            if (bgmSource.clip != bgmSoundClip)
            {
                bgmSource.clip = bgmSoundClip;
            }

            if (!bgmSource.isPlaying)
            {
                bgmSource.Play();
            }
        }
    }

    /// <summary>
    /// BGMの再生を停止する
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }
}