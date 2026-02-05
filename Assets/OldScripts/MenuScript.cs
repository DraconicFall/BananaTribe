using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    Animator canvasAnim;
    public string sceneName;
    // Start is called before the first frame update
    void Start()
    {
        canvasAnim = GameObject.Find("Canvas").GetComponent<Animator>();
    }

    public void regularButtonInput()
    {
        canvasAnim.Play("FadeInBLACKONLY");
        GameManagerScript.resetAll();
        StartCoroutine(loadSceneTimed(0.45f));
    }

    public void restartLevelButton()
    {
        canvasAnim.Play("FadeInBLACKONLY");
        sceneName = SceneManager.GetActiveScene().name;
        GameManagerScript.resetAll();
        StartCoroutine(loadSceneTimed(0.45f));
    }

    public void buttonQuit()
    {
        Application.Quit();
    }

    IEnumerator loadSceneTimed(float waitTime)
    {
        GameManagerScript.resetAll();
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(sceneName);
    }
}
