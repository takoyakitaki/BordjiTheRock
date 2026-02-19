using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ติดบน Button GameObject แต่ละปุ่ม
/// เมื่อ hover → ขยาย, เมื่อออก → หด กลับขนาดเดิม
/// ใช้ Lerp ให้ animation smooth
/// </summary>
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Settings")]
    [Tooltip("ขนาดตอน hover (1.1 = ใหญ่ขึ้น 10%)")]
    public float hoverScale  = 1.1f;
    [Tooltip("ขนาดตอนกดค้าง")]
    public float pressScale  = 0.95f;
    [Tooltip("ความเร็ว animation (สูง = เร็ว)")]
    public float scaleSpeed  = 10f;

    private Vector3 _originalScale;
    private Vector3 _targetScale;

    // ─────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _originalScale = transform.localScale;
        _targetScale   = _originalScale;
    }

    private void Update()
    {
        // Lerp ไปหา target ทุก frame → ได้ animation smooth
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            _targetScale,
            Time.deltaTime * scaleSpeed
        );
    }

    // ─── Pointer Events ───────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData _)
        => _targetScale = _originalScale * hoverScale;

    public void OnPointerExit(PointerEventData _)
        => _targetScale = _originalScale;

    public void OnPointerDown(PointerEventData _)
        => _targetScale = _originalScale * pressScale;

    public void OnPointerUp(PointerEventData _)
        => _targetScale = _originalScale * hoverScale; // กลับ hover state ถ้าเมาส์ยังอยู่
}