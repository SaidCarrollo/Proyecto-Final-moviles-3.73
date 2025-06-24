
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 

public class WinPanelAnimator : MonoBehaviour
{
    public Image[] stars;
    public float animationDuration = 0.5f; 
    public float delayBetweenStars = 0.3f;
    public float targetStarScale = 3.5f;
    public CanvasGroup panelCanvasGroup; 

    void OnEnable()
    {
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0;
            panelCanvasGroup.DOFade(1, 0.5f);
        }
    }

    public void ShowStars(int numberOfStars)
    {
        foreach (var star in stars)
        {
            star.transform.localScale = Vector3.zero;
            star.enabled = true; 
        }

        Sequence starSequence = DOTween.Sequence();

        for (int i = 0; i < numberOfStars; i++)
        {
            starSequence.Append(
                stars[i].transform.DOScale(targetStarScale, animationDuration)
                    .SetEase(Ease.OutBack)
            );

            if (i < numberOfStars - 1)
            {
                starSequence.AppendInterval(delayBetweenStars);
            }
        }
    }
}