using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeneGames.DialogueSystem
{
    [System.Serializable]
    public class DialogueBlock
    {
        [Header("─── BLOCK ───────────────────────────────────")]
        public List<NPC_Centence> sentences = new List<NPC_Centence>();
        public bool hasChoice = true;
        public List<DialogueChoice> choices = new List<DialogueChoice>();
    }

    [System.Serializable]
    public class DialogueChoice
    {
        [Tooltip("ข้อความบนปุ่ม")]
        public string choiceText;
        [Tooltip("ลดความเครียด NPC (บวก = ดี, ลบ = แย่)")]
        public int stressReduction = 20;
        [Tooltip("NPC พูดอะไรหลังผู้เล่นเลือก (ว่าง = ข้ามไป block ถัดไปเลย)")]
        [TextArea(2, 4)]
        public string npcReaction;
        [Tooltip("Block ถัดไปหลัง reaction (-1 = ถัดไปตามลำดับ)")]
        public int nextBlockIndex = -1;
    }

    public class NPCActivator : MonoBehaviour
    {
        [Header("1. ก่อนเล่น (Intro)")]
        [SerializeField] private List<DialogueBlock> dialogueBlocks = new List<DialogueBlock>();

        [Header("2. หลังเล่น: ผ่าน (Pass)")] 
        [SerializeField] private List<DialogueBlock> passDialogueBlocks = new List<DialogueBlock>(); // เปลี่ยนชื่อให้ชัดเจน

        [Header("3. หลังเล่น: ไม่ผ่าน (Fail)")] 
        [SerializeField] private List<DialogueBlock> failDialogueBlocks = new List<DialogueBlock>(); // ✅ เพิ่มช่องนี้

        private DialogueTrigger _trigger;

        private void OnEnable()
        {
            _trigger = GetComponent<DialogueTrigger>();
            if (_trigger == null || DialogueManager.Instance == null) return;

            if (!_trigger.IsReadyToTrigger())
            {
                // ดีเลย์การปิด เพื่อให้ NPC ตัวถัดไปนั่นมีเวลา activate ทันที
                Invoke(nameof(DelayedDeactivate), 0.01f);
                return;
            }

            NPCStressSystem.Instance?.BeginSession();

            // ตรวจสอบสถานะจาก Sequencer
            List<DialogueBlock> selectedBlocks = dialogueBlocks; // ค่า Default คือ Intro
            
            if (BarNPCSequencer.Instance != null && 
                BarNPCSequencer.Instance.GetPendingNPC() == _trigger.NPCData && 
                BarNPCSequencer.Instance.IsReturningFromRhythm())
            {
                // ถ้ากลับมาจากเกม เช็คว่า Pass หรือ Fail
                bool isSuccess = BarNPCSequencer.Instance.WasLastRhythmSuccessful();
                
                if (isSuccess)
                {
                    Debug.Log($"[NPCActivator] {_trigger.NPCData.npcName} : PASS Dialogue");
                    selectedBlocks = passDialogueBlocks;
                }
                else
                {
                    Debug.Log($"[NPCActivator] {_trigger.NPCData.npcName} : FAIL Dialogue");
                    // ถ้าไม่ได้ใส่ fail blocks ไว้ ให้ใช้ intro เดิมแทนกัน error
                    selectedBlocks = failDialogueBlocks.Count > 0 ? failDialogueBlocks : dialogueBlocks;
                }
            }

            DialogueManager.Instance.StartDialogueFromNPC(_trigger, selectedBlocks);
        }

        private void DelayedDeactivate()
        {
            gameObject.SetActive(false);
        }
    }
}