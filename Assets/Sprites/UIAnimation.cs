using UnityEngine;
using UnityEngine.UI;

public class UIAnimation : MonoBehaviour
{
    [SerializeField] Image targetImage;
    [SerializeField] Sprite[] frames;
    [SerializeField] float frameRate = 0.1f; // her frame için saniye

    int currentFrame;
    float timer;

    void Reset()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            timer -= frameRate;
            currentFrame = (currentFrame + 1) % frames.Length;
            targetImage.sprite = frames[currentFrame];
        }
    }
}