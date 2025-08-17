using System;
using System.Collections;
using System.IO;
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomizeProbe : MonoBehaviour
{
    public GameObject probe;
    public float rotationSpeed;
    public Image firstWindow;
    public Image performanceWindow;
    public Image shotWindow;
    public ShotDataList shotDataList;
    private Image _currentPanel;
    private string[] _setData;
    public GameObject scrollContentPrefab;
    public Transform scrollContentRoot;
    public Image currentShotInfo;
    public Image tempShotInfo;
    public Button setShotButton;
    public Image shotDialogueWindow;
    public Image loadingBackground;
    public Sprite[] loadingSprites;
    public Image loadingGauge;
    public TMP_Text gold, price;
    public Image performanceInfo;
    public PerformanceLevelGauge fuelLevel, speedLevel, fuelRatioLevel;
    public FillGauge fuelGauge,speedGauge,fuelRatioGauge;

    void Start()
    {
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);
        
        firstWindow.gameObject.SetActive(true);
        performanceWindow.gameObject.SetActive(false);
        shotWindow.gameObject.SetActive(false);

        _currentPanel = firstWindow;

        string probeData = PlayerPrefs.GetString("probeData", "");
        if (probeData == "")
        {
            throw new NullReferenceException();
        }

        _setData = probeData.Split('|');
        if (_setData.Length != 4)
        {
            throw new InvalidDataException("probeDataの長さが不正です");
        }

        string shotUnlockData = PlayerPrefs.GetString("shotUnlockData", "");

        if (shotUnlockData == "")
        {
            string initialUnlockData = "1"; //データの初期化
            //解放済みは1, 未開放は0の文字列を登録 文字のインデックスがshotIDに対応
            for (int i = 1; i < shotDataList.shotDataList.Count; i++)
            {
                initialUnlockData += shotDataList.shotDataList[i].isUnlock ? "1" : "0";
            }

            PlayerPrefs.SetString("shotUnlockData", initialUnlockData);
            shotDataList.FindShotDataById(0).isUnlock = true; //初回時はID0のshotのみ解放
        }
        else
        {
            if (shotUnlockData.Length < shotDataList.shotDataList.Count) //後からデータ追加したとき
            {
                int diff = shotDataList.shotDataList.Count - shotUnlockData.Length;
                for (int i = 0; i < diff; i++)
                    shotUnlockData += "0"; //後ろに足りなかった分だけ0を追加(最初は未開放)

                PlayerPrefs.SetString("shotUnlockData", shotUnlockData);

                ShotUnlock(shotUnlockData);
            }

            else if (shotUnlockData.Length > shotDataList.shotDataList.Count) //読み取りデータが想定より長いとき
            {
                throw new InvalidDataException("shotUnlockDataが想定より長いです");
            }

            else
            {
                ShotUnlock(shotUnlockData);
            }
        }

        tempShotInfo.gameObject.SetActive(false);
        setShotButton.interactable = false;

        shotDialogueWindow.rectTransform.anchoredPosition = new Vector2(0f, Screen.height);//画面上部に初期化
        shotDialogueWindow.gameObject.SetActive(false);
        
        loadingGauge.fillAmount = 0f;
        loadingBackground.gameObject.SetActive(false);

        int goldValue = PlayerPrefs.GetInt("gold", 0);
        gold.text = goldValue.ToString();
    }

    private void InitShotMenu()
    {
        foreach (Transform child in scrollContentRoot.transform)
        {
            Destroy(child.gameObject);//子要素を削除
        }
        
        for (int i = 0; i < shotDataList.shotDataList.Count; i++)
        {
            GameObject scroll = Instantiate(scrollContentPrefab, scrollContentRoot);
            TMP_Text[] scrollTexts = scroll.GetComponentsInChildren<TMP_Text>();
            ShotData shotData = shotDataList.FindShotDataById(i);
            foreach (var text in scrollTexts)
            {
                if (text.name == "ShotName") text.text = shotData.shotName;
                else if (text.name == "ShotPrice")
                {
                    if (shotData.isUnlock) text.text = "―";
                    else
                    {
                        text.text = shotData.price + "G";
                    }
                }
            }

            Button button = scroll.GetComponentInChildren<Button>();
            button.onClick.AddListener(()=>
            {
                SetTempWindow(shotData.shotID);
                if (!shotData.isUnlock)
                {
                    price.text = (-shotData.price).ToString();
                }
            });
        }

    }

    private void InitShotInfo(int shotId, Image shotInfo)
    {
        ShotData shotData = shotDataList.FindShotDataById(shotId);
        
        TMP_Text[] shotInfos = shotInfo.GetComponentsInChildren<TMP_Text>();
        foreach (var text in shotInfos)
        {
            if (text.name == "ShotName") text.text = shotData.shotName;
            else if (text.name == "ShotAttack")
            {
                Image[] images = text.GetComponentsInChildren<Image>();
                foreach (var image in images)
                {
                    if (image.name == "Fill") image.fillAmount = shotData.attack / 20f; //最大値を仮に100としてfill
                }
            }
            else if (text.name == "ShotSpeed")
            {
                Image[] images = text.GetComponentsInChildren<Image>();
                foreach (var image in images)
                {
                    if (image.name == "Fill") image.fillAmount = shotData.speed / 50f;//仮に最大値を50としてfill
                }
            }
            else if (text.name == "ShotInterval")
            {
                Image[] images = text.GetComponentsInChildren<Image>();
                foreach (var image in images)
                {
                    if (image.name == "Fill") image.fillAmount =  1f - shotData.shotInterval;//仮に最大値を0としてfill
                }
            }
            else if (text.name == "ShotFuel")
            {
                Image[] images = text.GetComponentsInChildren<Image>();
                foreach (var image in images)
                {
                    if (image.name == "Fill") image.fillAmount =  shotData.fuelConsumptionOfShot / 20f;//仮に最大値を20としてfill
                }
            }
            else if (text.name == "ShotID")
            {
                text.text = shotData.shotID.ToString();
            }
            else if (text.name == "CurrentShotText" || text.name == "TempShotText")
            {
                continue;
            }
            else
            {
                text.text = "Error!";
            }
        }
    }

    private void SetTempWindow(int shotID)
    {
        InitShotInfo(shotID, tempShotInfo);
        tempShotInfo.gameObject.SetActive(true);
    }

    void Update()
    {
        probe.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        if (shotWindow.gameObject.activeInHierarchy && tempShotInfo.gameObject.activeInHierarchy)
        {
            int currentShotId = GetShotID(currentShotInfo);
            int tempShotID = GetShotID(tempShotInfo);
            ShotData shotData = shotDataList.FindShotDataById(tempShotID);
            if (!shotData.isUnlock)
            {
                setShotButton.interactable = (int.Parse(gold.text) - shotData.price) >= 0;//まだアンロックされていないなら所持金を確認
            }
            else
            {
                setShotButton.interactable = (currentShotId != tempShotID);//アンロック済みならばIDが違えばOK
            }
        }

        price.gameObject.SetActive(int.Parse(price.text) < 0);//テキストが負の整数ならアクティブ化
    }

    private int GetShotID(Image window)
    {
        int id = 0;
        TMP_Text[] infos = window.GetComponentsInChildren<TMP_Text>();

        foreach (var text in infos)
        {
            if (text.name == "ShotID") id = int.Parse(text.text);
        }

        return id;
    }

    private void ShotUnlock(string shotUnlockData)
    {
        for (int i = 0; i < shotUnlockData.Length; i++)
        {
            if (shotUnlockData[i] == '1') shotDataList.FindShotDataById(i).isUnlock = true;
            else if (shotUnlockData[i] == '0') shotDataList.FindShotDataById(i).isUnlock = false;
            else throw new InvalidDataException("shotUnlockDataがインデックス" + i + "で不正です");
        }
    }

    public void OnPerformanceButtonClicked()
    {
        InitPerformanceInfo();
        
        fuelLevel.InitLevelGauge(int.Parse(_setData[0]));
        speedLevel.InitLevelGauge(int.Parse(_setData[1]));
        fuelRatioLevel.InitLevelGauge(int.Parse(_setData[2]));
        
        StartCoroutine(TransitionPanel(performanceWindow));
    }

    public void OnShotButtonClicked()
    {
        InitShotInfo(int.Parse(_setData[3]), currentShotInfo);
        tempShotInfo.gameObject.SetActive(false);
        setShotButton.interactable = false;
        InitShotMenu();
        StartCoroutine(TransitionPanel(shotWindow));
    }

    public void OnBackButtonClicked()
    {
        StartCoroutine(TransitionPanel(firstWindow));
    }

    private IEnumerator TransitionPanel(Image transitionPanel)
    {
        transitionPanel.gameObject.SetActive(true);
        
        price.text = "0";//パネル移動ごとにpriceを更新
        
        float transitionValue = _currentPanel.rectTransform.anchoredPosition.x -
                                transitionPanel.rectTransform.anchoredPosition.x;
        _currentPanel.rectTransform.DOAnchorPosX(transitionValue, 0.8f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.5f);
        transitionPanel.rectTransform.DOAnchorPosX(0f, 0.8f).SetEase(Ease.OutCubic);
        _currentPanel.gameObject.SetActive(false);
        _currentPanel = transitionPanel;
        
    }

    public void OnShotSetButtonClicked()
    {
        StartCoroutine(AppearWindow(shotDialogueWindow));
        Button[] buttons = shotDialogueWindow.GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            if (button.name == "Yes")
            {
                button.onClick.AddListener(() =>
                {
                    int shotID = GetShotID(tempShotInfo);
                    ShotData shotData = shotDataList.FindShotDataById(shotID);
                    if (!shotData.isUnlock)
                    {
                        int goldValue = int.Parse(gold.text) - shotData.price;
                        gold.text = goldValue.ToString();
                        PlayerPrefs.SetInt("gold", goldValue);
                        StartCoroutine(FadeOutPrice());
                        
                        string shotUnlockData = PlayerPrefs.GetString("shotUnlockData", "");
                        StringBuilder sb = new StringBuilder(shotUnlockData);
                        sb[shotID] = '1';
                        shotUnlockData = sb.ToString();
                        
                        PlayerPrefs.SetString("shotUnlockData", shotUnlockData);
                        ShotUnlock(shotUnlockData);
                        InitShotMenu();
                    }
                    _setData[3] = shotID.ToString();//shotIDを変更
                    string probeData = ConcatProbeData();//probeDataを構成
                    PlayerPrefs.SetString("probeData", probeData);
                    StartCoroutine(DisappearWindow(shotDialogueWindow));
                    
                    tempShotInfo.gameObject.SetActive(false);
                    setShotButton.interactable = false;
                    InitShotInfo(shotID, currentShotInfo);
                });
            }
            else if (button.name == "No")
            {
                button.onClick.AddListener(() =>
                {
                    StartCoroutine(DisappearWindow(shotDialogueWindow));
                });                
            }
        }
    }

    private IEnumerator FadeOutPrice()
    {
        Vector2 originalPosition = price.rectTransform.anchoredPosition;
        price.rectTransform.DOAnchorPosY(20f, 0.2f).SetEase(Ease.OutCubic);
        price.DOFade(0f,0.2f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.2f);
        price.text = "0";
        price.rectTransform.anchoredPosition = originalPosition;
        price.gameObject.SetActive(false);
        price.alpha = 1f;
    }

    private IEnumerator AppearWindow(Image dialogue)
    {
        dialogue.gameObject.SetActive(true);
        dialogue.rectTransform.DOAnchorPosY(0f, 0.5f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator DisappearWindow(Image dialogue)
    {
        dialogue.rectTransform.DOAnchorPosY(Screen.height, 0.8f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.5f);
        dialogue.gameObject.SetActive(false);
    }

    private string ConcatProbeData()
    {
        string dataString = "";
        for (int i = 0; i < _setData.Length; i++)
        {
            if (i != _setData.Length - 1) dataString += _setData[i] + "|";
            else dataString += _setData[i];
        }
        
        return dataString;
    }

    private void InitPerformanceInfo()
    {
        int maxFuelLevelValue = int.Parse(_setData[0]);
        int speedLevelValue = int.Parse(_setData[1]);
        int fuelRatioLevelValue = int.Parse(_setData[2]);

        TMP_Text[] perfInfo = performanceInfo.GetComponentsInChildren<TMP_Text>();

        float fuelFill = (float)Math.Max(2 * maxFuelLevelValue - speedLevelValue + 1, 1) / 20;
        float speedFill = (float)Math.Max(2 * speedLevelValue - fuelRatioLevelValue + 1, 1) / 20;
        float fuelRatioFill = (float)Math.Max(maxFuelLevelValue + speedLevelValue - fuelRatioLevelValue + 1, 1) / 20;
        
        fuelGauge.SetFill(fuelFill,0.3f);
        speedGauge.SetFill(speedFill,0f);
        fuelRatioGauge.SetFill(fuelRatioFill, 0f);
    }

    public void OnBackToSelectButtonClicked()
    {
        int spriteIndex = UnityEngine.Random.Range(0, loadingSprites.Length);
        loadingBackground.sprite = loadingSprites[spriteIndex];
        loadingBackground.gameObject.SetActive(true);

        StartCoroutine(LoadScene("SelectScene"));
    }
    
    private IEnumerator LoadScene(string sceneName)
    {
        yield return new WaitForSeconds(1f);
        
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        
        if (async == null)
        {
            throw new Exception("Failed to load scene");
        }

        while (!async.isDone)
        {
            loadingGauge.fillAmount = Mathf.Clamp01(async.progress / 0.9f);
            yield return null;
        }
    }

}
