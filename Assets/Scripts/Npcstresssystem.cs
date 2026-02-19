using UnityEngine;
using UnityEngine.Events;

namespace HeneGames.DialogueSystem
{
    public class NPCStressSystem : MonoBehaviour
    {
        public static NPCStressSystem Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int maxStress = 100;

        [Header("Events")]
        public UnityEvent<int> onStressChanged;
        public UnityEvent onStressZero;

        private int _current;

        public int CurrentStress => _current;
        public int MaxStress => maxStress;
        public float StressRatio => (float)_current / maxStress;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void BeginSession()
        {
            _current = maxStress;
            onStressChanged?.Invoke(_current);
        }

        public void ReduceStress(int amount)
        {
            _current = Mathf.Max(0, _current - amount);
            onStressChanged?.Invoke(_current);
            if (_current <= 0) onStressZero?.Invoke();
        }

        public void IncreaseStress(int amount)
        {
            _current = Mathf.Min(maxStress, _current + amount);
            onStressChanged?.Invoke(_current);
        }
    }
}