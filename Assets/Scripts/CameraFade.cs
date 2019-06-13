using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;


public enum FadingMode{
	Image, FX
}


// This class is used to fade the entire screen to black (or
// any chosen colour).  It should be used to smooth out the
// transition between scenes or restarting of a scene.
public class CameraFade : MonoBehaviour
{
	public event Action OnFadeComplete;                             // This is called when the fade in or out has finished.


	[SerializeField] private Image m_FadeImage;                     // Reference to the image that covers the screen.
	[SerializeField] private PostProcessVolume m_PostProcessVolume;
	[SerializeField] private AudioMixerSnapshot m_DefaultSnapshot;  // Settings for the audio mixer to use normally.
	[SerializeField] private AudioMixerSnapshot m_FadedSnapshot;    // Settings for the audio mixer to use when faded out.
	[SerializeField] private Color m_FadeColor = Color.black;       // The colour the image fades out to.
	[SerializeField] private float m_FadeDuration = 1.0f;           // How long it takes to fade in seconds.
	[SerializeField] private bool m_FadeInOnSceneLoad = false;      // Whether a fade in should happen as soon as the scene is loaded.
	[SerializeField] private bool m_FadeInOnStart = false;          // Whether a fade in should happen just but Updates start.
	[SerializeField] private float m_timer = 0.0f ;
	
	public bool m_IsFadingIn, m_IsFadingOut;                        // Whether the screen is currently fading.
	public GameEngine gameEngine;  
	public FadingMode fadingMode;
	private float m_FadeStartTime;                                  // The time when fading started.
	private Color m_FadeOutColor;                                   // This is a transparent version of the fade colour, it will ensure fading looks normal.
	private float customFadeDuration;
	private Bloom bloomLayer;
	private ChromaticAberration chromaticAberration;
		
	public float timer { get { return m_timer; } }
	public float m_timeRemaning { get { return m_FadeDuration - m_timer; } }	

	private void Awake()
	{
		//SceneManager.sceneLoaded += HandleSceneLoaded;

		m_FadeOutColor = new Color(m_FadeColor.r, m_FadeColor.g, m_FadeColor.b, 0f);
		m_PostProcessVolume = GetComponent<PostProcessVolume>();
 
		m_PostProcessVolume.profile.TryGetSettings(out bloomLayer);
		m_PostProcessVolume.profile.TryGetSettings(out chromaticAberration);

		if(fadingMode == FadingMode.Image) m_FadeImage.enabled = true;
        gameEngine = GameObject.FindWithTag("GameEngine").GetComponent<GameEngine>();
	}


	private void Start()
	{
		// If applicable set the immediate colour to be faded out and then fade in.
		if (m_FadeInOnStart && fadingMode == FadingMode.Image)
		{
			m_FadeImage.color = m_FadeColor;
			FadeIn();
		}
	}


	public void FadeOut()
	{
		Debug.Log("Fade out request ...");	
		if(fadingMode == FadingMode.Image) m_FadeImage.enabled = true;
		
		if (m_IsFadingIn)	 {
			print("Already Fading In, time remaining : "+(m_timeRemaning));

			Invoke("DelayedFadeOut", m_FadeDuration);
		}
		else if(m_IsFadingOut) return;
		else {
			if(fadingMode == FadingMode.Image) StartCoroutine(BeginFade(m_FadeOutColor, m_FadeColor, m_FadeDuration, 0));
			else if(fadingMode == FadingMode.FX) StartCoroutine(BeginFadeFX(0,10,0,1,m_FadeDuration, 0));
		}
		
	}


	public void FadeIn()
	{
				
		Debug.Log("Fade in request ...");
		// If not already fading start a coroutine
		if (m_IsFadingOut) {
			print("Already Fading Out, time remaining : "+(m_timeRemaning));
			Invoke("DelayedFadeIn", m_FadeDuration);
		}
		else if(m_IsFadingIn) return;
		else{
			if(fadingMode == FadingMode.Image) StartCoroutine(BeginFade(m_FadeOutColor, m_FadeColor, m_FadeDuration, 1));
			else if(fadingMode == FadingMode.FX) StartCoroutine(BeginFadeFX(10,0,1,0,m_FadeDuration, 1));
		}
	}

	void DelayedFadeIn(){
		FadeIn();
	}

	void DelayedFadeOut(){
		FadeOut();
	}

	private IEnumerator BeginFade(Color startCol, Color endCol, float duration, float fadeType)
	{
		// Fading is now happening.  This ensures it won't be interupted by non-coroutine calls.
		if(fadeType == 1) m_IsFadingIn = true;
		else if (fadeType == 0) m_IsFadingOut = true;
		
		// Execute this loop once per frame until the timer exceeds the duration.
		m_timer = 0f;

		while (timer <= duration + 0.1f)
		{
			// Set the colour based on the normalised time.
			m_FadeImage.color = Color.Lerp(startCol, endCol, timer / duration);
			
			// Increment the timer by the time between frames and return next frame.
			m_timer += Time.deltaTime;
			yield return null;
		}
		m_FadeImage.color = endCol;

		if(fadeType == 1) m_IsFadingIn = false;
		else if (fadeType == 0) m_IsFadingOut = false;
		
		if(gameEngine.pendingVisualisationMode != "none"){
			gameEngine.userManager.ChangeVisualisationMode(gameEngine.pendingVisualisationMode, gameEngine, false);
			FadeIn();
			gameEngine.pendingVisualisationMode = "none";
		} 
	}


	private IEnumerator BeginFadeFX(float bloomStartValue, float bloomEndValue, float chromaStartValue, float chromaEndValue, float duration, float fadeType)
	{
		// Fading is now happening.  This ensures it won't be interupted by non-coroutine calls.
		if(fadeType == 1) m_IsFadingIn = true;
		else if (fadeType == 0) m_IsFadingOut = true;
		
		// Execute this loop once per frame until the timer exceeds the duration.
		m_timer = 0f;


		while (timer <= duration + 0.1f)
		{
			chromaticAberration.intensity.value = Mathf.Lerp(chromaStartValue, chromaEndValue, timer / duration);
			bloomLayer.intensity.value = Mathf.Lerp(bloomStartValue, bloomEndValue, timer / duration);
			
			// Increment the timer by the time between frames and return next frame.
			m_timer += Time.deltaTime;
			yield return null;
		}
		chromaticAberration.intensity.value = chromaEndValue;
		bloomLayer.intensity.value = bloomEndValue;
		
		if(fadeType == 1) m_IsFadingIn = false;
		else if(fadeType == 0) m_IsFadingOut = false;

		if(gameEngine.pendingVisualisationMode != "none"){
			gameEngine.userManager.ChangeVisualisationMode(gameEngine.pendingVisualisationMode, gameEngine, false);
			FadeIn();
			gameEngine.pendingVisualisationMode = "none";
		} 
	}


}