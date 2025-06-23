using System;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
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
    private bool _didStartOnce;//一回スタートしたかを管理するフラグ
    private bool _isPlaying;// ゲームがプレイ中かどうかのフラグ
    public Image fuelGauge;
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
        _isPlaying = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(probe.transform.position, _mover.drawLine.GetPosition(0)) < 1f && !_didStartOnce && !DrawLine.IsDrawing)
        {
            startButton.interactable = true;// UIボタンを有効化
        }

        if (probe.fuel > 0 && _isPlaying)
        {
            fuelText.text = probe.fuel.ToString();// 燃料を表示
            fuelGauge.fillAmount = (float)probe.fuel / probe.maxFuel; // 燃料ゲージの更新
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

}
