using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HeneGames.DialogueSystem
{
    public class RhythmSceneConnector : MonoBehaviour
    {
        [SerializeField] private string barSceneName = "BarScene";

        [Header("Events")]
        public UnityEvent<float> onRhythmStart;
        public UnityEvent onRhythmComplete;

        public float RequiredPassPercent { get; private set; }

        private void Start()
        {
            if (BarNPCSequencer.Instance == null)
            {
                Debug.LogWarning("[RhythmSceneConnector] ไม่พบ BarNPCSequencer!");
                RequiredPassPercent = 1f;
                return;
            }

            RequiredPassPercent = BarNPCSequencer.Instance.GetPendingStressRatio();
            Debug.Log($"[RhythmSceneConnector] Required: {RequiredPassPercent:P0}");
            onRhythmStart.Invoke(RequiredPassPercent);
        }

        public void OnRhythmFinished(bool success)
        {
            // ส่งผลลัพธ์ไปเก็บที่ Sequencer
            BarNPCSequencer.Instance?.OnRhythmFinished(success);
            
            // แล้ว Load กลับ Bar Scene
            SceneManager.LoadScene(barSceneName);
        }

        public bool IsPassed(float score) => score >= RequiredPassPercent;

        [ContextMenu("Debug: Simulate Finish")]
        private void DebugFinish() => OnRhythmFinished(true);  // Pass true to simulate success; change to false if needed
    }
    
}