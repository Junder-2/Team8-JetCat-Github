using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public static int playerCount;
    private int _currentButtonIndex;
    private float _currentFade;
    private Fade _fade;
    // set to in awake later. 

    private const int Fadein = 1;
    private const int Fadeout = 0;
 

    //playerprefs?

    private void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GameObject.Destroy(this.gameObject);
            return;
        }
    }

    private void OnEnable() {
        _fade = FindObjectOfType<Fade>();
        _fade.gameObject.SetActive(true);
    }

    private void Start()
    {
        if (_fade != null)
            _currentFade = _fade.GetFade();

        StartCoroutine(Fade(Fadein));
    }

    //start new game with int amount of players
    public void PlayGame(int players)
    {
        playerCount = players;
        //SceneManager.LoadScene("Tanakorn");
        LoadScene("Tanakorn");
    }
    
    //pause game - call on pause input not a button hehe
    public void PauseGame()
    {
        if (_paused)
        {
            UnPauseGame();
            return;
        }
        _paused = true;
        
        _fade.SetFade(1);
        _fade.SetAlpha(.5f);
        Time.timeScale = 0; //might be better to tell gamemanager to set timescale depending on gamestate
    }

    private bool _paused = false;
    //resume game
    public void UnPauseGame()
    {
        _paused = false;
        Time.timeScale = 1f; //might be better to tell gamemanager to set timescale depending on gamestate
        _fade.SetFade(0);
        _fade.SetAlpha(1);
    }

    //return to menu
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; //might be better to tell gamemanager to set timescale depending on gamestate
        //SceneManager.LoadScene(0);
        
        LoadScene("MainMenu");
    }

    public void LoadScene(string sceneName)
    {
        if(Fading)
            return;
        
        StopAllCoroutines();
        StartCoroutine(Fade(Fadeout, sceneName));
    }

    public void FadeOut(float fadeSpeed = DefaultFadeSpeed)
    {
        StopAllCoroutines();
        StartCoroutine(Fade(Fadeout, "", fadeSpeed));
    }

    public void FadeIn(float fadeSpeed = DefaultFadeSpeed)
    {
        StopAllCoroutines();
        StartCoroutine(Fade(Fadein, "", fadeSpeed));
    }

    private const float DefaultFadeSpeed = .33f;
    
    private IEnumerator Fade(float currentFade, string sceneName = "", float fadeSpeed = DefaultFadeSpeed) 
    {
        Fading = true;
        WaitForEndOfFrame frameskip = new WaitForEndOfFrame();
        var target = currentFade != Fadeout ? Fadeout : Fadein;
        
        _fade.SetAlpha(1);
        _fade.SetFade(currentFade);

        yield return frameskip;

        float timer = 0;
        while (timer < 1)
        {
            timer += fadeSpeed * Time.unscaledDeltaTime;
            currentFade = Mathf.Lerp(currentFade, target, timer);
            _fade.SetFade(currentFade);
            yield return frameskip;
        }

        currentFade = target;
        _fade.SetFade(currentFade);

        if(sceneName != "")
            SceneManager.LoadScene(sceneName);
        
        Fading = false;
    }

    public bool Fading { get; set; }

    //quit game
    public void QuitGame()
    {
        Application.Quit();
    }
}
