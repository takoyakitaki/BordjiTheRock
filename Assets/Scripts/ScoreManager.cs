using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("UI References")]
    public TextMeshProUGUI scorePercentText;
    public TextMeshProUGUI comboText;

    [Header("Score Settings")]
    public int perfectScore        = 100;
    public int greatScore          = 50;
    public int longNoteClearScore  = 150;

    private int _currentScore  = 0;
    private int _maxPossible   = 0;  // ── ข้อ 3: set จาก beatmap ก่อนเกมเริ่ม
    private int _currentCombo  = 0;

    // ── ข้อ 3: flag ว่าได้ pre-calculate แล้วหรือยัง ──────────────────
    private bool _maxPreCalculated = false;

    public float ScoreRatio   => _maxPossible > 0 ? (float)_currentScore / _maxPossible : 0f;
    public float ScorePercent => ScoreRatio * 100f;

    // ─────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateScoreDisplay();
        UpdateComboDisplay();
    }

    // ── ข้อ 3: เรียกจาก BeatmapSpawner หลัง LoadBeatmap() ──────────────
    /// <summary>
    /// คำนวณ MaxPossible จาก beatmap ทั้งหมดล่วงหน้า
    /// ทำให้ ScoreRatio ถูกต้องตั้งแต่ต้น แม้โน้ตยังไม่ spawn
    /// </summary>
    public void PreCalculateMaxScore(BeatmapData beatmap)
    {
        if (beatmap == null) return;
        if (_maxPreCalculated)
        {
            Debug.LogWarning("[ScoreManager] PreCalculateMaxScore ถูกเรียกซ้ำ!");
            return;
        }

        int total = 0;
        foreach (NoteData note in beatmap.notes)
        {
            if (note.noteType == 1) // Long Note
                total += longNoteClearScore;
            else                    // Tap Note
                total += perfectScore;
        }

        _maxPossible       = total;
        _maxPreCalculated  = true;

        Debug.Log($"[ScoreManager] Pre-calculated MaxPossible = {_maxPossible} " +
                  $"({beatmap.notes.Count} notes)");

        UpdateScoreDisplay();
    }

    // ─────────────────────────────────────────────────────────────────────

    /// <summary>เรียกตอน Perfect / Great / LongNoteClear</summary>
    public void AddScore(int points)
    {
        _currentScore += points;

        // ถ้า pre-calculate แล้ว ไม่ต้องบวก _maxPossible อีก
        // ถ้ายังไม่ได้ pre-calculate (fallback) ให้บวกเหมือนเดิม
        if (!_maxPreCalculated)
            _maxPossible += points;

        UpdateScoreDisplay();
    }

    /// <summary>เรียกตอน Miss</summary>
    public void RegisterMiss()
    {
        // ถ้า pre-calculate แล้ว max ถูก lock ไว้แล้ว ไม่ต้องทำอะไร
        // ถ้ายังไม่ได้ pre-calculate (fallback) บวก max เหมือนเดิม
        if (!_maxPreCalculated)
            _maxPossible += perfectScore;

        UpdateScoreDisplay();
    }

    public void AddCombo()
    {
        _currentCombo++;
        UpdateComboDisplay();
    }

    public void ResetCombo()
    {
        _currentCombo = 0;
        UpdateComboDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scorePercentText != null)
            scorePercentText.text = $"{ScorePercent:F1}%";
    }

    private void UpdateComboDisplay()
    {
        if (comboText == null) return;
        if (_currentCombo > 1)
        {
            comboText.text = $"{_currentCombo}";
            comboText.gameObject.SetActive(true);
        }
        else
        {
            comboText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Toggle visibility of score/combo UI elements (used by result screens).
    /// </summary>
    public void SetUIVisibility(bool visible)
    {
        if (scorePercentText != null) scorePercentText.gameObject.SetActive(visible);
        if (comboText != null)
        {
            // comboText is normally hidden when combo <= 1, so only enable/disable when forcing visibility
            comboText.gameObject.SetActive(visible && _currentCombo > 1);
        }
    }
}