using UnityEngine;
using DG.Tweening;

public class MainMenuUI : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] RectTransform title;
    [SerializeField] CanvasGroup titleGroup;

    [Header("Buttons (TopBottom)")]
    [SerializeField] RectTransform[] buttons;
    [SerializeField] CanvasGroup[] buttonGroups;

    [Header("Right Panel")]
    [SerializeField] RectTransform rightPanel;
    [SerializeField] CanvasGroup rightPanelGroup;

    [Header("Left Characters (TopBottom)")]
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

        seq.Append(title.DOAnchorPosY(title.anchoredPosition.y, titleDuration).From(title.anchoredPosition.y - titleYOffset).SetEase(ease));
        seq.Join(titleGroup.DOFade(1f, titleDuration));

        for (int i = 0; i < buttons.Length; i++)
        {
            float delay = (i + 1) * buttonStagger;
            seq.AppendInterval(i == 0 ? 0.05f : 0f);
            seq.Insert(seq.Duration() + delay,
                buttons[i].DOAnchorPosY(buttons[i].anchoredPosition.y, buttonDuration).From(buttons[i].anchoredPosition.y - buttonYOffset).SetEase(ease));
            seq.Insert(seq.Duration(),
                buttonGroups[i].DOFade(1f, buttonDuration + 0.05f));
        }

        float afterButtonsTime = seq.Duration() + (buttons.Length > 0 ? (buttons.Length * buttonStagger) : 0f) + 0.05f;
        seq.Insert(afterButtonsTime,
            rightPanel.DOAnchorPosX(rightPanel.anchoredPosition.x, panelDuration).From(rightPanel.anchoredPosition.x + panelXOffset).SetEase(ease));
        seq.Insert(afterButtonsTime,
            rightPanelGroup.DOFade(1f, panelDuration));

        float startCharactersTime = afterButtonsTime + panelDuration * 0.7f;
        for (int i = 0; i < characters.Length; i++)
        {
            float t = startCharactersTime + i * characterStagger;
            seq.Insert(t,
                characters[i].DOAnchorPosX(characters[i].anchoredPosition.x, characterDuration).From(characters[i].anchoredPosition.x - characterXOffset).SetEase(ease));
            seq.Insert(t,
                characterGroups[i].DOFade(1f, characterDuration));
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
        if (titleGroup != null) titleGroup.alpha = 0f;
        if (title != null) title.anchoredPosition = new Vector2(title.anchoredPosition.x, title.anchoredPosition.y - titleYOffset);

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttonGroups != null && i < buttonGroups.Length && buttonGroups[i] != null) buttonGroups[i].alpha = 0f;
            if (buttons[i] != null) buttons[i].anchoredPosition = new Vector2(buttons[i].anchoredPosition.x, buttons[i].anchoredPosition.y - buttonYOffset);
        }

        if (rightPanelGroup != null) rightPanelGroup.alpha = 0f;
        if (rightPanel != null) rightPanel.anchoredPosition = new Vector2(rightPanel.anchoredPosition.x + panelXOffset, rightPanel.anchoredPosition.y);

        for (int i = 0; i < characters.Length; i++)
        {
            if (characterGroups != null && i < characterGroups.Length && characterGroups[i] != null) characterGroups[i].alpha = 0f;
            if (characters[i] != null) characters[i].anchoredPosition = new Vector2(characters[i].anchoredPosition.x - characterXOffset, characters[i].anchoredPosition.y);
        }
    }

    void SetFinalStates()
    {
        if (titleGroup != null) titleGroup.alpha = 1f;
        if (title != null) title.anchoredPosition = new Vector2(title.anchoredPosition.x, title.anchoredPosition.y + titleYOffset);

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttonGroups != null && i < buttonGroups.Length && buttonGroups[i] != null) buttonGroups[i].alpha = 1f;
            if (buttons[i] != null) buttons[i].anchoredPosition = new Vector2(buttons[i].anchoredPosition.x, buttons[i].anchoredPosition.y + buttonYOffset);
        }

        if (rightPanelGroup != null) rightPanelGroup.alpha = 1f;
        if (rightPanel != null) rightPanel.anchoredPosition = new Vector2(rightPanel.anchoredPosition.x - panelXOffset, rightPanel.anchoredPosition.y);

        for (int i = 0; i < characters.Length; i++)
        {
            if (characterGroups != null && i < characterGroups.Length && characterGroups[i] != null) characterGroups[i].alpha = 1f;
            if (characters[i] != null) characters[i].anchoredPosition = new Vector2(characters[i].anchoredPosition.x + characterXOffset, characters[i].anchoredPosition.y);
        }
    }
}
