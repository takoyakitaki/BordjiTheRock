using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HeneGames.DialogueSystem
{
    public class DialogueChoiceUI : MonoBehaviour
    {
        public static DialogueChoiceUI Instance { get; private set; }

        [SerializeField] private GameObject choicePanel;
        [SerializeField] private List<Button> choiceButtons = new List<Button>();
        [SerializeField] private List<TextMeshProUGUI> choiceTexts = new List<TextMeshProUGUI>();

        private Action<int> _callback;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            for (int i = 0; i < choiceButtons.Count; i++)
            {
                int idx = i;
                choiceButtons[i].onClick.AddListener(() => OnClick(idx));
            }

            choicePanel.SetActive(false);
        }

        public void ShowChoices(List<DialogueChoice> choices, Action<int> callback)
        {
            _callback = callback;

            for (int i = 0; i < choiceButtons.Count; i++)
                choiceButtons[i].gameObject.SetActive(false);

            for (int i = 0; i < choices.Count && i < choiceButtons.Count; i++)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceTexts[i].text = choices[i].choiceText;
            }

            choicePanel.SetActive(true);
        }

        public void HideChoices()
        {
            choicePanel.SetActive(false);
            _callback = null;
        }

        private void OnClick(int index)
        {
            var cb = _callback;
            HideChoices();
            cb?.Invoke(index);
        }
    }
}