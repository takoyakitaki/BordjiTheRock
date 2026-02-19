using UnityEngine;
using System.Collections.Generic;

public class HitZone : MonoBehaviour
{
    [Header("Lane Settings")]
    public string laneName;
    public string expectedNoteTag;

    [Header("Input Settings")]
    public KeyCode keyToPress;
    public KeyCode alternativeKey;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color defaultColor = Color.white;
    public Color pressedColor = Color.gray;

    [Header("Judgement")]
    public JudgementSpawner judgementSpawner;

    [Header("Scoring Thresholds")]
    public float perfectThreshold = 0.08f;
    public float greatThreshold   = 0.25f;

    [Header("Early Press Dead Zone")]
    [Tooltip("กดตอนโน้ตห่างกว่านี้ → เมิน ไม่นับ Miss")]
    public float earlyPressDeadZone = 0.6f;

    [Header("Debug Visuals")]
    public float debugLineHeight = 1f;

    [Header("Hit Effect")]
    public HitEffectController hitEffect;

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip   hitSound;        // เสียงตอนกด Perfect / Great
    public AudioClip   missSound;       // เสียงตอน Miss (ถ้าไม่มีปล่อยว่างได้)
    [Range(0f, 1f)]
    public float hitVolume  = 1f;
    [Range(0f, 1f)]
    public float missVolume = 0.6f;

    // ─── Note Queue ──────────────────────────────────────────────────────
    private List<Note> notesInRange = new List<Note>();

    // ─── Long Note state ─────────────────────────────────────────────────
    private Note currentLongNote   = null;
    private bool isHoldingLongNote = false;

    // ─────────────────────────────────────────────────────────────────────

