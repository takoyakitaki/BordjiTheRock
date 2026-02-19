using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeneGames.DialogueSystem
{
    /// <summary>
    /// DialogueManager — State Machine
    ///
    /// States:
    ///   Idle            → ไม่มี dialogue
    ///   ShowingSentence → กำลังแสดงประโยค รอผู้เล่นกด Space/Click
    ///   ShowingChoice   → รอผู้เล่นเลือก choice (หยุดรับ keyboard/mouse)
    ///
    /// Input ทั้งหมดจัดการใน Update() ที่นี่ ไม่ผ่าน DialogueUI อีกต่อไป
    /// DialogueUI ทำหน้าที่แค่แสดงผล (show/hide/typewriter) เท่านั้น
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        // ─── State ────────────────────────────────────────────────────────────

        private enum DialogueState { Idle, ShowingSentence, ShowingChoice }
        private DialogueState _state = DialogueState.Idle;

        private int _blockIndex;
        private int _sentenceIndex;
        private List<DialogueBlock> _blocks;
        private DialogueTrigger _trigger;
        private int _pendingBlockAfterReaction = -1;
        private bool _isReactionMode = false;

        // ─── Inspector ────────────────────────────────────────────────────────

        [Header("References")]
        [SerializeField] private AudioSource audioSource;

        [Header("Events")]
        public UnityEvent onDialogueStart;
        public UnityEvent onDialogueEnd;

        // ─────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            if (_state != DialogueState.ShowingSentence) return;

            bool pressed = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
            if (!pressed) return;

            if (DialogueUI.instance.IsTyping())
            {
                // กด → จบ typewriter แสดงข้อความทั้งหมดทันที
                DialogueUI.instance.CompleteCurrentText();
            }
            else
            {
                // กด → ไปต่อ
                Advance();
            }
        }

        // ─── Public Entry Point ───────────────────────────────────────────────

        public void StartDialogueFromNPC(DialogueTrigger trigger, List<DialogueBlock> blocks)
        {
            if (_state != DialogueState.Idle)
            {
                Debug.LogWarning("[DialogueManager] Dialogue กำลังเล่นอยู่แล้ว");
                return;
            }

            _trigger = trigger;
            _blocks = blocks;
            _blockIndex = 0;
            _sentenceIndex = 0;
            _isReactionMode = false;

            _trigger?.startDialogueEvent.Invoke();
            onDialogueStart.Invoke();

            ShowCurrentSentence();
        }

        // ─── Core Flow ────────────────────────────────────────────────────────

        private void Advance()
        {
            // ถ้าอยู่ใน reaction mode → จบ reaction แล้วไป block ถัดไป
            if (_isReactionMode)
            {
                _isReactionMode = false;
                GoToBlock(_pendingBlockAfterReaction);
                _pendingBlockAfterReaction = -1;
                return;
            }

            _sentenceIndex++;
            var block = _blocks[_blockIndex];

            // ยังมีประโยคใน block นี้
            if (_sentenceIndex < block.sentences.Count)
            {
                ShowCurrentSentence();
                return;
            }

            // หมด sentence ใน block
            if (block.hasChoice && block.choices.Count > 0)
            {
                _state = DialogueState.ShowingChoice;
                DialogueChoiceUI.Instance.ShowChoices(block.choices, OnChoiceSelected);
            }
            else
            {
                EndDialogue();
            }
        }

        private void OnChoiceSelected(int choiceIndex)
        {
            var choice = _blocks[_blockIndex].choices[choiceIndex];

            // อัปเดต stress
            if (choice.stressReduction > 0)
                NPCStressSystem.Instance?.ReduceStress(choice.stressReduction);
            else if (choice.stressReduction < 0)
                NPCStressSystem.Instance?.IncreaseStress(-choice.stressReduction);

            int nextBlock = choice.nextBlockIndex >= 0 ? choice.nextBlockIndex : _blockIndex + 1;

            if (!string.IsNullOrEmpty(choice.npcReaction))
            {
                // แสดง reaction sentence ก่อน
                _pendingBlockAfterReaction = nextBlock;
                _isReactionMode = true;

                var block = _blocks[_blockIndex];
                var character = block.sentences.Count > 0
                    ? block.sentences[block.sentences.Count - 1].dialogueCharacter
                    : null;

                _state = DialogueState.ShowingSentence;
                DialogueUI.instance.ShowSentence(EnsureCharacter(character), choice.npcReaction);
            }
            else
            {
                GoToBlock(nextBlock);
            }
        }

        private void GoToBlock(int blockIndex)
        {
            if (blockIndex >= _blocks.Count)
            {
                EndDialogue();
                return;
            }

            _blockIndex = blockIndex;
            _sentenceIndex = 0;
            ShowCurrentSentence();
        }

        private void ShowCurrentSentence()
        {
            var s = _blocks[_blockIndex].sentences[_sentenceIndex];
            PlaySound(s.sentenceSound);
            s.sentenceEvent.Invoke();
            _state = DialogueState.ShowingSentence;
            DialogueUI.instance.ShowSentence(EnsureCharacter(s.dialogueCharacter), s.sentence);
        }

        private void EndDialogue()
        {
            _state = DialogueState.Idle;
            DialogueUI.instance.HideDialogue();
            if (audioSource != null) audioSource.Stop();

            onDialogueEnd.Invoke();

            var trigger = _trigger;
            _trigger = null;
            _blocks = null;

            trigger?.endDialogueEvent.Invoke();
            trigger?.OnDialogueFinished();
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private DialogueCharacter EnsureCharacter(DialogueCharacter c)
        {
            if (c != null) return c;
            var blank = ScriptableObject.CreateInstance<DialogueCharacter>();
            blank.characterName = "";
            blank.characterPhoto = null;
            return blank;
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource == null) return;
            audioSource.Stop();
            if (clip != null) audioSource.PlayOneShot(clip);
        }

        public void ForceStop()
        {
            DialogueChoiceUI.Instance?.HideChoices();
            DialogueUI.instance.HideDialogue();
            if (audioSource != null) audioSource.Stop();
            _state = DialogueState.Idle;
            _trigger = null;
            _blocks = null;
        }

        public int CurrentSentenceLenght()
        {
            if (_blocks == null || _state == DialogueState.Idle) return 0;
            var block = _blocks[_blockIndex];
            if (_sentenceIndex >= block.sentences.Count) return 0;
            return block.sentences[_sentenceIndex].sentence.Length;
        }
    }

    // ─── Data Structures ──────────────────────────────────────────────────────

    [System.Serializable]
    public class NPC_Centence
    {
        [Header("----")]
        public DialogueCharacter dialogueCharacter;
        [TextArea(3, 10)] public string sentence;
        public float skipDelayTime = 0.5f;
        public AudioClip sentenceSound;
        public UnityEvent sentenceEvent;
    }
}