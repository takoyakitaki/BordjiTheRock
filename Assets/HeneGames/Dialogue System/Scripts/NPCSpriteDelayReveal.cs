using System.Collections;
using UnityEngine;

namespace HeneGames.DialogueSystem
{
    /// <summary>
    /// ซ่อน SpriteRenderer ของ NPC ลูก (Child) จนกว่า dialogue จะจบ
    /// แล้ว fade-in เข้ามา
    /// → Hook endDialogueEvent ใน DialogueTrigger ให้เรียก RevealSprite()
    /// </summary>
    public class NPCSpriteDelayReveal : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("SpriteRenderer ของ NPC (Child GameObject) — ปล่อยว่าง = หาใน Children อัตโนมัติ)")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Fade Settings")]
        [Tooltip("ใช้เวลา fade-in กี่วินาที")]
        [SerializeField] private float fadeDuration = 0.8f;

        [Tooltip("หน่วง delay ก่อนเริ่ม fade (วินาที)")]
        [SerializeField] private float delayBeforeFade = 0.2f;

        private Coroutine _fadeCoroutine;

        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            // หา SpriteRenderer ใน Children ถ้าไม่ได้กำหนดไว้
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer == null)
            {
                Debug.LogWarning($"[NPCSpriteDelayReveal] ไม่พบ SpriteRenderer ใน {gameObject.name} หรือ Children!");
                return;
            }

            // ซ่อนตั้งแต่แรก (alpha = 0, enabled = false เพื่อประหยัด draw call)
            SetAlpha(0f);
            spriteRenderer.enabled = false;
        }

        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// เรียกจาก endDialogueEvent ใน Inspector
        /// </summary>
        public void RevealSprite()
        {
            if (spriteRenderer == null) return;

            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = StartCoroutine(FadeInRoutine());
        }

        /// <summary>ซ่อนทันทีถ้าต้องการ reset</summary>
        public void HideSprite()
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            if (spriteRenderer == null) return;

            SetAlpha(0f);
            spriteRenderer.enabled = false;
        }

        // ─────────────────────────────────────────────────────────────

        private IEnumerator FadeInRoutine()
        {
            // เปิด renderer ก่อน fade เริ่ม
            spriteRenderer.enabled = true;
            SetAlpha(0f);

            // delay เล็กน้อยก่อนเริ่ม fade
            if (delayBeforeFade > 0f)
                yield return new WaitForSeconds(delayBeforeFade);

            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / fadeDuration);
                SetAlpha(alpha);
                yield return null;
            }

            SetAlpha(1f); // ล็อค alpha ให้เต็มตอนจบ
            _fadeCoroutine = null;
        }

        private void SetAlpha(float alpha)
        {
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }
    }
}