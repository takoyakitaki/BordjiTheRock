using UnityEngine;
using System.Collections;

public class HitEffectController : MonoBehaviour
{
    [Header("Particle")]
    public ParticleSystem hitParticle;

    [Header("Glow Sprite (ใส่ sprite วงกลมโปร่งใส)")]
    public SpriteRenderer glowSprite;
    public Color glowColor       = new Color(1f, 1f, 0.5f, 0.8f);
    public float glowDuration    = 0.15f;
    public float glowMaxScale    = 1.5f;
    public AnimationCurve scaleCurve;   // EaseOut
    public AnimationCurve alphaCurve;   // EaseIn (fade out)

    private Coroutine glowCoroutine;

    public void PlayHitEffect()
    {
        // Particle
        if (hitParticle != null)
            hitParticle.Play();

        // Glow flash
        if (glowCoroutine != null) StopCoroutine(glowCoroutine);
        glowCoroutine = StartCoroutine(GlowRoutine());
    }

    private IEnumerator GlowRoutine()
    {
        float elapsed = 0f;
        glowSprite.enabled = true;

        while (elapsed < glowDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / glowDuration;

            float scale = Mathf.Lerp(0.3f, glowMaxScale, scaleCurve.Evaluate(t));
            float alpha = Mathf.Lerp(1f,   0f,           alphaCurve.Evaluate(t));

            glowSprite.transform.localScale = Vector3.one * scale;
            glowSprite.color = new Color(glowColor.r, glowColor.g, glowColor.b, alpha);

            yield return null;
        }

        glowSprite.enabled = false;
    }
}