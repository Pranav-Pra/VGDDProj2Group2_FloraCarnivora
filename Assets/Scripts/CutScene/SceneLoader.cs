using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SceneLoader : MonoBehaviour
{
    static SceneLoader instance;

    public Image bug;                 
    public Sprite squeezeSprite;      
    public float totalTime = 3f;      
    public float fadeTime = 0.4f;
    public float squeezeEvery = 0.6f; 
    public float squeezeTime = 0.15f; 

    CanvasGroup group;
    Sprite normalSprite;

    void Awake()
    {
        instance = this;
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;             
        group.blocksRaycasts = false;
        normalSprite = bug.sprite;
    }

    public static void Load(string scene)
    {
        if (instance == null) { SceneManager.LoadScene(scene); return; } // no loading screen in this scene
        DontDestroyOnLoad(instance.gameObject); // survive the scene change so it can fade out
        instance.StartCoroutine(instance.Loading(scene));
    }

    void Update()
    { 
        bool squeezed = Time.unscaledTime % squeezeEvery < squeezeTime;
        bug.sprite = squeezed ? squeezeSprite : normalSprite;
    }

    IEnumerator Loading(string scene)
    {
        group.blocksRaycasts = true;
        yield return Fade(0f, 1f);

        var op = SceneManager.LoadSceneAsync(scene);
        op.allowSceneActivation = false;                          // load in the background, don't switch yet
        yield return new WaitForSecondsRealtime(totalTime - 2f * fadeTime);
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        yield return Fade(1f, 0f);
        Destroy(gameObject);
    }

    IEnumerator Fade(float from, float to)
    {
        for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        group.alpha = to;
    }
}