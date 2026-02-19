using UnityEngine;
using System;

public class Note : MonoBehaviour
{
    public float speed = 5f;
    public bool isHit = false;

    [Header("Long Note Settings")]
    public int noteType = 0;
    public float tailLength = 0f;
    public bool isBeingHeld = false;

    [Header("References")]
    public SpriteRenderer tailSprite;
    public BoxCollider2D noteCollider;

    public SpriteRenderer headSprite;

    // ── ข้อ 2: Callback แจ้ง HitZone ก่อนโน้ตถูก Destroy ──────────────
    // HitZone จะ subscribe เพื่อ ClearHoldState() ก่อนที่ reference จะกลายเป็น null
    public event Action<Note> OnNoteDestroyed;

    // ── ข้อ 1: Flag ป้องกัน double-miss ─────────────────────────────────
    // ใช้ flag แยกจาก isHit เพื่อให้ lock ได้เร็วที่สุด ก่อน Destroy จะมีผล
    private bool _isPendingDestroy = false;
    public bool IsPendingDestroy => _isPendingDestroy;

    // ─────────────────────────────────────────────────────────────────────

    public void SetupNote(int type, float duration, float spd)
    {
        noteType = type;
        speed    = spd;

        if (noteType == 1)
        {
            tailLength = duration * speed;

            if (tailSprite != null)
                tailSprite.size = new Vector2(tailLength, tailSprite.size.y);

            if (noteCollider != null)
            {
                noteCollider.size   = new Vector2(tailLength + 1f, noteCollider.size.y);
                noteCollider.offset = new Vector2(tailLength / 2f, 0f);
            }
        }
    }

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }

    public void HitNote()
    {
        if (_isPendingDestroy) return; // ── ป้องกัน double-call
        _isPendingDestroy = true;
        isHit = true;
        OnNoteDestroyed?.Invoke(this);
        Destroy(gameObject);
    }

    public void MissNote()
    {
        if (_isPendingDestroy) return; // ── ป้องกัน double-call
        _isPendingDestroy = true;
        // isHit ยังคงเป็น false → HitZone จะรู้ว่านี่คือ miss จริงๆ
        OnNoteDestroyed?.Invoke(this);
        Destroy(gameObject);
    }

    // ── ข้อ 2: Safety net — ถ้าโน้ตถูก Destroy โดยวิธีอื่น (scene reload ฯลฯ)
    private void OnDestroy()
    {
        // ถ้ายังไม่ได้ fire event (ไม่ได้เรียกผ่าน HitNote/MissNote)
        if (!_isPendingDestroy)
        {
            _isPendingDestroy = true;
            OnNoteDestroyed?.Invoke(this);
        }
    }
}