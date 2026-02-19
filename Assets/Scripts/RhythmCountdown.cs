using UnityEngine;
using TMPro;
using System.Collections;

public class RhythmCountdown : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI countdownText;

    [Header("Settings")]
    [Tooltip("จำนวนวินาทีที่นับถอยหลังก่อนเริ่มเกม")]
    public float countdownDuration = 3f;
    [Tooltip("ขนาด font เมื่อแสดงตัวเลข")]
    public int fontSize = 80;
    [Tooltip("สีของตัวเลข")]
    public Color countdownColor = Color.white;

    [Header("Animation")]
    [Tooltip("ขนาด scale เริ่มต้น (ใหญ่) ก่อน animate ลง")]
    public float startScale = 2.5f;
    [Tooltip("ขนาด scale ปลาย (เล็ก) ก่อน fade หาย")]
    public float endScale = 0.8f;
    [Tooltip("ความเร็ว animate scale ลง")]
    public float scaleSpeed = 3f;
    [Tooltip("ช่วงเวลา (วินาที) ก่อนตัวเลขถัดไป ที่ใช้ fade out")]
    public float fadeDuration = 0.25f;

    private bool _isCountingDown = false;
    public bool IsCountingDown => _isCountingDown;

    private void Awake()
    {
        if (countdownText != null)
        {
            countdownText.enabled = false;
            countdownText.fontSize = fontSize;
            countdownText.color = countdownColor;
        }
    }

   

    public void StartCountdown(System.Action onCountdownComplete = null)
    {
        if (_isCountingDown) return;
        StartCoroutine(CountdownCoroutine(onCountdownComplete));
    }

    public void CancelCountdown()
    {
        if (_isCountingDown)
        {
            StopAllCoroutines();
            _isCountingDown = false;
            if (countdownText != null) countdownText.enabled = false;
        }
    }

    private IEnumerator CountdownCoroutine(System.Action onCountdownComplete)
    {
        _isCountingDown = true;

        if (countdownText != null)
        {
            countdownText.enabled = true;
            countdownText.fontSize = fontSize;
        }

        float remaining = countdownDuration;
        while (remaining > 0)
        {
            int n = Mathf.CeilToInt(remaining);
            yield return StartCoroutine(AnimateNumber(n.ToString()));
            remaining -= 1f;
        }

        yield return StartCoroutine(AnimateNumber("GO!", fast: true));

        _isCountingDown = false;
        onCountdownComplete?.Invoke();
    }

    private IEnumerator AnimateNumber(string text, bool fast = false)
    {
        if (countdownText == null) yield break;

        countdownText.text = text;
        float duration  = fast ? 0.5f : 1f;
        float elapsed   = 0f;
        float startSize = fontSize * startScale;
        float endSize   = fontSize * endScale;

        countdownText.fontSize = startSize;
        countdownText.color    = new Color(countdownColor.r, countdownColor.g, countdownColor.b, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            countdownText.fontSize = Mathf.Lerp(startSize, endSize, t);

            float fadeStart = 1f - (fadeDuration / duration);
            float alpha = t < fadeStart ? 1f : Mathf.InverseLerp(1f, fadeStart, t);
            countdownText.color = new Color(countdownColor.r, countdownColor.g, countdownColor.b, alpha);

            yield return null;
        }

        if (!fast)
        {
            countdownText.fontSize = startSize;
            countdownText.color = new Color(countdownColor.r, countdownColor.g, countdownColor.b, 1f);
        }
    }
}