    void Update()
    {
        bool isKeyDown = Input.GetKeyDown(keyToPress) || Input.GetKeyDown(alternativeKey);
        bool isKeyHeld = Input.GetKey(keyToPress)     || Input.GetKey(alternativeKey);
        bool isKeyUp   = Input.GetKeyUp(keyToPress)   || Input.GetKeyUp(alternativeKey);

        // 1. กดหัวโน้ต → Effect และเสียงจะเรียกใน CheckHit() เพื่อไม่ให้ซ้ำ
        if (isKeyDown)
        {
            spriteRenderer.color = pressedColor;
            CheckHit();
        }

        // 2. กดค้าง Long Note
        if (isKeyHeld && isHoldingLongNote && currentLongNote != null)
        {
            float tailEndX = currentLongNote.transform.position.x + currentLongNote.tailLength;
            if (tailEndX <= transform.position.x)
            {
                Debug.Log($"<color=cyan>[{laneName}] Long Note Cleared!</color>");
                ScoreManager.instance.AddScore(ScoreManager.instance.longNoteClearScore);
                ScoreManager.instance.AddCombo();
                PlayHitSound();
                hitEffect?.PlayHitEffect();
                currentLongNote.HitNote();
                judgementSpawner.Spawn("Great", currentLongNote.transform.position + Vector3.up * 0.5f);
            }
        }

        // 3. ปล่อยนิ้ว
        if (isKeyUp)
        {
            spriteRenderer.color = defaultColor;
            if (isHoldingLongNote && currentLongNote != null)
            {
                Debug.Log($"<color=red>[{laneName}] Miss! ปล่อยนิ้วเร็วกว่ากำหนด</color>");
                PlayMissSound();
                currentLongNote.MissNote();
                ScoreManager.instance.ResetCombo();
                ScoreManager.instance.RegisterMiss();
                judgementSpawner.Spawn("Miss", currentLongNote.transform.position + Vector3.up * 0.5f);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────

    void CheckHit()
    {
        if (isHoldingLongNote) return;

        Note target = GetClosestNote();
        if (target == null) return;

        float distance = Mathf.Abs(transform.position.x - target.transform.position.x);

        // Dead Zone: ยังไม่ถึงเขตกด → เมิน
        if (distance > earlyPressDeadZone)
        {
            Debug.Log($"<color=grey>[{laneName}] กดเร็วเกิน (ห่าง {distance:F2}) → เมิน</color>");
            return;
        }

        // ── ตัดสินผล ──────────────────────────────────────────────────────
        if (distance <= perfectThreshold)
        {
            Debug.Log($"<color=green>[{laneName}] Perfect!</color>");
            ScoreManager.instance.AddScore(ScoreManager.instance.perfectScore);
            ScoreManager.instance.AddCombo();
            PlayHitSound();
            hitEffect?.PlayHitEffect();
            judgementSpawner.Spawn("Perfect", target.transform.position + Vector3.up * 0.5f);
        }
        else if (distance <= greatThreshold)
        {
            Debug.Log($"<color=yellow>[{laneName}] Great!</color>");
            ScoreManager.instance.AddScore(ScoreManager.instance.greatScore);
            ScoreManager.instance.AddCombo();
            PlayHitSound();
            hitEffect?.PlayHitEffect();
            judgementSpawner.Spawn("Great", target.transform.position + Vector3.up * 0.5f);
        }
        else
        {
            Debug.Log($"<color=red>[{laneName}] Miss (จังหวะผิด)!</color>");
            ScoreManager.instance.ResetCombo();
            ScoreManager.instance.RegisterMiss();
            PlayMissSound();
            // ไม่เล่น hitEffect ตอน Miss
            judgementSpawner.Spawn("Miss", target.transform.position + Vector3.up * 0.5f);
        }

        notesInRange.Remove(target);

        if (target.noteType == 1)
        {
            isHoldingLongNote           = true;
            currentLongNote             = target;
            currentLongNote.isBeingHeld = true;
            if (currentLongNote.headSprite != null)
                currentLongNote.headSprite.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            if (currentLongNote.tailSprite != null)
                currentLongNote.tailSprite.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        }
        else
        {
            target.HitNote();
        }
    }

    // ─── Sound Helpers ────────────────────────────────────────────────────

    void PlayHitSound()
    {
        if (audioSource != null && hitSound != null)
            audioSource.PlayOneShot(hitSound, hitVolume);
    }

    void PlayMissSound()
    {
        if (audioSource != null && missSound != null)
            audioSource.PlayOneShot(missSound, missVolume);
    }

    // ─── Trigger callbacks ───────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(expectedNoteTag)) return;
        Note n = other.GetComponent<Note>();
        if (n == null || notesInRange.Contains(n)) return;

        notesInRange.Add(n);
        n.OnNoteDestroyed += HandleNoteDestroyed;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(expectedNoteTag)) return;
        Note outNote = other.GetComponent<Note>();
        if (outNote == null) return;

        if (!outNote.isHit && !outNote.isBeingHeld && !outNote.IsPendingDestroy)
        {
            Debug.Log($"<color=red>[{laneName}] Miss (ปล่อยหลุด)!</color>");
            PlayMissSound();
            outNote.MissNote();
            ScoreManager.instance.ResetCombo();
            ScoreManager.instance.RegisterMiss();
            judgementSpawner.Spawn("Miss", outNote.transform.position + Vector3.up * 0.5f);
        }

        notesInRange.Remove(outNote);
    }

    private void HandleNoteDestroyed(Note note)
    {
        note.OnNoteDestroyed -= HandleNoteDestroyed;

        if (note == currentLongNote)
        {
            if (!note.isHit && isHoldingLongNote)
            {
                Debug.Log($"<color=red>[{laneName}] Long Note destroyed mid-hold → Miss</color>");
                ScoreManager.instance.ResetCombo();
                ScoreManager.instance.RegisterMiss();
                judgementSpawner.Spawn("Miss", note.transform.position + Vector3.up * 0.5f);
            }

            ClearHoldState();
        }

        notesInRange.Remove(note);
    }

    void ClearHoldState()
    {
        if (currentLongNote != null)
        {
            if (currentLongNote.headSprite != null)
                currentLongNote.headSprite.color = Color.white;
            if (currentLongNote.tailSprite != null)
                currentLongNote.tailSprite.color = Color.white;
        }
        isHoldingLongNote = false;
        currentLongNote   = null;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private Note GetClosestNote()
    {
        Note closest  = null;
        float minDist = float.MaxValue;

        for (int i = notesInRange.Count - 1; i >= 0; i--)
        {
            Note n = notesInRange[i];

            if (n == null || n.IsPendingDestroy)
            {
                notesInRange.RemoveAt(i);
                continue;
            }

            float d = Mathf.Abs(transform.position.x - n.transform.position.x);
            if (d < minDist) { minDist = d; closest = n; }
        }

        return closest;
    }

    // ─── Debug Gizmos ────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, new Vector3(perfectThreshold * 2, debugLineHeight, 0));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, new Vector3(greatThreshold * 2, debugLineHeight, 0));

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireCube(center, new Vector3(earlyPressDeadZone * 2, debugLineHeight + 0.2f, 0));

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(center + (Vector3)col.offset,
                new Vector3(col.size.x, debugLineHeight + 0.5f, 0));
        }
    }
}