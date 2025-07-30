using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public Image loadingBackground;
    public Slider loadingSlider;
    public TMP_Text loadingText;

    void Start()
    {
        loadingBackground.gameObject.SetActive(false);
        loadingSlider.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(false);
    }

    public void SceneLoad()
    {
        loadingBackground.gameObject.SetActive(true);
        loadingText.gameObject.SetActive(true);
        loadingSlider.gameObject.SetActive(true);

        StartCoroutine(WaitAndLoad(2,"SolarSystem"));
    }

    public void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // エディタでの実行停止
        #else
        Application.Quit(); // ビルド版でのアプリケーション終了
        #endif
    }
    
    IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        
        while (!async.isDone)
        {
            float progress = Mathf.Clamp01(async.progress / 0.9f);
            loadingSlider.value = progress;
            
            yield return null;
        }
        
    }
    
    IEnumerator WaitAndLoad(int seconds, string sceneName)
    {
        yield return new WaitForSeconds(seconds); // 指定された秒数待機
        yield return LoadScene(sceneName); // シーンをロード
    }
}
