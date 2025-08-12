using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    private Image[] _containerPanels;
    public Image containerPanelPrefab;
    private Image[] _levelPanels;
    public Image levelPanelPrefab;
    public LevelData levelData;
    private List<Level> _levels;
    private int _currentContainerIndex;
    private Button _nextButton;
    private Button _backButton;
    public Image blackBackground;
    public GameObject panelRoot;
    public Image loadingBackground;
    public Sprite[] loadingSprites;
    public Image loadingGauge;
    public Button backToTitleButton;
    private bool _isTransferring;
    public Image windowPanel;
    private bool _isWindowOpen;

    void Awake()
    {
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        
        loadingBackground.gameObject.SetActive(false);
        loadingGauge.fillAmount = 0f;
        
        _levels = levelData.levelList;
        _levelPanels = new Image[_levels.Count];
        int pages = (_levelPanels.Length % 3 == 0) ? _levelPanels.Length / 3 : _levelPanels.Length / 3 + 1;//3つで1ページ

        _containerPanels = new Image[pages];
        for (int i = 0; i < pages; i++)
        {
            _containerPanels[i] = Instantiate(containerPanelPrefab);
            _containerPanels[i].transform.SetParent(panelRoot.transform);
            _containerPanels[i].rectTransform.offsetMin = Vector2.zero;
            _containerPanels[i].rectTransform.offsetMax = new Vector2(1f, 1f);
            if (i > 0)
            {
                _containerPanels[i].rectTransform.anchoredPosition = new Vector2(Screen.width, 0f);//二枚目以降は画面右に配置
            }
            _containerPanels[i].gameObject.SetActive(false);
            
            _containerPanels[i].name = "ContainerPanel" + (i + 1);
        }
        _containerPanels[0].rectTransform.anchoredPosition = new Vector2(0f, 0f);//一個目は真ん中に設置

        for (int i = 0; i < _levels.Count; i++)
        {
            int pageNumber = i / 3;

            Vector2 anchoredPosition = (i % 3) switch
            {
                0 => new Vector2(-levelPanelPrefab.rectTransform.rect.width, 0f),
                1 => new Vector2(0f, 0f),
                2 => new Vector2(levelPanelPrefab.rectTransform.rect.width, 0f),
                _ => Vector2.zero
            };

            _levelPanels[i] = Instantiate(levelPanelPrefab);
            _levelPanels[i].transform.SetParent(_containerPanels[pageNumber].transform);
            _levelPanels[i].transform.SetAsFirstSibling();
            _levelPanels[i].rectTransform.anchoredPosition = anchoredPosition;

            string sceneName = _levels[i].sceneName;
            int index = i;
            int levelId = _levels[i].levelId;
            _levelPanels[i].gameObject.GetComponentInChildren<Button>().onClick
                .AddListener(() => OnStageButtonClicked(sceneName, index, levelId));

            if (_levels[i].thumbnail != null)
            {
                var thumbnail = _levelPanels[i].transform.GetChild(0).GetComponent<Image>();
                thumbnail.sprite = _levels[i].thumbnail;
            }

            string text = "Stage " + _levels[i].levelId;
            int highScore = PlayerPrefs.GetInt("HighScore" + _levels[i].levelId, 0);
            text += "\nHigh Score: " + highScore;

            _levelPanels[i].GetComponentInChildren<TMP_Text>().text = text;

            _levelPanels[i].name = "LevelPanel" + i;
        }

        _currentContainerIndex = 0;
        
        SetButtons(_currentContainerIndex);
        _backButton.interactable = false;
        
        _containerPanels[_currentContainerIndex].gameObject.SetActive(true);

        _isTransferring = false;

        _isWindowOpen = false;
    }

    IEnumerator Start()
    {
        blackBackground.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);//少し待機
        blackBackground.rectTransform.DOAnchorPosX(-Screen.width, 0.5f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.5f);
        blackBackground.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_currentContainerIndex == _containerPanels.Length - 1 && _nextButton.interactable)
        {
            _nextButton.interactable = false;
        }

        if (_currentContainerIndex == 0 && _backButton.interactable)
        {
            _backButton.interactable = false;
        }
        
        
        backToTitleButton.interactable = !(_isTransferring || _isWindowOpen);
        // Debug.Log(_currentContainerIndex);
    }

    private void OnNextButtonClicked()
    {
        _nextButton.interactable = false;
        StartCoroutine(PanelTransition(_currentContainerIndex + 1));
        // _currentContainerIndex++;
        // SetButtons(_currentContainerIndex);
    }

    private void OnBackButtonClicked()
    {
        _backButton.interactable = false;
        StartCoroutine(PanelTransition(_currentContainerIndex - 1));
        // _currentContainerIndex--;
        // SetButtons(_currentContainerIndex);
    }

    private void OnStageButtonClicked(string sceneName, int panelIndex, int levelId)
    {
        int spriteIndex = UnityEngine.Random.Range(0, loadingSprites.Length);
        loadingBackground.sprite = loadingSprites[spriteIndex];

        ResultParameters.levelId = levelId;
        
        StartCoroutine(LoadStageScene(sceneName, panelIndex));
    }

    private IEnumerator LoadStageScene(string sceneName, int panelIndex)
    {
        for (int i = 0; i < _levelPanels.Length; i++)
        {
            if (i != panelIndex)
            {
                foreach(Transform child in _levelPanels[i].transform)
                    Destroy(child.gameObject);//なぜか子オブジェクトが残ったままだったので念のために子要素全て削除
                
                Destroy(_levelPanels[i]);//クリックしたもの以外はすべて削除
            }
        }

        _levelPanels[panelIndex].rectTransform.DOAnchorPos(new Vector2(0f, 5f), 1f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(1.5f);
        
        loadingBackground.gameObject.SetActive(true);
        
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

    private IEnumerator PanelTransition(int transferPanelIndex)
    {
        Image currentPanel = _containerPanels[_currentContainerIndex];
        Image nextPanel = _containerPanels[transferPanelIndex];
        nextPanel.gameObject.SetActive(true);
        _isTransferring = true;

        int transitionCoefficient = _currentContainerIndex - transferPanelIndex;//Nextなら-1, Backなら+1
        
        currentPanel.rectTransform.DOAnchorPos(new Vector2(transitionCoefficient * Screen.width, 0f), 0.8f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.5f);//食い気味に移動
        nextPanel.rectTransform.DOAnchorPos(new Vector2(0f, 0f), 0.8f).SetEase(Ease.OutCubic);

        _currentContainerIndex = transferPanelIndex;
        currentPanel.gameObject.SetActive(false);
        
        SetButtons(_currentContainerIndex);
        _isTransferring = false;
    }

    private void SetButtons(int containerIndex)
    {
        var buttons = _containerPanels[containerIndex].GetComponentsInChildren<Button>();

        foreach (var button in buttons)
        {
            if(button.name == "NextButton") _nextButton = button;
            if(button.name == "BackButton") _backButton = button;
        }
        
        _nextButton.onClick.AddListener(OnNextButtonClicked);
        _backButton.onClick.AddListener(OnBackButtonClicked);

        _nextButton.interactable = true;
        _backButton.interactable = true;
    }

    public void OnBackToTitleButtonClicked()
    {
        StartCoroutine(AppearWindow());
    }

    private IEnumerator AppearWindow()
    {
        _isWindowOpen = true;
        windowPanel.rectTransform.DOAnchorPosY(0f, 0.5f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.5f);
    }

    public void OnCloseButtonClicked()
    {
        StartCoroutine(DisAppearWindow());
        
    }

    private IEnumerator DisAppearWindow()
    {
        windowPanel.rectTransform.DOAnchorPosY(Screen.height, 0.5f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.5f);

        _isWindowOpen = false;
    }

    public void BackToTitle()
    {
        int spriteIndex = UnityEngine.Random.Range(0, loadingSprites.Length);
        loadingBackground.sprite = loadingSprites[spriteIndex];
        loadingBackground.gameObject.SetActive(true);


        StartCoroutine(LoadTitleScene());
    }

    private IEnumerator LoadTitleScene()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync("Title Scene");
        
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
