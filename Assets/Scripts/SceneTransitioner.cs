using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Singleton that handles fade-to-black scene transitions.
/// Attach to a Canvas GameObject that sits above everything else
/// (Sort Order high, e.g. 100) with a full-screen black Image child.
/// Call SceneTransitioner.Instance.LoadScene("SceneName") from anywhere.
/// </summary>
public class SceneTransitioner : MonoBehaviour
{
    public static SceneTransitioner Instance { get; private set; }

    [Tooltip("The full-screen black Image used for fading.")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetAlpha(1f);
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(Fade(1f, 0f));
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionTo(sceneName));
    }

    private IEnumerator TransitionTo(string sceneName)
    {
        // Fade out to black.
        yield return StartCoroutine(Fade(0f, 1f));

        SceneManager.LoadScene(sceneName);

        // Fade in from black — Start() handles this automatically since
        // DontDestroyOnLoad keeps this object alive across loads.
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            SetAlpha(Mathf.Lerp(from, to, elapsed / fadeDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        SetAlpha(to);
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }
}