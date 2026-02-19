using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;
using HeneGames.DialogueSystem;

public class RhythmResultHandler : MonoBehaviour
{
    [Header("References")]
    public AudioSource musicSource;
    public RhythmSceneConnector connector;

    [Header("Scene Names")]
    public string lobbySceneName = "Lobby";

    [Header("Fail Panel")]
    public GameObject failPanel;
    public UnityEvent onFail;

    [Header("Pass Panel")]
    public GameObject passPanel;
    public TextMeshProUGUI scorePercentText;
    public TextMeshProUGUI rankText;
    public UnityEvent onPass;

    [Header("Optional Animators")]
    public ScoreCounterUI scoreCounter;
    public RankAnimator   rankAnimator;

    [Header("SFX")]
    [Tooltip("AudioSource to play short SFX (separate from music)")]
    public AudioSource sfxSource;
    [Tooltip("SFX when player passes the rhythm")] public AudioClip passSfx;
    [Tooltip("SFX when player fails the rhythm")] public AudioClip failSfx;

    [Header("Rank Thresholds (%)")]
    public float rankS = 90f;
    public float rankA = 75f;
    public float rankB = 60f;
    public float rankC = 45f;

    [Header("Rank Colors")]
    public Color colorS = new Color(1f,    0.84f, 0f);
    public Color colorA = new Color(0.35f, 0.75f, 1f);
    public Color colorB = new Color(0.4f,  0.9f,  0.4f);
    public Color colorC = new Color(1f,    0.65f, 0.1f);
    public Color colorD = new Color(0.85f, 0.25f, 0.25f);

    private bool _resultShown = false;

    private void Start()
    {
        failPanel?.SetActive(false);
        passPanel?.SetActive(false);
        // หมายเหตุ: ไม่เรียก SetUIVisibility(true) ตรงนี้
        // เพราะ score/combo UI ควรโชว์อยู่แล้ว และการเรียกตอน Start()
        // อาจไปรบกวน object อื่นที่อยู่ใต้ parent เดียวกัน
    }

    private bool _musicHasStarted = false;

    private void Update()
    {
        if (_resultShown) return;
        if (musicSource == null || ScoreManager.instance == null) return;

        // รอให้เพลงเริ่มเล่นจริงก่อน (กันช่วง countdown delay)
        if (!_musicHasStarted)
        {
            if (musicSource.isPlaying) _musicHasStarted = true;
            return;
        }

        // เพลงเริ่มแล้ว เช็คว่าหยุด = จบเพลง
        if (!musicSource.isPlaying)
            ShowResult();
    }

    public void ShowResult()
    {
        if (_resultShown) return;
        _resultShown = true;

        float scoreRatio   = ScoreManager.instance.ScoreRatio;
        float scorePercent = scoreRatio * 100f;
        bool  passed       = connector != null && connector.IsPassed(scoreRatio);

        if (passed) 
        { 
            // แก้ไข: ไม่เรียก connector.OnRhythmFinished(true) ตรงนี้แล้ว
            // ให้แค่โชว์ Panel แล้วรอคนกดปุ่ม
            ShowPassPanel(scorePercent); 
            onPass.Invoke(); 
        }
        else        
        { 
            ShowFailPanel();                                                
            onFail.Invoke(); 
        }
    }

    private void ShowPassPanel(float scorePercent)
    {
        // Hide in-game UI and floating texts while pass panel is shown
        ScoreManager.instance?.SetUIVisibility(false);
 
        passPanel?.SetActive(true);
        // เล่น SFX ของผ่านเกม ถ้ามี
        if (sfxSource != null && passSfx != null) sfxSource.PlayOneShot(passSfx);
        string rank  = GetRank(scorePercent);
        Color  color = GetRankColor(rank);

        if (scoreCounter != null) scoreCounter.CountTo(scorePercent);
        else if (scorePercentText != null) scorePercentText.text = $"{scorePercent:F1}%";

        if (rankText != null) { rankText.text = rank; rankText.color = color; }
        rankAnimator?.PlayRankReveal();

        Debug.Log($"[Result] PASS | {scorePercent:F1}% | Rank {rank}");
    }

    private void ShowFailPanel()
    {
        // Hide in-game UI and floating texts while fail panel is shown
        ScoreManager.instance?.SetUIVisibility(false);
 
        failPanel?.SetActive(true);
        // เล่น SFX ของแพ้เกม ถ้ามี
        if (sfxSource != null && failSfx != null) sfxSource.PlayOneShot(failSfx);
        Debug.Log("[Result] FAIL");
    }

    // ── Button Callbacks (Fail Panel) ──────────────────────────────────────

    /// <summary>ปุ่ม Retry (หน้า Fail)</summary>
    public void OnRetryYes() 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>ปุ่ม No / Back to Lobby (หน้า Fail)</summary>
    public void OnRetryNo()
    {
        connector?.OnRhythmFinished(false);
        SceneManager.LoadScene(lobbySceneName);
    }

    // ── Button Callbacks (Pass Panel) ──────────────────────────────────────

    /// <summary>
    /// [NEW] ปุ่ม Next (หน้า Win/Pass)
    /// ส่งค่า Finish = true กลับไป และโหลดหน้า Lobby
    /// </summary>
    public void OnPassNext()
    {
        // ส่งค่า true ตามที่ต้องการ
        connector?.OnRhythmFinished(true);
        SceneManager.LoadScene("Main");
    }

    /// <summary>
    /// [NEW] ปุ่ม Retry (หน้า Win/Pass)
    /// โหลด Scene เดิมซ้ำ (เล่นใหม่อีกรอบด้วยตัวละครเดิม)
    /// </summary>
    public void OnPassRetry()
    {
        // การ LoadScene เดิมซ้ำ จะเป็นการ Reset เกมแต่ยังคงค่า Static (เช่น ตัวละครที่เลือกไว้)
        // ขึ้นอยู่กับว่าคุณเก็บตัวละครไว้ที่ไหน ถ้าเก็บใน GameManager(DontDestroyOnLoad) ก็จะยังอยู่เหมือนเดิมครับ
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ── Rank Helpers ────────────────────────────────────────────────────

    private string GetRank(float percent)
    {
        if (percent >= rankS) return "S";
        if (percent >= rankA) return "A";
        if (percent >= rankB) return "B";
        if (percent >= rankC) return "C";
        return "D";
    }

    private Color GetRankColor(string rank) => rank switch
    {
        "S" => colorS,
        "A" => colorA,
        "B" => colorB,
        "C" => colorC,
        _   => colorD,
    };
}