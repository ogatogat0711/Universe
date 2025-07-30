using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class InformationWindow : MonoBehaviour
{
    public Probe probe;
    public GameObject infoWindow;
    public TMP_Text infoText;
    private CelestialBodyData _targetData;
    public static bool isShowing;//表示中かどうかのフラグ
    public Button closeButton;//閉じるボタン
    public PlayableDirector appearingDirector;//出現時アニメション
    public PlayableDirector disappearingDirector;//消失時アニメション

    void Start()
    {
        infoText.text = "";
        infoWindow.SetActive(false);
        _targetData = new CelestialBodyData();
        isShowing = false;
        appearingDirector.Stop();
        disappearingDirector.Stop();
        infoText.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
    }

    public void SetInformation(CelestialBodyData data)
    {
        if (_targetData.Equals(data))
        {
            // Debug.Log("This information is already set.");
            return;
        }
        _targetData = data;
        MakeDescription();
    }

    private void MakeDescription()
    {
        string text = $"天体の名称：{_targetData.bodyName}\n" 
                      + $"天体の区分：{_targetData.type}\n"
                      +"重力の強さ：";

        if (_targetData.gravitationCoefficient <= 10f)
        {
            text += "非常に弱い\n";
        }
        else if(10f < _targetData.gravitationCoefficient && _targetData.gravitationCoefficient <= 50f)
        {
            text += "弱い\n";
        }
        else if (50f < _targetData.gravitationCoefficient && _targetData.gravitationCoefficient <= 200f)
        {
            text += "ほどほど\n";
        }
        else if(200 < _targetData.gravitationCoefficient && _targetData.gravitationCoefficient <= 1000f)
        {
            text += "強い\n";
        }
        else if(1000 < _targetData.gravitationCoefficient && _targetData.gravitationCoefficient <= 2500f)
        {
            text += "非常に強い\n";
        }
        else
        {
            text += "極めて強い\n";
        }

        if (_targetData.bodyName == probe.collisionTarget.name)
        {
            text += "今回の目標天体です\n";
        }
        
        infoText.text = text;
    }
    
    public IEnumerator ShowInfoWindow()
    {
        infoWindow.SetActive(true);
        appearingDirector.Play();
        while (appearingDirector.state == PlayState.Playing)
        {
            yield return null;
        }
        
        closeButton.gameObject.SetActive(true);
        infoText.gameObject.SetActive(true);
        isShowing = true;
    }

    public void OnClickCloseButton()
    {
        StartCoroutine(HideInfoWindow());
    }
    
    public IEnumerator HideInfoWindow()
    {
        infoText.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
        disappearingDirector.Play();
        
        while (disappearingDirector.state == PlayState.Playing)
        {
            yield return null;
        }
        
        infoWindow.SetActive(false);
        isShowing = false;
    }
}
