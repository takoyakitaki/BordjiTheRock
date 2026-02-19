using UnityEngine;

namespace HeneGames.DialogueSystem
{
    /// <summary>
    /// ScriptableObject เก็บข้อมูลของ NPC แต่ละตัวในฉาก Bar
    /// สร้างได้จาก Assets > Dialogue System > New NPC Bar Data
    /// Progress เก็บแบบ runtime only (ไม่ save ข้าม session)
    /// </summary>
    [CreateAssetMenu(fileName = "New NPC Bar Data", menuName = "Dialogue System/New NPC Bar Data", order = 2)]
    public class NPCBarData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("ID ของ NPC นี้ — ต้องไม่ซ้ำกัน ใช้เช็ค dialogue trigger และส่งต่อไป Rhythm Scene")]
        public int npcID;

        [Tooltip("ชื่อแสดงผลของ NPC (ใช้ debug / UI)")]
        public string npcName;

        [Header("Rhythm Game")]
        [Tooltip("ชื่อ Scene ของ Rhythm Game ที่จะโหลดหลังจบ dialogue")]
        public string rhythmSceneName;

        [Header("Order")]
        [Tooltip("ลำดับที่ NPC นี้จะเข้ามาหาผู้เล่น (1 = คนแรกสุด)")]
        public int order;

        // ─── Runtime-only state ──────────────────────────────────────────────
        // ไม่ได้ serialize — reset ทุกครั้งที่เปิดเกมใหม่

        [System.NonSerialized]
        private bool _isDone = false;

        /// <summary>ทำเครื่องหมายว่าคุยกับ NPC นี้เสร็จแล้ว (runtime เท่านั้น)</summary>
        public void MarkAsDone() => _isDone = true;

        /// <summary>เช็คว่าคุยกับ NPC นี้จบแล้วหรือยัง</summary>
        public bool IsDone() => _isDone;

        /// <summary>รีเซ็ต state (ใช้สำหรับ debug หรือเริ่มรอบใหม่)</summary>
        public void ResetProgress() => _isDone = false;
    }
}