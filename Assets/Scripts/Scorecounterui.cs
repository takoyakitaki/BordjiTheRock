using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// ติดบน scorePercentText — ทำให้ตัวเลข % ค่อยๆ นับขึ้นแบบ osu
///
/// วิธีใช้:
///   1. ติด ScoreCounterUI บน scorePercentText GameObject
///   2. เรียก scoreCounter.CountTo(scorePercent) จาก RhythmResultHandler
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class ScoreCounterUI : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("เวลารวมในการนับ (วินาที)")]
    public float countDuration = 1.2f;
    [Tooltip("delay ก่อนเริ่มนับ")]
    public float startDelay    = 0.5f;

    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>เรียกจาก RhythmResultHandler ตอน ShowPassPanel</summary>
    public void CountTo(float targetPercent)
    {
        StopAllCoroutines();
        StartCoroutine(CountCoroutine(targetPercent));
    }

    private IEnumerator CountCoroutine(float target)
    {
        _text.text = "0.0%";
        yield return new WaitForSeconds(startDelay);

        float elapsed = 0f;
        while (elapsed < countDuration)
        {
            elapsed += Time.deltaTime;
            float t       = Mathf.Clamp01(elapsed / countDuration);
            float current = Mathf.Lerp(0f, target, EaseOutQuart(t));
            _text.text    = $"{current:F1}%";
            yield return null;
        }

        _text.text = $"{target:F1}%";
    }

    private float EaseOutQuart(float x)
    {
        return 1f - Mathf.Pow(1f - x, 4f);
    }
}