using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class NoteData 
{
    public float timeStamp;    // เวลาที่โน้ตเริ่มโผล่มา
    public int buttonType;     // 0 = ปุ่มซ้าย, 1 = ปุ่มขวา
    public int noteType;       // 0 = กดธรรมดา (Tap), 1 = กดค้าง (Hold)
    public float duration;     // ความยาวของการกดค้าง (ถ้าเป็น Tap จะเป็น 0)
}

[System.Serializable]
public class BeatmapData 
{
    public List<NoteData> notes = new List<NoteData>(); 
}

public class BeatmapRecorder : MonoBehaviour
{
    public AudioSource musicSource;
    public BeatmapData beatmap = new BeatmapData();

    // ตัวแปรสำหรับจำเวลาตอนเริ่มกดปุ่มซ้าย
    private float leftStartTime;
    private bool isLeftHolding;

    // ตัวแปรสำหรับจำเวลาตอนเริ่มกดปุ่มขวา
    private float rightStartTime;
    private bool isRightHolding;

    void Update()
    {
        // 1. ย้ายปุ่มเซฟมาไว้ด้านบนสุด! เพื่อให้กดเซฟได้ตลอดเวลาแม้เพลงจะจบไปแล้ว
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ExportBeatmap();
        }

        // 2. ค่อยเช็กว่าเพลงเล่นอยู่ไหม ถ้าไม่เล่น ก็ข้ามการอัดจังหวะด้านล่างไป
        if (!musicSource.isPlaying) return;

        // ================= ฝั่งปุ่มซ้าย =================
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.F))
        {
            leftStartTime = musicSource.time;
            isLeftHolding = true;
        }
        if ((Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.F)) && isLeftHolding)
        {
            RecordNote(0, leftStartTime, musicSource.time);
            isLeftHolding = false;
        }

        // ================= ฝั่งปุ่มขวา =================
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.J))
        {
            rightStartTime = musicSource.time;
            isRightHolding = true;
        }
        if ((Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.J)) && isRightHolding)
        {
            RecordNote(1, rightStartTime, musicSource.time);
            isRightHolding = false;
        }
    }

    void RecordNote(int btnType, float startTime, float endTime)
    {
        float holdDuration = endTime - startTime;
        NoteData newNote = new NoteData();
        newNote.timeStamp = startTime;
        newNote.buttonType = btnType;
        
        // ทริค: ถ้าเรากดแล้วปล่อยเร็วกว่า 0.15 วินาที ให้ระบบตีความว่าเป็นโน้ต "แตะธรรมดา"
        if (holdDuration < 0.15f) 
        {
            newNote.noteType = 0; // แตะธรรมดา
            newNote.duration = 0f;
        }
        else 
        {
            newNote.noteType = 1; // กดค้าง
            newNote.duration = holdDuration;
        }
        
        beatmap.notes.Add(newNote);
        
        string typeText = newNote.noteType == 0 ? "แตะปกติ" : "กดค้าง";
        Debug.Log($"บันทึก: ปุ่ม {btnType} | แบบ {typeText} | เวลา {startTime:F2}s | ยาว {newNote.duration:F2}s");
    }

    void ExportBeatmap()
    {
        string json = JsonUtility.ToJson(beatmap, true);
        string path = Application.dataPath + "/Beatmap_Song1.json";
        File.WriteAllText(path, json);
        Debug.Log("เซฟ Beatmap สำเร็จ! ไฟล์อยู่ที่: " + path);
    }
}