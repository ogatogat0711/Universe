using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class InformationWindow : MonoBehaviour
{
    public GameObject infoWindow;
    public TMP_Text infoText;
    private CelestialBodyData _targetData;
    public static bool isShowing;//表示中かどうかのフラグ
    // public PlayableDirector appearingDirector;//出現時アニメション
    // public PlayableDirector disappearingDirector;//消失時アニメション

    void Start()
    {
        infoText.text = "";
        infoWindow.SetActive(false);
        _targetData = new CelestialBodyData();
        isShowing = false;
        // appearingDirector.Stop();
        // disappearingDirector.Stop();
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
        
        infoText.text = text;
    }
    
    public void ShowInfoWindow()
    {
        // appearingDirector.Play();
        infoWindow.SetActive(true);
        isShowing = true;
    }
    
    public void HideInfoWindow()
    {
        // disappearingDirector.Play();
        infoWindow.SetActive(false);
        isShowing = false;
    }
}
