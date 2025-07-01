using System;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
    private bool _hasWarnedForHalfFuel; // 半分の燃料を警告したかどうかのフラグ
    public Image fuelGauge;
    public Image recoveryTimerGauge; // 復帰タイマーゲージ
    public TMP_Text timerText;// タイマー表示用のテキスト
    private int _timerMinutes; // タイマーの分
    private float _timerSeconds;// タイマーの秒
    private float _formerSeconds;// 前の秒数を保持する変数
    public Navigation navigationForUpper; //上方カメラ用のナビゲーション
    public Navigation navigationForFollowing; //追従カメラ用のナビゲーション
    public Canvas normalCanvas; // 通常表示用のCanvas
    public Canvas resultCanvas; // 結果表示用のCanvas
    public TMP_Text gameOverText;
    public TMP_Text gameOverResultText;
    public TMP_Text clearText;
    public TMP_Text clearResultText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        normalCanvas.enabled = false;
        resultCanvas.enabled = false; // 結果表示用のCanvasを無効化
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
        _hasWarnedForHalfFuel = false;
        
        _timerMinutes = 0; // タイマーの初期値
        _timerSeconds = 0f;
        _formerSeconds = 0f; // 前の秒数を初期化
        
        timerText.text = "00:00";
        navigationForUpper.navigationText.text = "";
        navigationForUpper.ShowMessage("マウスを使って予定航路を描きましょう！\n");
        navigationForFollowing.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(probe.transform.position, _mover.drawLine.GetPosition(0)) < 1f && !_didStartOnce && !DrawLine.IsDrawing)
        {
            startButton.interactable = true;// UIボタンを有効化
            if (Vector3.Distance(_mover.drawLine.GetPosition(_mover.drawLine.positionCount - 1) , probe.collisionTarget.transform.position) > probe.collisionTarget.transform.localScale.x)
            {  // 航路の終点がターゲットの半径より遠いとき
                navigationForUpper.ShowMessage("航路の終点が目標から遠いようです！\n"
                                        + "もう一回航路を見直すのを推奨します！\n");
            }
            else
            {
                navigationForUpper.ShowMessage("準備はできましたか？\n"
                                       + "「スタート」ボタンを押して、出発しましょう！\n"
                                       + "この航路の最低消費燃料の理論値は"
                                       + _mover.drawLine.positionCount
                                       + "です");
            }
            
        }
        else
        {
            startButton.interactable = false;

            if (Vector3.Distance(probe.transform.position, _mover.drawLine.GetPosition(0)) > 1f && !DrawLine.IsDrawing && _didDrawOnce)
            {
                navigationForUpper.ShowMessage("予定航路の始点が探査機から遠いところにあるみたいです・・・\n"
                                                + "もう少し探査機の近傍から描いてみてください！");
            }
        }

        if (_mover.isMoving)
        { //自動航行中
            navigationForFollowing.ShowMessage("自動航行中です！\n");
        }

        if (_mover.wasFarAway)
        {
            if (!_mover.canAutoMove)
            {
                navigationForFollowing.ShowMessage("予定航路から大きく離れました！\n"
                                                   + "直ちに手動操縦で予定航路に接近してください！\n"
                                                   + "(WASDキーで手動操縦)");
            }

            else if (_mover.canAutoMove)
            {
                navigationForFollowing.ShowMessage("復帰準備完了です！\n"
                                                   + "(Rキーで自動航行に復帰)");
            }

            recoveryTimerGauge.enabled = _mover.isRecovering;
            if (recoveryTimerGauge.enabled)
            {
                recoveryTimerGauge.fillAmount = _mover.nearLineTimer / _mover.reenableAutoMoveTime; // 復帰タイマーゲージの更新
            }
        
        }

        if (probe.fuel > 0 && _isPlaying)
        {
            fuelText.text = probe.fuel.ToString();// 燃料を表示
            fuelGauge.fillAmount = (float)probe.fuel / probe.maxFuel; // 燃料ゲージの更新

            if ((float)probe.fuel / probe.maxFuel < 0.5f && !_hasWarnedForHalfFuel)
            {
                navigationForFollowing.ShowMessage("燃料が半分を切りました！\n"
                                                +"残量に留意ください！");
                _hasWarnedForHalfFuel = true; // 半分の燃料を警告したのでフラグを立てる
            }
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

        if (probe.isClear)
        {
            _isPlaying = false;
            Clear();
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
        _mover.isEnableFollowing = true;// 追従カメラを有効にしたのでフラグを立てる
        ChangeCamera(upperCamera, followingCamera);// 上方カメラから追従カメラに切り替え
        navigationForFollowing.enabled = true;//ナビゲーションを有効化
        navigationForFollowing.navigationText.text = "";
        upperCamera.enabled = false;// 上方カメラを無効化
        navigationForUpper.enabled = false;//上方カメラのナビゲーションを無効化
        normalCanvas.enabled = true;
        resultCanvas.enabled = false;
        
        probe.canMove = true;// Probeの移動を有効化
        recoveryTimerGauge.enabled = false;// 復帰タイマーゲージは最初は無効化

        fuelText.text = probe.maxFuel.ToString(); // 初期燃料を表示
        
    }

    private void GameOver()
    {
        normalCanvas.enabled = false;
        resultCanvas.enabled = true; // 結果表示用のCanvasを有効化
        clearText.enabled = false;
        
        float distanceToTarget = Vector3.Distance(probe.transform.position, probe.collisionTarget.transform.position);
        gameOverResultText.text += distanceToTarget.ToString("F2") + "万km\n";
        Time.timeScale = 0;
        //Debug.Log("Game Over");
    }

    public void Retry()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 現在のシーンを再読み込み
    }

    private void Clear()
    {
        Time.timeScale = 0;
        normalCanvas.enabled = false;
        resultCanvas.enabled = true; // 結果表示用のCanvasを有効化
        clearText.enabled = true;
        gameOverText.enabled = false; // ゲームオーバーテキストを非表示
        gameOverResultText.enabled = false;
        float fuelPercentage = (float)probe.fuel / probe.maxFuel * 100f;
        clearResultText.text = "Time:" + _timerMinutes.ToString("00") + ":" + ((int)_timerSeconds).ToString("00") + "\n"
                          + "Remaining Fuel:" + fuelPercentage.ToString("00") + "%\n";
        
        int timeInSeconds = _timerMinutes * 60 + (int)_timerSeconds;
        int score = probe.fuel / probe.maxFuel * 1000;
        score += (10000 / timeInSeconds);

        clearResultText.text += "Score:" + score.ToString() + "\n";
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
