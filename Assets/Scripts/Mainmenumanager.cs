using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("ชื่อ Scene ที่จะโหลดเมื่อกด Play")]
    public string mainSceneName = "Main";

    [Header("Panels")]
    public GameObject settingsPanel;

    // ─────────────────────────────────────────────────────────────────────

    private void Start()
    {
        // ตรวจให้แน่ใจว่า Settings Panel ปิดอยู่ตอนเริ่ม
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // ─── Button Methods ───────────────────────────────────────────────────

    public void OnPlayPressed()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    public void OnSettingsPressed()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void OnSettingsClosePressed()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OnExitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}