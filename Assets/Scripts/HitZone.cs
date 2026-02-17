using UnityEngine;

public class HitZone : MonoBehaviour
{
    [Header("Lane Settings (ตั้งค่าเลน)")]
    public string laneName; 
    public string expectedNoteTag; 

    [Header("Input Settings")]
    public KeyCode keyToPress; 
    
    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color defaultColor = Color.white;
    public Color pressedColor = Color.gray;

    [Header("Scoring Thresholds (ระยะคะแนน)")]
    public float perfectThreshold = 0.08f; 
    public float greatThreshold = 0.25f;   

    [Header("Debug Visuals (ตั้งค่าเส้นนำสายตา)")]
    [Tooltip("ปรับความสูงของเส้นกรอบแกน Y ในหน้า Scene ให้พอดีกับปุ่ม")]
    public float debugLineHeight = 1f; 

    private Note currentNoteInRange = null;

    void Update()
    {
        if (Input.GetKeyDown(keyToPress))
        {
            spriteRenderer.color = pressedColor;
            CheckHit();
        }
        if (Input.GetKeyUp(keyToPress))
        {
            spriteRenderer.color = defaultColor;
        }
    }

    void CheckHit()
    {
        if (currentNoteInRange != null)
        {
            float distance = Mathf.Abs(transform.position.x - currentNoteInRange.transform.position.x);
            
            if (distance <= perfectThreshold) 
            {
                Debug.Log($"<color=green>[{laneName}] Perfect!</color> ระยะ: {distance:F2}");
            }
            else if (distance <= greatThreshold) 
            {
                Debug.Log($"<color=yellow>[{laneName}] Great!</color> ระยะ: {distance:F2}");
            }
            else 
            {
                Debug.Log($"<color=red>[{laneName}] Miss (กดเร็ว/ช้าไป)!</color> ระยะ: {distance:F2}");
            }

            currentNoteInRange.HitNote();
            currentNoteInRange = null; 
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(expectedNoteTag))
        {
            currentNoteInRange = other.GetComponent<Note>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(expectedNoteTag))
        {
            Note outNote = other.GetComponent<Note>();
            
            // เพิ่มเงื่อนไข !outNote.isHit เพื่อเช็กว่าโน้ตตัวนี้ต้อง "ยังไม่เคยถูกกดติด" ถึงจะถือว่า Miss
            if (outNote != null && !outNote.isHit && outNote == currentNoteInRange)
            {
                Debug.Log($"<color=red>[{laneName}] Miss (ปล่อยหลุด)!</color>");
                currentNoteInRange = null;
                outNote.MissNote();
            }
        }
    }

    // ==========================================
    // ตัววาดเส้น Debug ในหน้า Scene
    // ==========================================
    private void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position;

        // 1. เส้นสีเขียว = Perfect (ใช้ความสูงจาก debugLineHeight)
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, new Vector3(perfectThreshold * 2, debugLineHeight, 0));

        // 2. เส้นสีเหลือง = Great (ใช้ความสูงจาก debugLineHeight)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, new Vector3(greatThreshold * 2, debugLineHeight, 0));

        // 3. เส้นสีฟ้า = ขนาดของ BoxCollider2D (ตัวรับการชน)
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.color = Color.cyan;
            // เส้นสีฟ้าจะสูงกว่าเส้นอื่นนิดหน่อย (+0.5f) เพื่อให้เห็นขอบเขตชัดเจนว่าใหญ่กว่า
            Gizmos.DrawWireCube(center + (Vector3)col.offset, new Vector3(col.size.x, debugLineHeight + 0.5f, 0));
        }
    }
}