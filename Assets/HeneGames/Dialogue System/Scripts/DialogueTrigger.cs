using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HeneGames.DialogueSystem
{
    /// <summary>
    /// DialogueTrigger ที่รองรับระบบ NPC Bar
    /// - เก็บ NPCBarData เพื่อระบุตัวตน
    /// - ตรวจสอบลำดับผ่าน BarNPCSequencer ก่อนอนุญาต trigger
    /// - เมื่อ dialogue จบ → ส่งตัวเองไป BarNPCSequencer แล้วโหลด Rhythm Scene
    /// </summary>
    public class DialogueTrigger : MonoBehaviour
    {
        [Header("NPC Data")]
        [Tooltip("ScriptableObject ข้อมูลของ NPC ตัวนี้")]
        [SerializeField] private NPCBarData npcData;

        [Header("Events")]
        public UnityEvent startDialogueEvent;
        public UnityEvent nextSentenceDialogueEvent;

        [Tooltip("Fire เมื่อ dialogue จบ ก่อนโหลด Rhythm Scene")]
        public UnityEvent endDialogueEvent;

        // ─────────────────────────────────────────────────────────────────────

        public NPCBarData NPCData => npcData;

        /// <summary>เช็คว่า NPC ตัวนี้พร้อม trigger ได้ไหม</summary>
        public bool IsReadyToTrigger()
        {
            if (npcData == null)
            {
                Debug.LogWarning($"[DialogueTrigger] {gameObject.name} ไม่ได้กำหนด NPCBarData!");
                return false;
            }

            if (BarNPCSequencer.Instance == null)
            {
                Debug.LogWarning("[DialogueTrigger] ไม่พบ BarNPCSequencer ใน Scene!");
                return false;
            }

            return BarNPCSequencer.Instance.IsReadyToTrigger(npcData);
        }

        /// <summary>
        /// เรียกโดย DialogueManager เมื่อ dialogue จบ
        /// → บอก BarNPCSequencer ว่า NPC นี้กำลังจะไป Rhythm แล้วโหลด Scene
        /// </summary>
        public void OnDialogueFinished()
        {
            endDialogueEvent.Invoke();

            if (BarNPCSequencer.Instance == null) return;

            // ถ้ากำลัง returning จาก Rhythm
            if (BarNPCSequencer.Instance.IsReturningFromRhythm())
            {
                bool success = BarNPCSequencer.Instance.WasLastRhythmSuccessful();
                if (success)
                {
                    // ผ่านแล้ว → จบ NPC นี้และเปิดตัวถัดไป
                    BarNPCSequencer.Instance.CompleteCurrentNPC();
                }
                else
                {
                    // ไม่ผ่าน → หลัง dialogue แพ้เสร็จ กลับไปเล่น rhythm อีกรอบ
                    BarNPCSequencer.Instance.SetSkipDialogueAfterFail();
                    BarNPCSequencer.Instance.RetryCurrentNPC();
                }
            }
            else
            {
                // ครั้งแรก → Set pending และ Load Rhythm Scene
                BarNPCSequencer.Instance.SetPendingNPC(npcData);
                BarNPCSequencer.Instance.SetPendingStressRatio(NPCStressSystem.Instance?.StressRatio ?? 1f);
                SceneManager.LoadScene(npcData.rhythmSceneName);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (npcData == null) return;

            string doneStatus = Application.isPlaying ? (npcData.IsDone() ? "✓ Done" : "○ Pending") : "—";
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2f,
                $"NPC: {npcData.npcName}\nID: {npcData.npcID}  |  Order: {npcData.order}\n{doneStatus}"
            );
        }
#endif
    }
}