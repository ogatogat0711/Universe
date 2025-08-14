using System;
using System.Collections;
using System.IO;
using DG.Tweening;
using TMPro;
using UnityEngine;
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

    void Start()
    {
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
        if (_setData.Length != 6)
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
        
        InitShotMenu();
        InitShotInfo(int.Parse(_setData[5]), currentShotInfo);
        tempShotInfo.gameObject.SetActive(false);
    }

    private void InitShotMenu()
    {
        for (int i = 0; i < shotDataList.shotDataList.Count; i++)
        {
            GameObject scroll = Instantiate(scrollContentPrefab,scrollContentRoot);
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
        StartCoroutine(TransitionPanel(performanceWindow));
    }

    public void OnShotButtonClicked()
    {
        StartCoroutine(TransitionPanel(shotWindow));
    }

    public void OnBackButtonClicked()
    {
        StartCoroutine(TransitionPanel(firstWindow));
    }

    private IEnumerator TransitionPanel(Image transitionPanel)
    {
        transitionPanel.gameObject.SetActive(true);
        float transitionValue = _currentPanel.rectTransform.anchoredPosition.x -
                                transitionPanel.rectTransform.anchoredPosition.x;
        _currentPanel.rectTransform.DOAnchorPosX(transitionValue, 0.8f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.5f);
        transitionPanel.rectTransform.DOAnchorPosX(0f, 0.8f).SetEase(Ease.OutCubic);
        _currentPanel.gameObject.SetActive(false);
        _currentPanel = transitionPanel;
    }

}
