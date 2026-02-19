using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HeneGames.DialogueSystem;

public class NPCStressUI : MonoBehaviour
{
    [Header("UI")]
    public Slider stressSlider;
    public TextMeshProUGUI npcNameText;

    private void OnEnable()
    {
        // subscribe to stress changes
        if (NPCStressSystem.Instance != null)
            NPCStressSystem.Instance.onStressChanged.AddListener(OnStressChanged);

        RefreshUI();
    }

    private void OnDisable()
    {
        if (NPCStressSystem.Instance != null)
            NPCStressSystem.Instance.onStressChanged.RemoveListener(OnStressChanged);
    }

    private void Update()
    {
        // Update active NPC name every frame (cheap)
        var active = BarNPCSequencer.Instance != null ? BarNPCSequencer.Instance.GetActiveNPC() : null;
        if (active != null)
        {
            if (npcNameText != null) npcNameText.text = active.npcName;
        }
        else
        {
            if (npcNameText != null) npcNameText.text = "";
        }
    }

    private void OnStressChanged(int current)
    {
        if (NPCStressSystem.Instance == null || stressSlider == null) return;
        float ratio = NPCStressSystem.Instance.MaxStress > 0 ? (float)current / NPCStressSystem.Instance.MaxStress : 0f;
        stressSlider.value = ratio;
    }

    private void RefreshUI()
    {
        if (NPCStressSystem.Instance != null && stressSlider != null)
        {
            int cur = NPCStressSystem.Instance.CurrentStress;
            float ratio = NPCStressSystem.Instance.MaxStress > 0 ? (float)cur / NPCStressSystem.Instance.MaxStress : 0f;
            stressSlider.value = ratio;
        }
    }
}
