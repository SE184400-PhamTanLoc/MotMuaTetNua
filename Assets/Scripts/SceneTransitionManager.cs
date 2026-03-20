using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Settings")]
    public float fadeDuration = 0.5f;

    private Image fadeImage;
    private bool isTransitioning = false;

    // Automatically initialize this singleton when the game starts
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("SceneTransitionManager");
            Instance = go.AddComponent<SceneTransitionManager>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CreateFadeUI();
        
        // Listen to scene loaded event to fade in
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void CreateFadeUI()
    {
        // Add a canvas dynamically
        GameObject canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Ensure it renders on top of everything

        // Add the black image
        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform);
        fadeImage = imageGO.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = true; // Blocks clicks during transition

        // Make the image stretch across the screen
        RectTransform rt = fadeImage.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        fadeImage.gameObject.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Fade in when a new scene finishes loading
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(FadeInRoutine());
        }
    }

    /// <summary>
    /// Smoothly transitions to the specified scene with fade out and fade in.
    /// </summary>
    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;
        
        // Disable touch/click events globally or just via UI
        fadeImage.gameObject.SetActive(true);

        // Fade Out (to black)
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 1f);

        // Keep black for a tiny bit
        yield return new WaitForSecondsRealtime(0.1f);

        // Load Scene asynchronously
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (op != null && !op.isDone)
        {
            yield return null;
        }
        
        // Note: Fade In will be handled by OnSceneLoaded event.
        // But in some edge cases if OnSceneLoaded fires early, we may need to make sure we don't conflict.
        // We will just wait for the sceneLoaded event to clear the flag.
        isTransitioning = false;
    }

    private IEnumerator FadeInRoutine()
    {
        // Don't fade in if we're not currently blocking (e.g. initial game boot might not need fade from black if already clear)
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 1f);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0f);
        fadeImage.gameObject.SetActive(false);
    }
}
