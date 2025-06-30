using System;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private GameObject[] _celestialBodies;
    public Probe probe;
    public Camera upperCamera; // 上方カメラ
    public Camera followingCamera; // 追従カメラ
    public Button startButton; // UIボタン
    public TMP_Text fuelText; // 燃料表示用のテキスト
    private MoveAlongLine _mover;
    public DrawLine draw;
    private bool _didStartOnce;//一回スタートしたかを管理するフラグ
    private bool _didDrawOnce;//一回描画したかを管理するフラグ
    private bool _isPlaying;// ゲームがプレイ中かどうかのフラグ
    public Image fuelGauge;
    public TMP_Text timerText;// タイマー表示用のテキスト
    private int _timerMinutes; // タイマーの分
    private float _timerSeconds;// タイマーの秒
    private float _formerSeconds;// 前の秒数を保持する変数
    public Navigation navigation; // ナビゲーション
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        followingCamera.enabled = false;// 追従カメラは最初は無効化
        startButton.interactable = false;// UIボタンは最初は無効化
        _celestialBodies = GameObject.FindGameObjectsWithTag("CelestialBody");
        foreach (var cb in _celestialBodies)
        {
            CelestialBody celestialBody = cb.GetComponent<CelestialBody>();
            celestialBody.isGravitation = false;//最初は万有引力を無効化
            if (celestialBody.isSpinOrbital)
            {
                celestialBody.isSpinOrbital = false;//公転を無効化
                celestialBody.wasSetSpinOrbital = true;//公転するという情報は保持したまま
            }
        }
        probe.canMove = false;//最初は移動を無効化
        _mover = probe.GetComponent<MoveAlongLine>();
        _didStartOnce = false;
        _didDrawOnce = false;
        _isPlaying = true;
        
        _timerMinutes = 0; // タイマーの初期値
        _timerSeconds = 0f;
        _formerSeconds = 0f; // 前の秒数を初期化
        
        timerText.text = "00:00";
        navigation.navigationText.text = "";
        navigation.ShowMessage("マウスを使って予定航路を描きましょう！\n");
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(probe.transform.position, _mover.drawLine.GetPosition(0)) < 1f && !_didStartOnce && !DrawLine.IsDrawing)
        {
            startButton.interactable = true;// UIボタンを有効化
            if (draw.GetLineLength() < probe.GetDistanceToTheTarget())
            {
                navigation.ShowMessage("なんか航路が短くないですか？\n"
                                        + "もう一回航路を見直した方がいい気がします・・・\n");
            }
            else
            {
                navigation.ShowMessage("準備はできましたか？\n"
                                       + "「スタート」ボタンを押して、出発しましょう！\n");
            }
            
        }
        else
        {
            startButton.interactable = false;

            if (Vector3.Distance(probe.transform.position, _mover.drawLine.GetPosition(0)) > 1f && !DrawLine.IsDrawing && _didDrawOnce)
            {
                navigation.ShowMessage("予定航路の始点が探査機から遠いところにあるみたいです・・・\n"
                                       + "もう少し探査機の近傍から描いてみてください！");
                _didDrawOnce = true;
            }
        }

        if (probe.fuel > 0 && _isPlaying)
        {
            fuelText.text = probe.fuel.ToString();// 燃料を表示
            fuelGauge.fillAmount = (float)probe.fuel / probe.maxFuel; // 燃料ゲージの更新
        }

        if (followingCamera.enabled)
        {
            UpdateTimer();
        }

        if (probe.fuel <= 0 && _isPlaying)//燃料が0以下になったらゲームオーバー
        {
            fuelText.text = "0";// 燃料を0に表示
            fuelGauge.fillAmount = 0; // 燃料ゲージを0に更新
            _isPlaying = false;// ゲームプレイ中フラグをfalseにする
            GameOver();// ゲームオーバー処理を呼び出す
        }
        
        
    }
    
    //UIボタンから呼び出されるメソッド
    public void StartMovingOfProbe()
    {
        foreach (var cb in _celestialBodies)
        {
            CelestialBody celestialBody = cb.GetComponent<CelestialBody>();
            celestialBody.isGravitation = true;//万有引力を有効化
            if (celestialBody.wasSetSpinOrbital)//公転するという情報がある場合
            {
                celestialBody.isSpinOrbital = true;//公転を有効化
            }
        }
        
        _didStartOnce = true;
        
        _mover.canAutoMove = true;
        _mover.isMoving = true;
        _mover.currentIndex = 0;
        
        followingCamera.enabled = true;// 追従カメラを有効化
        ChangeCamera(upperCamera, followingCamera);// 上方カメラから追従カメラに切り替え
        upperCamera.enabled = false;// 上方カメラを無効化
        probe.canMove = true;// Probeの移動を有効化

        fuelText.text = probe.maxFuel.ToString(); // 初期燃料を表示
    }

    private void GameOver()
    {
        Time.timeScale = 0;
        Debug.Log("Game Over");
    }
    
    private void ChangeCamera(Camera oldCamera, Camera newCamera)
    {
        if (oldCamera != null)
        {
            oldCamera.gameObject.SetActive(false);
        }
        
        if (newCamera != null)
        {
            newCamera.gameObject.SetActive(true);
        }
    }

    private void UpdateTimer()
    {
        _timerSeconds += Time.deltaTime;

        if (_timerSeconds >= 60f)// 60秒を超えたら分を加算
        {
            _timerMinutes++;
            _timerSeconds -= 60f;
        }
        // 秒数が異なったときのみタイマーの表示を更新
        if ((int)_timerSeconds != (int)_formerSeconds)
        {
            timerText.text = _timerMinutes.ToString("00") + ":" + ((int)_timerSeconds).ToString("00");
        }
        _formerSeconds = _timerSeconds; // 前の秒数を更新
    }

}
