using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    [Header("References")]
    public Image fadeImage;
    
    [Header("Settings")]
    public float fadeDuration = 1f;
    
    private Coroutine currentFadeCoroutine;
    
    public void FadeIn(float duration = -1f)
    {
        if (duration < 0) duration = fadeDuration;
        
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        
        currentFadeCoroutine = StartCoroutine(FadeInCoroutine(duration));
    }
    
    public void FadeOut(float duration = -1f)
    {
        if (duration < 0) duration = fadeDuration;
        
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        
        currentFadeCoroutine = StartCoroutine(FadeOutCoroutine(duration));
    }
    
    public void SetBlackScreen()
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }
    }
    
    public void SetClearScreen()
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
    }
    
    private IEnumerator FadeInCoroutine(float duration)
    {
        if (fadeImage == null) yield break;
        
        fadeImage.gameObject.SetActive(true);
        Color c = fadeImage.color;
        float startAlpha = c.a;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, 1f, elapsed / duration);
            fadeImage.color = c;
            yield return null;
        }
        
        c.a = 1f;
        fadeImage.color = c;
    }
    
    private IEnumerator FadeOutCoroutine(float duration)
    {
        if (fadeImage == null) yield break;
        
        Color c = fadeImage.color;
        float startAlpha = c.a;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            fadeImage.color = c;
            yield return null;
        }
        
        c.a = 0f;
        fadeImage.color = c;
        fadeImage.gameObject.SetActive(false);
    }
}
