using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MainMenuUI : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] RectTransform title;
    [SerializeField] Image titleImage;

    [Header("Buttons TopToBottom")]
    [SerializeField] RectTransform[] buttons;
    [SerializeField] CanvasGroup[] buttonGroups;

    [Header("Right Panel")]
    [SerializeField] RectTransform rightPanel;
    [SerializeField] CanvasGroup rightPanelGroup;

    [Header("Left Characters TopToBottom")]
    [SerializeField] RectTransform[] characters;
    [SerializeField] CanvasGroup[] characterGroups;

    [Header("Timings")]
    [SerializeField] float titleDuration = 0.6f;
    [SerializeField] float buttonDuration = 0.35f;
    [SerializeField] float buttonStagger = 0.15f;
    [SerializeField] float panelDuration = 0.5f;
    [SerializeField] float characterDuration = 0.3f;
    [SerializeField] float characterStagger = 0.08f;

    [Header("Offsets")]
    [SerializeField] float titleYOffset = 160f;
    [SerializeField] float buttonYOffset = 140f;
    [SerializeField] float panelXOffset = 260f;
    [SerializeField] float characterXOffset = 220f;

    [Header("Easing")]
    [SerializeField] Ease ease = Ease.OutCubic;

    Sequence seq;

    void Awake()
    {
        DOTween.Kill(this);
        PrepareInitialStates();
    }

    void OnEnable()
    {
        PlayIntro();
    }

    public void PlayIntro()
    {
        DOTween.Kill(this);
        seq = DOTween.Sequence().SetId(this);

        seq.Append(title.DOAnchorPos(
            new Vector2(title.anchoredPosition.x, title.anchoredPosition.y),
            titleDuration
        ).From(new Vector2(title.anchoredPosition.x, title.anchoredPosition.y - titleYOffset)).SetEase(ease));
        seq.Join(titleImage.DOFade(1f, titleDuration));

        for (int i = 0; i < buttons.Length; i++)
        {
            float targetY = buttons[i].anchoredPosition.y + buttonYOffset;
            seq.Append(buttons[i].DOAnchorPosY(targetY, buttonDuration).SetEase(ease));
            if (buttonGroups != null && i < buttonGroups.Length && buttonGroups[i] != null)
                seq.Join(buttonGroups[i].DOFade(1f, buttonDuration));
            if (i < buttons.Length - 1) seq.AppendInterval(buttonStagger);
            int idx = i;
            if (buttonGroups != null && idx < buttonGroups.Length && buttonGroups[idx] != null)
                seq.AppendCallback(() =>
                {
                    buttonGroups[idx].interactable = true;
                    buttonGroups[idx].blocksRaycasts = true;
                });
        }

        seq.AppendInterval(0.05f);

        if (rightPanel != null && rightPanelGroup != null)
        {
            seq.Append(rightPanel.DOAnchorPos(
                new Vector2(rightPanel.anchoredPosition.x, rightPanel.anchoredPosition.y),
                panelDuration
            ).From(new Vector2(rightPanel.anchoredPosition.x + panelXOffset, rightPanel.anchoredPosition.y)).SetEase(ease));
            seq.Join(rightPanelGroup.DOFade(1f, panelDuration));
        }

        float charsStart = seq.Duration() - panelDuration * 0.3f;
        for (int i = 0; i < characters.Length; i++)
        {
            float t = charsStart + i * characterStagger;
            seq.Insert(t, characters[i].DOAnchorPos(
                new Vector2(characters[i].anchoredPosition.x, characters[i].anchoredPosition.y),
                characterDuration
            ).From(new Vector2(characters[i].anchoredPosition.x - characterXOffset, characters[i].anchoredPosition.y)).SetEase(ease));
            if (characterGroups != null && i < characterGroups.Length && characterGroups[i] != null)
                seq.Insert(t, characterGroups[i].DOFade(1f, characterDuration));
        }
    }

    public void SkipToEnd()
    {
        DOTween.Kill(this, false);
        SetFinalStates();
    }

    public void ResetAndPrepare()
    {
        DOTween.Kill(this, true);
        PrepareInitialStates();
    }

    void PrepareInitialStates()
    {
        if (titleImage != null) titleImage.color = new Color(titleImage.color.r, titleImage.color.g, titleImage.color.b, 0f);
        if (title != null) title.anchoredPosition = new Vector2(title.anchoredPosition.x, title.anchoredPosition.y - titleYOffset);

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
                buttons[i].anchoredPosition = new Vector2(buttons[i].anchoredPosition.x, buttons[i].anchoredPosition.y - buttonYOffset);
            if (buttonGroups != null && i < buttonGroups.Length && buttonGroups[i] != null)
            {
                buttonGroups[i].alpha = 0f;
                buttonGroups[i].interactable = false;
                buttonGroups[i].blocksRaycasts = false;
            }
        }

        if (rightPanel != null) rightPanel.anchoredPosition = new Vector2(rightPanel.anchoredPosition.x + panelXOffset, rightPanel.anchoredPosition.y);
        if (rightPanelGroup != null) rightPanelGroup.alpha = 0f;

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null)
                characters[i].anchoredPosition = new Vector2(characters[i].anchoredPosition.x - characterXOffset, characters[i].anchoredPosition.y);
            if (characterGroups != null && i < characterGroups.Length && characterGroups[i] != null)
                characterGroups[i].alpha = 0f;
        }
    }

    void SetFinalStates()
    {
        if (titleImage != null) titleImage.color = new Color(titleImage.color.r, titleImage.color.g, titleImage.color.b, 1f);
        if (title != null) title.anchoredPosition = new Vector2(title.anchoredPosition.x, title.anchoredPosition.y);

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
                buttons[i].anchoredPosition = new Vector2(buttons[i].anchoredPosition.x, buttons[i].anchoredPosition.y);
            if (buttonGroups != null && i < buttonGroups.Length && buttonGroups[i] != null)
            {
                buttonGroups[i].alpha = 1f;
                buttonGroups[i].interactable = true;
                buttonGroups[i].blocksRaycasts = true;
            }
        }

        if (rightPanel != null) rightPanel.anchoredPosition = new Vector2(rightPanel.anchoredPosition.x, rightPanel.anchoredPosition.y);
        if (rightPanelGroup != null) rightPanelGroup.alpha = 1f;

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null)
                characters[i].anchoredPosition = new Vector2(characters[i].anchoredPosition.x, characters[i].anchoredPosition.y);
            if (characterGroups != null && i < characterGroups.Length && characterGroups[i] != null)
                characterGroups[i].alpha = 1f;
        }
    }
}