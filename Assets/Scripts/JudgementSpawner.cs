using UnityEngine;
using System.Collections;

public class JudgementSpawner : MonoBehaviour
{
    [Header("Judgement Prefabs")]
    public GameObject perfectPrefab;
    public GameObject greatPrefab;
    public GameObject missPrefab;

    [Header("Animation")]
    public float riseSpeed     = 3f;    // ความเร็วลอยขึ้น
    public float scaleDuration = 0.1f;  // เวลา scale โต
    public float holdDuration  = 0.15f; // เวลาค้างก่อน fade
    public float fadeDuration  = 0.2f;  // เวลา fade out
    public float scaleTarget   = .7f;  // ขนาดสูงสุด

    private GameObject _current; // track อันที่แสดงอยู่

    public void Spawn(string judgement, Vector3 worldPos)
    {
        GameObject prefab = judgement?.Trim().ToLower() switch
        {
            "perfect" => perfectPrefab,
            "great"   => greatPrefab,
            "miss"    => missPrefab,
            _         => null
        };

        if (prefab == null) return;

        // ลบอันเก่าทิ้งก่อน
        if (_current != null)
        {
            Destroy(_current);
            _current = null;
        }

        GameObject go = Instantiate(prefab, worldPos, Quaternion.identity);
        _current = go;

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "UI";
            sr.sortingOrder = 10;
        }

        StartCoroutine(AnimateCoroutine(go, sr));
    }

    private IEnumerator AnimateCoroutine(GameObject go, SpriteRenderer sr)
    {
        if (go == null) yield break;

        // 1. Scale โตขึ้น
        float elapsed = 0f;
        go.transform.localScale = Vector3.zero;

        while (elapsed < scaleDuration && go != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleDuration);
            float s = Mathf.Lerp(0f, scaleTarget, EaseOutBack(t));
            go.transform.localScale = Vector3.one * s;
            yield return null;
        }

        if (go == null) yield break;
        go.transform.localScale = Vector3.one * scaleTarget;

        // 2. ค้างและลอยขึ้น
        elapsed = 0f;
        while (elapsed < holdDuration && go != null)
        {
            elapsed += Time.deltaTime;
            go.transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            yield return null;
        }

        // 3. Fade out พร้อมลอยต่อ
        if (go == null) yield break;
        elapsed = 0f;
        Color col = sr != null ? sr.color : Color.white;

        while (elapsed < fadeDuration && go != null)
        {
            elapsed += Time.deltaTime;
            go.transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            if (sr != null)
            {
                col.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                sr.color = col;
            }
            yield return null;
        }

        if (go != null)
        {
            if (_current == go) _current = null;
            Destroy(go);
        }
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}