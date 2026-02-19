using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Image สีดำใน Canvas — ปล่อยว่าง = หาใน Children อัตโนมัติ")]
    [SerializeField] private Image fadePanel;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.8f;
    [SerializeField] private float delayBeforeFade = 0.2f;

    private Coroutine _fadeCoroutine;

    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (fadePanel == null)
            fadePanel = GetComponentInChildren<Image>();

        if (fadePanel == null)
        {
            Debug.LogWarning($"[SceneFader] ไม่พบ Image ใน {gameObject.name} หรือ Children!");
            return;
        }

        SetAlpha(1f);
        fadePanel.gameObject.SetActive(true);
    }

    // ─────────────────────────────────────────────────────────────

    /// <summary>จอดำเข้ามา</summary>
    public void FadeOut()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeRoutine(0f, 1f, false));
    }

    /// <summary>จอดำจางออก</summary>
    public void FadeIn()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeRoutine(1f, 0f, true));
    }

    // ─────────────────────────────────────────────────────────────

    private IEnumerator FadeRoutine(float from, float to, bool disableOnFinish)
    {
        fadePanel.gameObject.SetActive(true);
        SetAlpha(from);

        if (delayBeforeFade > 0f)
            yield return new WaitForSeconds(delayBeforeFade);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / fadeDuration)));
            yield return null;
        }

        SetAlpha(to);

        if (disableOnFinish)
            fadePanel.gameObject.SetActive(false);

        _fadeCoroutine = null;
    }

    private void SetAlpha(float alpha)
    {
        if (fadePanel == null) return;
        Color c = fadePanel.color;
        c.a = alpha;
        fadePanel.color = c;
    }
}