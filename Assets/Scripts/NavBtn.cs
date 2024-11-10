using System.Collections;
using UnityEngine;

public class NavBtn : MonoBehaviour
{
    [SerializeField] GameObject currentScreen;
    [SerializeField] GameObject nextScreen;
    [SerializeField] float transitionDuration = 0.5f;

    public void SwitchView()
    {
        StartCoroutine(FadeTransition());
    }

    private IEnumerator FadeTransition()
    {
        CanvasGroup currentCanvasGroup = currentScreen.TryGetComponent(out CanvasGroup ccg) ? ccg : currentScreen.AddComponent<CanvasGroup>();
        CanvasGroup nextCanvasGroup = nextScreen.TryGetComponent(out CanvasGroup ncg) ? ncg : nextScreen.AddComponent<CanvasGroup>();

        nextScreen.SetActive(true);

        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsedTime / transitionDuration);
            currentCanvasGroup.alpha = alpha;
            nextCanvasGroup.alpha = 1 - alpha;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentCanvasGroup.alpha = 0;
        nextCanvasGroup.alpha = 1;

        currentScreen.SetActive(false);
    }
}