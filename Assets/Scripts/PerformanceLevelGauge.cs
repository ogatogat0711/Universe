using System;
using UnityEngine;
using UnityEngine.UI;

public class PerformanceLevelGauge : MonoBehaviour
{
    public Sprite enableImage, disableImage;
    private Image[] _levelGauge;
    private int _level;
    public Button upButton, downButton;

    private void Start()
    {
        _levelGauge = transform.GetComponentsInChildren<Image>();
        
        upButton.onClick.AddListener(OnUpButtonClicked);
        downButton.onClick.AddListener(OnDownButtonClicked);
    }

    void Update()
    {
        upButton.interactable = _level != 10;
        downButton.interactable = _level != 1;
    }

    public void InitLevelGauge(int level)
    {
        foreach (Image image in _levelGauge)
        {
            int levelCount = int.Parse(image.name);
            if (levelCount <= level)
            {
                image.sprite = enableImage;
            }
            else
            {
                image.sprite = disableImage;
            }
        }
        
        _level = level;
    }

    private void OnUpButtonClicked()
    {
        _level++;
        InitLevelGauge(_level);
    }

    private void OnDownButtonClicked()
    {
        _level--;
        InitLevelGauge(_level);
    }
}
