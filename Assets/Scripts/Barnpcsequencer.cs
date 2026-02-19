using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
// เพิ่ม namespace System.Linq เพื่อใช้ค้นหา
using System.Linq;

namespace HeneGames.DialogueSystem
{
    public class BarNPCSequencer : MonoBehaviour
    {
        public static BarNPCSequencer Instance { get; private set; }

        [System.Serializable]
        public struct NPCEntry
        {
            public NPCBarData data;
            public GameObject npcGameObject;
        }

        [Header("NPC ทั้งหมดในฉาก — เรียงตาม Order")]
        [SerializeField] private List<NPCEntry> npcEntries = new List<NPCEntry>();

        [Header("Events")]
        public UnityEvent onAllNPCsDone;

        private NPCBarData _pendingNPC;
        private float _pendingStressRatio = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ResetAllProgress();
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 1. ค้นหา DialogueTrigger ทั้งหมดใน Scene ใหม่
            var allTriggersInScene = FindObjectsOfType<DialogueTrigger>(true);

            // 2. Map GameObject กลับเข้าไปใน npcEntries โดยเทียบจาก NPCBarData
            for (int i = 0; i < npcEntries.Count; i++)
            {
                var entry = npcEntries[i]; // copy struct
                if (entry.data != null)
                {
                    // หาตัวที่มี data ตรงกัน
                    var foundTrigger = allTriggersInScene.FirstOrDefault(t => t.NPCData == entry.data);
                    if (foundTrigger != null)
                    {
                        entry.npcGameObject = foundTrigger.gameObject;
                        
                        // ต้องปิดไว้ก่อน เดี๋ยว ActivateNextNPC จะเป็นคนเปิดเอง
                        if (entry.npcGameObject != null)
                            entry.npcGameObject.SetActive(false); 
                    }
                    else
                    {
                        // ถ้าไม่เจอใน scene ใหม่ ให้ clear reference
                        entry.npcGameObject = null;
                    }
                }
                npcEntries[i] = entry; // update struct กลับเข้าไปใน list
            }

            ActivateNextNPC();
        }

        public void ActivateNextNPC()
        {
            if (_skipDialogueAfterFail)
            {
                SceneManager.LoadScene(_pendingNPC.rhythmSceneName);
                _skipDialogueAfterFail = false;
                return;
            }

            foreach (var e in npcEntries) 
            {
                if (e.npcGameObject != null)
                    e.npcGameObject.SetActive(false);
            }

            NPCEntry? next = null;
            int lowest = int.MaxValue;

            foreach (var e in npcEntries)
            {
                if (e.data == null || e.data.IsDone()) continue;
                if (e.data.order < lowest) { lowest = e.data.order; next = e; }
            }

            if (next.HasValue && next.Value.npcGameObject != null)
            {
                next.Value.npcGameObject.SetActive(true);
                Debug.Log($"[BarNPCSequencer] เปิด: {next.Value.data.npcName}");
            }
            else
            {
                Debug.Log("[BarNPCSequencer] NPC ทุกตัวจบแล้ว");
                onAllNPCsDone.Invoke();
            }
        }

        public bool IsReadyToTrigger(NPCBarData npc)
        {
            if (npc == null) return false;
            foreach (var e in npcEntries)
            {
                if (e.data == null || e.data == npc) continue;
                if (e.data.order < npc.order && !e.data.IsDone()) return false;
            }
            return true;
        }

        public void SetPendingNPC(NPCBarData npc) { _pendingNPC = npc; }
        public void SetPendingStressRatio(float ratio) { _pendingStressRatio = Mathf.Clamp01(ratio); }
        public NPCBarData GetPendingNPC() => _pendingNPC;
        public float GetPendingStressRatio() => _pendingStressRatio;


private bool _isReturningFromRhythm = false;
        private bool _lastRhythmSuccess = false;
        private bool _skipDialogueAfterFail = false;

public bool IsReturningFromRhythm() => _isReturningFromRhythm;
        public bool WasLastRhythmSuccessful() => _lastRhythmSuccess;
        public void OnRhythmFinished(bool success)
        {
            _isReturningFromRhythm = true;
            _lastRhythmSuccess = success;
        }
        public void SetSkipDialogueAfterFail() { _skipDialogueAfterFail = true; }
        public void CompleteCurrentNPC()
        {
            if (_pendingNPC != null)
            {
                _pendingNPC.MarkAsDone(); //
            }
            _pendingNPC = null;
            _isReturningFromRhythm = false;
            _lastRhythmSuccess = false;
            _skipDialogueAfterFail = false;
            
            // เรียกฟังก์ชันเพื่อปิดตัวเดิมและเปิดตัวถัดไปทันที
            ActivateNextNPC(); 
        }
        

        public void RetryCurrentNPC()
        {
            _isReturningFromRhythm = false;
            _lastRhythmSuccess = false;
            _skipDialogueAfterFail = false;
        }
        

        public void ResetAllProgress()
        {
            foreach (var e in npcEntries) e.data?.ResetProgress();
            _pendingNPC = null;
            _isReturningFromRhythm = false;
            _lastRhythmSuccess = false;
            _skipDialogueAfterFail = false;
        }

        public bool IsAllDone()
        {
            foreach (var e in npcEntries) if (e.data != null && !e.data.IsDone()) return false;
            return true;
        }

        /// <summary>
        /// คืนค่า NPCBarData ของ NPC ที่ active อยู่ใน Scene ปัจจุบัน (หรือ null)
        /// </summary>
        public NPCBarData GetActiveNPC()
        {
            foreach (var e in npcEntries)
            {
                if (e.npcGameObject != null && e.npcGameObject.activeInHierarchy)
                    return e.data;
            }
            return null;
        }
    }
}