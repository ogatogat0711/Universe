using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public Image loadingBackground;
    public Sprite[] loadingSprites;
    public Image loadingGauge;

    void Start()
    {
        Time.timeScale = 1;
        
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);

        loadingGauge.fillAmount = 0f;
        loadingBackground.gameObject.SetActive(false);
    }

    public void SceneLoad()
    {
        int spriteIndex = UnityEngine.Random.Range(0, loadingSprites.Length);
        loadingBackground.sprite = loadingSprites[spriteIndex];
        loadingBackground.gameObject.SetActive(true);

        StartCoroutine(LoadScene("SelectScene"));
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
        yield return new WaitForSeconds(1f);
        
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);

        if (async == null)
        {
            throw new Exception("Loading scene failed");
        }
        
        while (!async.isDone)
        {
            float progress = Mathf.Clamp01(async.progress / 0.9f);
            loadingGauge.fillAmount = progress;
            
            yield return null;
        }
        
    }
}
