using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MMScript : MonoBehaviour
{
    public AudioClip clip;
    public Animator canvasAnim;
    public GameObject tutorialPanel;
    public MusicManager musicManager;
    public void startButton()
    {
        GameManagerScript.resetAll();
        SoundFXManager.instance.PlaySoundFXClip(clip, transform, 0.1f, 1, 1, false);
        canvasAnim.Play("FadeIn");
        //StartCoroutine(musicManager.FadeOut(musicManager.audio1, .35f));
        StartCoroutine(loadG());

    }

    public void gotoMainMenu()
    {
        GameManagerScript.resetAll();
        //SoundFXManager.instance.PlaySoundFXClip(clip, transform, 0.1f, 1, 1, false);
        //canvasAnim.Play("FadeIn");
        //StartCoroutine(musicManager.FadeOut(musicManager.audio1, .35f));
        StartCoroutine(loadMM());
    }

    public void tutorialButton()
    {
        SoundFXManager.instance.PlaySoundFXClip(clip, transform, 0.1f, 1, 1, false);
        if (tutorialPanel.activeSelf == false)
        {
            tutorialPanel.SetActive(true);
        }
        else
        {
            tutorialPanel.SetActive(false);
        }
    }

    public void quitButton()
    {
        Application.Quit();
    }

    IEnumerator loadMM()
    {
        yield return new WaitForSeconds(.45f);
        GameManagerScript.resetAll();
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator loadG()
    {
        yield return new WaitForSeconds(.45f);
        GameManagerScript.resetAll();
        SceneManager.LoadScene("TheTower");
    }
    IEnumerator loadSceneTimed(float waitTime, string sceneName)
    {
        yield return new WaitForSeconds(waitTime);
        GameManagerScript.resetAll();
        SceneManager.LoadScene(sceneName);
    }
}
