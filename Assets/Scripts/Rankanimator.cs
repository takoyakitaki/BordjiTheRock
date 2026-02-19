using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// ติดไว้บน GameObject เดียวกับ rankText
/// เมื่อ SetRank() ถูกเรียก → animate: punch scale + shimmer color
///
/// วิธีใช้:
///   1. ติด RankAnimator บน rankText GameObject
///   2. ลาก TextMeshProUGUI มาใส่ช่อง rankText
///   3. เรียก rankAnimator.PlayRankReveal() จาก RhythmResultHandler ตอน pass
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class RankAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("เวลา (วินาที) ที่ rank ค่อยๆ โตขึ้น")]
    public float punchDuration   = 0.4f;
    [Tooltip("สเกลสูงสุดตอน punch")]
    public float punchScale      = 1.4f;
    [Tooltip("เวลา delay ก่อนเริ่ม animate หลัง panel โชว์")]
    public float revealDelay     = 0.3f;
    [Tooltip("จำนวนครั้งที่ shimmer")]
    public int   shimmerCount    = 3;
    public float shimmerInterval = 0.12f;

    private TextMeshProUGUI _text;
    private Vector3         _originalScale;

    private void Awake()
    {
        _text          = GetComponent<TextMeshProUGUI>();
        _originalScale = transform.localScale;
    }

    /// <summary>เรียกจาก RhythmResultHandler ตอน ShowPassPanel</summary>
    public void PlayRankReveal()
    {
        StopAllCoroutines();
        StartCoroutine(RankRevealCoroutine());
    }

    private IEnumerator RankRevealCoroutine()
    {
        // reset
        transform.localScale = Vector3.zero;
        yield return new WaitForSeconds(revealDelay);

        // punch-in
        float t = 0f;
        while (t < punchDuration)
        {
            t += Time.deltaTime;
            float norm  = t / punchDuration;
            float scale = Mathf.LerpUnclamped(0f, punchScale,
                              EaseOutBack(norm));
            transform.localScale = _originalScale * scale;
            yield return null;
        }

        // clamp ke original
        transform.localScale = _originalScale;

        // shimmer: สลับ alpha
        Color baseColor = _text.color;
        Color dimColor  = new Color(baseColor.r, baseColor.g, baseColor.b, 0.4f);
        for (int i = 0; i < shimmerCount; i++)
        {
            _text.color = dimColor;
            yield return new WaitForSeconds(shimmerInterval);
            _text.color = baseColor;
            yield return new WaitForSeconds(shimmerInterval);
        }
    }

    // Ease Out Back formula
    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}