using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HeneGames.DialogueSystem
{
    /// <summary>
    /// DialogueUI — ทำหน้าที่แสดงผลอย่างเดียว
    /// - ไม่จัดการ input (ย้ายไป DialogueManager แล้ว)
    /// - ShowSentence() → แสดงข้อความพร้อม typewriter
    /// - CompleteCurrentText() → จบ typewriter ทันที
    /// - HideDialogue() → ซ่อน dialogue window
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        public static DialogueUI instance { get; private set; }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            dialogueWindow.SetActive(false);
            interactionUI.SetActive(false);
        }

        // ─── Inspector ────────────────────────────────────────────────────────

        [Header("References")]
        [SerializeField] private Image portrait;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private GameObject dialogueWindow;
        [SerializeField] private GameObject interactionUI;

        [Header("Settings")]
        [SerializeField] private bool animateText = true;
        [Range(0.01f, 0.2f)]
        [SerializeField] private float charDelay = 0.04f;

        // ─── State ────────────────────────────────────────────────────────────

        private bool _typing = false;
        private string _fullText = "";
        private Coroutine _typingCoroutine;

        public bool IsTyping() => _typing;

        // ─── Public API ───────────────────────────────────────────────────────

        /// <summary>แสดง sentence พร้อม typewriter effect</summary>
        public void ShowSentence(DialogueCharacter character, string message)
        {
            // หยุด typewriter เก่า
            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);

            dialogueWindow.SetActive(true);
            
            // --- ส่วนที่แก้ไข ---
            portrait.sprite = character.characterPhoto;
            portrait.preserveAspect = true; // <--- เพิ่มบรรทัดนี้ เพื่อไม่ให้รูปยืด
            // ------------------

            nameText.text = character.characterName;
            _fullText = message;

            if (animateText)
            {
                _typingCoroutine = StartCoroutine(TypewriterCoroutine(message));
            }
            else
            {
                messageText.text = message;
                _typing = false;
            }
        }

        /// <summary>จบ typewriter ทันที แสดงข้อความทั้งหมด</summary>
        public void CompleteCurrentText()
        {
            if (!_typing) return;
            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
            messageText.text = _fullText;
            _typing = false;
        }

        /// <summary>ซ่อน dialogue window</summary>
        public void HideDialogue()
        {
            CompleteCurrentText();
            dialogueWindow.SetActive(false);
        }

        public void ShowInteractionUI(bool value) => interactionUI.SetActive(value);

        // ─── Legacy compat ────────────────────────────────────────────────────
        // เก็บไว้เพื่อไม่ให้ script อื่นที่อาจเรียกใช้ error

        public void ClearText() => HideDialogue();
        public bool IsProcessingDialogue() => DialogueManager.Instance != null;
        public int CurrentDialogueSentenceLenght() => DialogueManager.Instance?.CurrentSentenceLenght() ?? 0;

        // ─── Typewriter ───────────────────────────────────────────────────────

        private IEnumerator TypewriterCoroutine(string text)
        {
            _typing = true;
            messageText.text = "";

            foreach (char c in text)
            {
                messageText.text += c;
                yield return new WaitForSeconds(charDelay);
            }

            _typing = false;
            _typingCoroutine = null;
        }
    }
}