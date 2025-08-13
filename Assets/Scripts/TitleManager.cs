using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public Image loadingBackground;
    public Sprite[] loadingSprites;
    public Image loadingGauge;
    public Image dialogueWindow;
    public Button okButton;
    public Button yesButton;
    public Button noButton;

    void Start()
    {
        Time.timeScale = 1;
        
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);

        loadingGauge.fillAmount = 0f;
        loadingBackground.gameObject.SetActive(false);
        
        dialogueWindow.gameObject.SetActive(false);
        
        yesButton.onClick.AddListener(() =>
        {
            StartCoroutine(DataReset());
        });
        
        noButton.onClick.AddListener(() =>
        {
            StartCoroutine(DisappearWindow());
        });
        
        okButton.onClick.AddListener(() =>
        {
            StartCoroutine(DisappearWindow());
        });
    }

    public void SceneLoad()
    {
        int spriteIndex = UnityEngine.Random.Range(0, loadingSprites.Length);
        loadingBackground.sprite = loadingSprites[spriteIndex];
        loadingBackground.gameObject.SetActive(true);

        string probeData = PlayerPrefs.GetString("probeData", "");
        if (probeData == "")
        {
            string initialProbeData = "500|300|3|100|1|0";//データの初期化
            //初期燃料|操作時速度|操作時消費燃料|自動時速度|自動時消費燃料|ShotID (要素数6)
            PlayerPrefs.SetString("probeData", initialProbeData);
        }

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

    public void OnResetButtonClicked()
    {
        dialogueWindow.gameObject.SetActive(true);
        TMP_Text dialogue = dialogueWindow.transform.Find("Dialogue").GetComponentInChildren<TMP_Text>();
        
        dialogue.text = "本当にデータをすべてリセットしますか？";
        dialogue.color = Color.red;
        
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
        okButton.gameObject.SetActive(false);
    }

    private IEnumerator DataReset()
    {
        yield return new WaitForSeconds(1f);
        PlayerPrefs.DeleteAll();

        TMP_Text dialogue = dialogueWindow.transform.Find("Dialogue").GetComponentInChildren<TMP_Text>();

        dialogue.text = "データをすべてリセットしました";
        dialogue.color = Color.white;
        
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        okButton.gameObject.SetActive(true);
    }

    private IEnumerator DisappearWindow()
    {
        dialogueWindow.rectTransform.DOAnchorPosY(Screen.height, 0.5f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(0.5f);
        
        dialogueWindow.gameObject.SetActive(false);
        dialogueWindow.rectTransform.anchoredPosition = new Vector2(0f, 0f);
    }
}
