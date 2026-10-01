using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct CutsceneFrame
{
    public Sprite image;
 
    [Tooltip("Leave empty to hide the textbox on this frame.")]
    [TextArea(2, 5)]
    public string text;
}

// Plays a sequence of full-screen still images with fades between them,
public class CutSceneManager : MonoBehaviour
{
    #region Serialized Fields
    [Header("Frames")]
    [Tooltip("The full-screen UI Image that shows each frame.")]
    [SerializeField] private Image frameImage;
    [Tooltip("Cutscene images in order. Just increase the size to add more.")]
    [SerializeField] private CutsceneFrame[] frames;

    [Header("Textbox")]
    [Tooltip("The textbox object (your custom textbox image).")]
    [SerializeField] private GameObject textBox;
    [Tooltip("The TextMeshPro text inside the textbox.")]
    [SerializeField] private TMP_Text dialogueText;
    [Tooltip("Typewriter speed. 0 = show all text instantly.")]
    [SerializeField] private float charactersPerSecond = 40f;
 
    [Header("Timing")]
    [Tooltip("Seconds each frame stays up before moving on. 0 = wait for a key press.")]
    [SerializeField] private float secondsPerFrame = 3f;
    [Tooltip("Seconds to fade in or out between frames.")]
    [SerializeField] private float fadeDuration = 0.5f;
 
    [Header("Fade")]
    [Tooltip("A black full-screen UI Image")]
    [SerializeField] private Image fadeOverlay;
 
    [Header("Input")]
    [SerializeField] private KeyCode nextFrameKey = KeyCode.Space;
    [SerializeField] private KeyCode skipCutsceneKey = KeyCode.Escape;
 
    [Header("Next Scene")]
    [Tooltip("Must match the game scene's name exactly.")]
    [SerializeField] private string gameSceneName = "SampleScene";
    #endregion

    #region Private Fields
    private bool advanceRequested;
    private bool isLoading;
    #endregion

    #region Unity Methods
    private void Start()
    {
        StartCoroutine(PlayCutscene());
    }
 
    private void Update()
    {
        if (Input.GetKeyDown(skipCutsceneKey))
        {
            LoadGame();
            return;
        }
 
        if (Input.GetKeyDown(nextFrameKey) || Input.GetMouseButtonDown(0))
        {
            advanceRequested = true;
        }
    }
    #endregion

    #region Cutscene
    private IEnumerator PlayCutscene()
    {
        SetOverlayAlpha(1f); // start on black
 
        foreach (CutsceneFrame frame in frames)
        {
            frameImage.sprite = frame.image;
            PrepareText(frame.text);
 
            yield return Fade(1f, 0f);       // fade in
            yield return TypeText();
            yield return WaitOnFrame();
            yield return Fade(0f, 1f);       // fade out
        }
 
        LoadGame();
    }
 
    private IEnumerator WaitOnFrame()
    {
        advanceRequested = false;
        float elapsed = 0f;
 
        while (!advanceRequested && (secondsPerFrame <= 0f || elapsed < secondsPerFrame))
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
 
    private void LoadGame()
    {
        if (isLoading)
        {
            return;
        }
 
        isLoading = true;
        StopAllCoroutines();
        SceneManager.LoadScene(gameSceneName);
    }
    #endregion

    #region Textbox
 
    private void PrepareText(string text)
    {
        bool hasText = !string.IsNullOrWhiteSpace(text);
        textBox.SetActive(hasText);
 
        dialogueText.text = hasText ? text : string.Empty;
        dialogueText.maxVisibleCharacters = 0; // hidden until typing starts
    }
 
    private IEnumerator TypeText()
    {
        dialogueText.ForceMeshUpdate();
        int totalCharacters = dialogueText.textInfo.characterCount;
 
        if (totalCharacters == 0 || charactersPerSecond <= 0f)
        {
            dialogueText.maxVisibleCharacters = totalCharacters;
            yield break;
        }
 
        advanceRequested = false;
        float visible = 0f;
 
        while (visible < totalCharacters)
        {
            // First press while typing just finishes the text
            if (advanceRequested)
            {
                break;
            }
 
            visible += charactersPerSecond * Time.deltaTime;
            dialogueText.maxVisibleCharacters = Mathf.FloorToInt(visible);
            yield return null;
        }
        dialogueText.maxVisibleCharacters = totalCharacters;
    }

    #endregion

    #region Fade
    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
 
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetOverlayAlpha(Mathf.Lerp(from, to, elapsed / fadeDuration));
            yield return null;
        }
 
        SetOverlayAlpha(to);
    }
 
    private void SetOverlayAlpha(float alpha)
    {
        Color color = fadeOverlay.color;
        color.a = alpha;
        fadeOverlay.color = color;
    }
    #endregion

}
