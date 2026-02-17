using UnityEngine;
using System.IO;

public class BeatmapSpawner : MonoBehaviour
{
    [Header("References")]
    public AudioSource musicSource;
    public GameObject blueNotePrefab;
    public GameObject redNotePrefab;
    
    [Header("Positions (จุดต่างๆ ในฉาก)")]
    public Transform leftSpawnPoint;  // จุดเกิดโน้ตฟ้า (ขวามือสุด)
    public Transform rightSpawnPoint; // จุดเกิดโน้ตแดง (ขวามือสุด)
    public Transform hitZoneButton;   // **ใหม่!** ลากปุ่มกด(สีฟ้าหรือแดงก็ได้) มาใส่ช่องนี้ เพื่อให้ระบบใช้วัดระยะทางออโต้

[Header("Settings")]
    public float noteSpeed = 5f;      
    
    [Tooltip("ชดเชยเวลาที่มนุษย์กดช้าตอนอัด (วินาที) เช่น 0.1 หรือ 0.15")]
    public float audioOffset = 0.1f; // <--- เพิ่มบรรทัดนี้

    private BeatmapData beatmap;
    private int noteIndex = 0;
    private float gameTimer = 0f;
    private float autoAdvanceTime = 0f; // เวลาล่วงหน้าที่ระบบคำนวณให้

    void Start()
    {
        // 1. บังคับปิด Play On Awake ผ่านโค้ดไปเลย เพื่อป้องกันบั๊กเพลงเล่นก่อน
        musicSource.playOnAwake = false;
        musicSource.Stop();

        // 2. พระเอกของเรา: คำนวณเวลาล่วงหน้าอัตโนมัติจากระยะทางจริงในฉาก! (สูตร: เวลา = ระยะทาง / ความเร็ว)
        float distance = Mathf.Abs(leftSpawnPoint.position.x - hitZoneButton.position.x);
        autoAdvanceTime = distance / noteSpeed;
        
        Debug.Log($"[ระบบออโต้] ระยะทางจริงคือ {distance} | โน้ตวิ่งด้วยความเร็ว {noteSpeed} | เพลงจะดีเลย์รอ {autoAdvanceTime} วินาที");

        LoadBeatmap();
        
        // 3. เริ่มนับเวลาเกมแบบติดลบ เผื่อเวลาให้โน้ตวิ่ง
        gameTimer = -autoAdvanceTime; 
        
        // 4. สั่งให้เพลงรอก่อน แล้วค่อยเล่น
        musicSource.PlayDelayed(autoAdvanceTime); 
    }

    void LoadBeatmap()
    {
        string path = Application.dataPath + "/Beatmap_Song1.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            beatmap = JsonUtility.FromJson<BeatmapData>(json);
        }
    }

    void Update()
    {
        if (beatmap == null || noteIndex >= beatmap.notes.Count) return;

        gameTimer += Time.deltaTime;

        // เสกโน้ตล่วงหน้าตามเวลาที่คำนวณได้
        while (noteIndex < beatmap.notes.Count && gameTimer >= (beatmap.notes[noteIndex].timeStamp - audioOffset) - autoAdvanceTime)
        {
            SpawnNote(beatmap.notes[noteIndex]);
            noteIndex++;
        }
    }

    void SpawnNote(NoteData data)
    {
        Transform spawnPoint = (data.buttonType == 0) ? leftSpawnPoint : rightSpawnPoint;
        GameObject prefabToSpawn = (data.buttonType == 0) ? blueNotePrefab : redNotePrefab;
        
        GameObject newNote = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
        
        Note noteScript = newNote.GetComponent<Note>();
        if (noteScript != null)
        {
            noteScript.speed = noteSpeed;
        }
    }
}