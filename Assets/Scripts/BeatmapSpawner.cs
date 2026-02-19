using UnityEngine;
using System.IO;

public class BeatmapSpawner : MonoBehaviour
{
    [Header("References")]
    public AudioSource musicSource;
    public GameObject blueNotePrefab;
    public GameObject redNotePrefab;
    public GameObject blueLongNotePrefab;
    public GameObject redLongNotePrefab;

    [Header("Positions")]
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;
    public Transform hitZoneButton;

    [Header("Settings")]
    public float noteSpeed   = 5f;
    [Tooltip("ชดเชยเวลาที่มนุษย์กดช้าตอนอัด (วินาที)")]
    public float audioOffset = 0.1f;

    [Header("Beatmap")]
    [Tooltip("ชื่อไฟล์ (ไม่มี .json) ใน Assets/beatmaps/")]
    public string beatmapFileName = "Beatmap_Song1";

    [Header("Countdown")]
    public RhythmCountdown countdownUI;
    [Tooltip("เปิด/ปิด การนับถอยหลังก่อนเล่น")]
    public bool enableCountdown = true;

    // ─── Internal ────────────────────────────────────────────────────────
    private BeatmapData beatmap;
    private int   noteIndex       = 0;
    private float gameTimer       = 0f;
    private float autoAdvanceTime = 0f;
    private bool  _musicStarted   = false;

    // ─────────────────────────────────────────────────────────────────────

    void Start()
    {
        musicSource.playOnAwake = false;
        musicSource.Stop();

        float distance = Mathf.Abs(leftSpawnPoint.position.x - hitZoneButton.position.x);
        autoAdvanceTime = distance / noteSpeed;

        Debug.Log($"[Spawner] ระยะทาง {distance:F2} | Speed {noteSpeed} | Delay {autoAdvanceTime:F2}s");

        LoadBeatmap(beatmapFileName);

        // ── ข้อ 3: Pre-calculate max score ทันทีหลัง load ───────────────
        if (beatmap != null && ScoreManager.instance != null)
            ScoreManager.instance.PreCalculateMaxScore(beatmap);

        gameTimer = -autoAdvanceTime;

        // Start countdown or music directly
        if (enableCountdown && countdownUI != null)
        {
            // Start countdown, then trigger music start
            countdownUI.StartCountdown(OnCountdownComplete);
        }
        else
        {
            // No countdown, start music immediately
            musicSource.PlayDelayed(autoAdvanceTime);
            _musicStarted = true;
        }
    }

    private void OnCountdownComplete()
    {
        // After countdown finishes, start the music
        musicSource.PlayDelayed(autoAdvanceTime);
        _musicStarted = true;
        Debug.Log("[BeatmapSpawner] Countdown complete, music starting...");
    }

    void LoadBeatmap(string fileName)
{
    string path = Path.Combine(Application.streamingAssetsPath, fileName + ".json");

    if (File.Exists(path))
    {
        string json = File.ReadAllText(path);
        beatmap = JsonUtility.FromJson<BeatmapData>(json);
        Debug.Log($"[Spawner] โหลด Beatmap: {path} ({beatmap.notes.Count} notes)");
    }
    else
    {
        Debug.LogError($"[Spawner] ไม่พบไฟล์: {path}");
    }
}

    void Update()
    {
        if (beatmap == null || noteIndex >= beatmap.notes.Count || !_musicStarted) return;

        gameTimer += Time.deltaTime;

        while (noteIndex < beatmap.notes.Count &&
               gameTimer >= (beatmap.notes[noteIndex].timeStamp - audioOffset) - autoAdvanceTime)
        {
            SpawnNote(beatmap.notes[noteIndex]);
            noteIndex++;
        }
    }

    void SpawnNote(NoteData data)
    {
        Transform spawnPoint = (data.buttonType == 0) ? leftSpawnPoint : rightSpawnPoint;

        GameObject prefabToSpawn;
        if (data.noteType == 0)
            prefabToSpawn = (data.buttonType == 0) ? blueNotePrefab : redNotePrefab;
        else
            prefabToSpawn = (data.buttonType == 0) ? blueLongNotePrefab : redLongNotePrefab;

        GameObject newNote = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);

        Note noteScript = newNote.GetComponent<Note>();
        if (noteScript != null)
            noteScript.SetupNote(data.noteType, data.duration, noteSpeed);
    }
}