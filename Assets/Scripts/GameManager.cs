using System;
using System.Collections;
using System.Net.Sockets;
using DG.Tweening;
using TMPro;
using Unity.Cinemachine;
using Unity.Properties;
//using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public bool isDebugMode = false; // デバッグモードのフラグ
    public CelestialBodySetting celestialBodySetting; // CelestialBodyのデータリスト
    private GameObject[] _celestialBodies;
    public Probe probe;
    public Camera mainCamera; // メインカメラ
    //public Camera upperCamera; // 上方カメラ
    public CinemachineVirtualCameraBase upperVirtualCamera; // 上方カメラの仮想カメラ
    //public Camera followingCamera; // 追従カメラ
    public CinemachineVirtualCameraBase followingVirtualCamera; // 追従カメラの仮想カメラ
    public CinemachineVirtualCameraBase fpsCamera;
    public Button startButton; // UIボタン
    public TMP_Text fuelTextForFollowing; // 燃料表示用のテキスト
    public TMP_Text fuelTextForFPS;
    private MoveAlongLine _mover;
    public DrawLine draw;
    private LineRenderer _lineRenderer;
    private bool _didDrawOnce;//一回描画したかを管理するフラグ
    public static bool isPlaying;// ゲームがプレイ中かどうかのフラグ
    private bool _hasWarnedForHalfFuel; // 半分の燃料を警告したかどうかのフラグ
    public TMP_Text currentStateText; // 現在の状態を表示するテキスト
    public Image fuelGaugeForFollowing;
    public Image fuelGaugeForFPS;
    public Image recoveryTimerGauge; // 復帰タイマーゲージ
    public TMP_Text timerText;// タイマー表示用のテキスト
    private int _timerMinutes; // タイマーの分
    private float _timerSeconds;// タイマーの秒
    private float _formerSeconds;// 前の秒数を保持する変数
    //public Navigation navigationForUpper; //上方カメラ用のナビゲーション
    public TMP_Text upperNavigationText; // 上方カメラのナビゲーションテキスト
    //public Navigation navigationForFollowing; //追従カメラ用のナビゲーション
    public Canvas upperCanvas; // 上方カメラ用のCanvas
    public Canvas normalFollowingCanvas; // 通常表示用のCanvas
    public Canvas resultCanvas; // 結果表示用のCanvas
    public Canvas fpsCanvas;// FPSカメラ用のCanvas
    public TMP_Text gameOverText;
    // public TMP_Text gameOverResultText;
    public TMP_Text clearText;
    // public TMP_Text clearResultText;
    public Image blackBackground;
    // public TMP_Text loadingText;
    // public Slider loadingSlider;

    private Color _originalStartColor;//LineRendererの開始点の元の色
    private Color _originalEndColor;// LineRendererの終了点の元の色
    private Color _transparentStartColor;// LineRendererの開始点の透明色
    private Color _transparentEndColor;// LineRendererの終了点の透明色

    public int timeScoreInterval;

    public enum GameOverType
    {
        Cleared,//クリア
        RunOut,//燃料切れ
        Devastated//損害率100%
    }
    
    private Coroutine _showingMessageCoroutine;

    // public InformationWindow infoWindow;
    // public GameObject guideForInformation;

    // public PlayableDirector toFpsDirector;//FPSカメラに切り替えたときのアニメーション
    // public PlayableDirector toFollowingDirector;//追従カメラに切り替えたときのアニメーション
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        upperCanvas.gameObject.SetActive(true);
        normalFollowingCanvas.gameObject.SetActive(false);
        resultCanvas.gameObject.SetActive(false); // 結果表示用のCanvasを無効化
        fpsCanvas.gameObject.SetActive(false);
        //followingCamera.enabled = false;// 追従カメラは最初は無効化
        startButton.interactable = false;// UIボタンは最初は無効化
        
        _celestialBodies = GameObject.FindGameObjectsWithTag("CelestialBody");
        foreach (var cb in _celestialBodies)
        {
            CelestialBody celestialBody = cb.GetComponent<CelestialBody>();
            var data = celestialBodySetting.GetDataByName(cb.name);

            if (data != null)
            {
                celestialBody.SetData(data);
            }
            else
            {
                Debug.Log("CelestialBody data not found for: " + cb.name);
            }
            
            celestialBody.isGravitation = false;//最初は万有引力を無効化
            if (celestialBody.isSpinOrbital)
            {
                celestialBody.isSpinOrbital = false;//公転を無効化
                celestialBody.wasSetSpinOrbital = true;//公転するという情報は保持したまま
            }
        }
        probe.canMove = false;//最初は移動を無効化
        
        _mover = probe.GetComponent<MoveAlongLine>();
        _didDrawOnce = false;
        isPlaying = true;
        _hasWarnedForHalfFuel = false;
        
        _timerMinutes = 0; // タイマーの初期値
        _timerSeconds = 0f;
        _formerSeconds = 0f; // 前の秒数を初期化
        
        timerText.text = "00:00";

        _lineRenderer = draw.lineRenderer;
        _originalStartColor = _lineRenderer.startColor;
        _originalEndColor = _lineRenderer.endColor;
        
        _transparentStartColor = new Color(_originalStartColor.r, _originalStartColor.g, _originalStartColor.b, 0f);
        _transparentEndColor = new Color(_originalEndColor.r, _originalEndColor.g, _originalEndColor.b, 0f);
        
        // guideForInformation.SetActive(false);
        
        // toFpsDirector.Stop();
        // toFollowingDirector.Stop();
        
        //navigationForUpper.navigationText.text = "";
        upperNavigationText.gameObject.SetActive(false);
        //navigationForUpper.ShowMessage("マウスを使って予定航路を描きましょう！\n");
        //navigationForFollowing.enabled = false;

        _showingMessageCoroutine = null;
    }

    IEnumerator Start()
    {
        yield return null;//1フレーム待機（ProbeのTransformが初期化されるのを待つ）
        
        upperVirtualCamera.Priority = 10;
        followingVirtualCamera.Priority = 0;
        fpsCamera.Priority = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && followingVirtualCamera.IsLive)
        {
            //Probeの慣性をリセット
            probe.ResetInertia();
            
            // Cキーが押されたとき、FPSカメラに切り替え
            fpsCamera.Priority = 20;
            
            _lineRenderer.startColor = _transparentStartColor;
            _lineRenderer.endColor = _transparentEndColor;

            // StartCoroutine(ChangeCameraToFps());
            
            normalFollowingCanvas.gameObject.SetActive(false);
            fpsCanvas.gameObject.SetActive(true);

            fuelTextForFPS.text = fuelTextForFollowing.text;
            fuelGaugeForFPS.fillAmount = fuelGaugeForFollowing.fillAmount;

            probe.canMove = false;//操作を無効化
        }

        if (Input.GetKey(KeyCode.C) && fpsCamera.IsLive)
        {
            probe.ResetInertia();
            fuelTextForFPS.text = fuelTextForFollowing.text;
            fuelGaugeForFPS.fillAmount = fuelGaugeForFollowing.fillAmount;
        }

        if (Input.GetKeyUp(KeyCode.C) && fpsCamera.IsLive)
        {
            //Probeの慣性をリセット&前方向を設定
            probe.ResetInertia();
            probe.SetForwardDirection(fpsCamera.transform.forward);
            
            // Cキーが離されたとき、追従カメラに切り替え
            fpsCamera.Priority = 5;

            _lineRenderer.startColor = _originalStartColor;
            _lineRenderer.endColor = _originalEndColor;
            
            // StartCoroutine(ChangeCameraToFollow());
            
            fpsCanvas.gameObject.SetActive(false);
            normalFollowingCanvas.gameObject.SetActive(true);
            
            probe.canMove = true;//操作を有効化
        }

        // if (upperVirtualCamera.IsLive)
        // {
        //     if (DrawLine.isDrawing) return;//描画中は何もしない
        //     
        //     //描画中じゃなければマウス位置にCelestialBodyがあるかどうかをチェック
        //     Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        //     RaycastHit hit;
        //     
        //     if (Physics.Raycast(ray, out hit))
        //     {
        //         // マウスの位置にCelestialBodyがある場合
        //         if (hit.collider.CompareTag("CelestialBody"))
        //         {
        //             // Debug.Log("Hit celestial body: " + hit.collider.gameObject.name);
        //             var cb = hit.collider.gameObject.GetComponent<CelestialBody>();
        //             // Debug.Log("GC of CelestialBody: "+ cb.gravitationCoefficient);
        //             var data = cb.GetCelestialBodyData();
        //             // Debug.Log(data.bodyName);
        //             
        //             infoWindow.SetInformation(data);
        //             guideForInformation.SetActive(true); //infoWindowのガイドを表示
        //             
        //             //QキーでinfoWindowを表示
        //             if (Input.GetKeyDown(KeyCode.Q))
        //             {
        //                 infoWindow.ShowInfoWindow();
        //             }
        //         }
        //         
        //     }
        //     else
        //     {
        //         guideForInformation.SetActive(false);
        //     }
        // }
    }

    // IEnumerator ChangeCameraToFps()
    // {
    //     Time.timeScale = 0;
    //     toFpsDirector.Play();// FPSカメラのアニメーションを再生
    //     fpsCamera.Priority = 20;
    //     Time.timeScale = 1;
    //     
    //
    //     yield return null;
    // }

    // IEnumerator ChangeCameraToFollow()
    // {
    //     Time.timeScale = 0;
    //     toFollowingDirector.Play();// 追従カメラのアニメーションを再生
    //     fpsCamera.Priority = 5;
    //     Time.timeScale = 1;
    //     
    //     _lineRenderer.startColor = _originalStartColor;//色を元に戻す
    //     _lineRenderer.endColor = _originalEndColor;
    //     
    //     yield return null;
    // }
    

    // Update is called once per frame
    void FixedUpdate()
    {
        if (DrawLine.isDrawing) return;//描画中は何もしない
        
        if(!_didDrawOnce && _mover.drawLine.positionCount > 1) _didDrawOnce = true;
        
        if (Vector3.Distance(probe.transform.position, _mover.drawLine.GetPosition(0)) < 1f && _didDrawOnce)
        {
            if(!startButton.interactable)
                startButton.interactable = true;

            upperNavigationText.text = "右のGoボタンで航行開始です。→\n"
                                       + "予定消費燃料は"
                                       + _mover.drawLine.positionCount * probe.fuelConsumptionRatioOfAutoMove
                                       + "です。";
            
            ShowMessage(upperNavigationText,false);
        }
        else
        {
            if(startButton.interactable)
                startButton.interactable = false;
            if (Vector3.Distance(probe.transform.position, _mover.drawLine.GetPosition(0)) >= 1f && _didDrawOnce)
            {
                upperNavigationText.text = "航路の始点があなたの探査機から少し遠いようです。\n"
                                           + "探査機付近から航路を描いてください。";
                ShowMessage(upperNavigationText,false);
            }
        }

        if (_mover.isMoving)
        { //自動航行中
            currentStateText.text = "航行：自動\n";
        }
        else if (probe.isManipulating)
        {
            currentStateText.text = "航行：手動\n";
        }
        else
        {
            currentStateText.text = "航行：停止\n";
        }

        if (_mover.wasFarAway)
        {
            if (!_mover.canAutoMove)
            {
                currentStateText.text += "航路から離脱\n";
            }

            else if (_mover.canAutoMove)
            {
                currentStateText.text += "復帰準備完了\n";
            }

            if (_mover.isRecovering)
            {
                currentStateText.text += "復帰中\n";
            }

            recoveryTimerGauge.gameObject.SetActive(_mover.isRecovering);
            if (recoveryTimerGauge.IsActive())
            {
                recoveryTimerGauge.fillAmount = _mover.nearLineTimer / _mover.reenableAutoMoveTime; // 復帰タイマーゲージの更新
            }
        
        }

        if (probe.fuel > 0 && isPlaying)
        {
            fuelTextForFollowing.text = probe.fuel.ToString();// 燃料を表示
            fuelGaugeForFollowing.fillAmount = (float)probe.fuel / probe.maxFuel; // 燃料ゲージの更新

            if ((float)probe.fuel / probe.maxFuel < 0.5f && !_hasWarnedForHalfFuel)
            {
                //navigationForFollowing.ShowMessage("燃料が半分を切りました！\n"
                //                                +"残量に留意ください！");
                _hasWarnedForHalfFuel = true; // 半分の燃料を警告したのでフラグを立てる
            }
        }

        if (followingVirtualCamera.IsLive || fpsCamera.IsLive)
        {
            UpdateTimer();
        }

        if (probe.fuel <= 0 && isPlaying)//燃料が0以下になったらゲームオーバー
        {
            fuelTextForFollowing.text = "0";// 燃料を0に表示
            fuelGaugeForFollowing.fillAmount = 0; // 燃料ゲージを0に更新
            isPlaying = false;// ゲームプレイ中フラグをfalseにする
            ResultParameters.gameOverType = GameOverType.RunOut;
            GameOver();// ゲームオーバー処理を呼び出す
        }

        if (probe.damagePercentage >= 100f && isPlaying)//損害率が100%以上になったらゲームオーバー
        {
            isPlaying = false;
            ResultParameters.gameOverType = GameOverType.Devastated;
            GameOver();
        }

        if (probe.isClear && isPlaying)
        {
            isPlaying = false;
            ResultParameters.gameOverType = GameOverType.Cleared;
            StartCoroutine(Clear());
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

        _mover.canAutoMove = true;
        _mover.isMoving = true;
        _mover.currentIndex = 0;
        
        //followingCamera.enabled = true;// 追従カメラを有効化
        _mover.isEnableFollowing = true;// 追従カメラを有効にしたのでフラグを立てる
        //ChangeCamera(upperCamera, followingCamera); // 上方カメラから追従カメラに切り替え
        
        //navigationForFollowing.enabled = true;//ナビゲーションを有効化
        //navigationForFollowing.navigationText.text = "";
        //upperCamera.enabled = false;// 上方カメラを無効化
        //navigationForUpper.enabled = false;//上方カメラのナビゲーションを無効化
        normalFollowingCanvas.gameObject.SetActive(true);
        resultCanvas.gameObject.SetActive(false);
        upperCanvas.gameObject.SetActive(false);
        
        probe.canMove = true;// Probeの移動を有効化
        recoveryTimerGauge.gameObject.SetActive(false);// 復帰タイマーゲージは最初は無効化
        
        ChangeVirtualCamera(upperVirtualCamera, followingVirtualCamera); // 上方カメラの仮想カメラから追従カメラの仮想カメラに切り替え

        fuelTextForFollowing.text = probe.maxFuel.ToString(); // 初期燃料を表示
        
    }

    private void GameOver()
    {
        Time.timeScale = 0;
        
        normalFollowingCanvas.gameObject.SetActive(false);
        upperCanvas.gameObject.SetActive(false);
        resultCanvas.gameObject.SetActive(true); // 結果表示用のCanvasを有効化
        
        blackBackground.gameObject.SetActive(false);
        clearText.gameObject.SetActive(false);
        // clearResultText.gameObject.SetActive(false);
        
        // float distanceToTarget = Vector3.Distance(probe.transform.position, probe.collisionTarget.transform.position);
        // gameOverResultText.text += distanceToTarget.ToString("F2") + "万km\n";
        
        //Debug.Log("Game Over");
    }

    public void Retry()
    {
        Time.timeScale = 1;
        
        blackBackground.gameObject.SetActive(true);
        // loadingText.gameObject.SetActive(true);
        // loadingSlider.gameObject.SetActive(true);

        StartCoroutine(WaitAndLoad(1, SceneManager.GetActiveScene().buildIndex)); // 1秒待ってからリトライ処理を開始
    }

    public void BackToTitle()
    {
        Time.timeScale = 1;
        
        blackBackground.gameObject.SetActive(true);
        // loadingText.gameObject.SetActive(true);
        // loadingSlider.gameObject.SetActive(true);
        
        StartCoroutine(WaitAndLoad(1,0)); // 1秒待ってからタイトルシーンをロード
    }

    IEnumerator LoadScene(int sceneBuildIndex)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneBuildIndex);
        
        while (!async.isDone)
        {
            float progress = Mathf.Clamp01(async.progress / 0.9f);
            // loadingSlider.value = progress;
            
            yield return null;
        }
        
    }
    
    IEnumerator WaitAndLoad(int seconds, int sceneBuildIndex)
    {
        yield return new WaitForSeconds(seconds); // 指定された秒数待機
        yield return LoadScene(sceneBuildIndex); // シーンをロード
    }

    private IEnumerator Clear()
    {
        Time.timeScale = 0;
        normalFollowingCanvas.gameObject.SetActive(false);
        upperCanvas.gameObject.SetActive(false);
        resultCanvas.gameObject.SetActive(true); // 結果表示用のCanvasを有効化
        
        blackBackground.gameObject.SetActive(true);
        
        clearText.gameObject.SetActive(true);
        clearText.transform.DOScale(new Vector3(2, 2, 1), 0.5f).SetEase(Ease.OutCubic).SetUpdate(true);
        clearText.rectTransform.DOAnchorPos(new Vector2(0f, 0f), 0.5f).SetEase(Ease.OutCubic).SetUpdate(true);
        yield return new WaitForSecondsRealtime(2f);
        
        //clearText.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(false); // ゲームオーバーテキストを非表示
        // gameOverResultText.gameObject.SetActive(false);
        
        float fuelPercentage = (float)probe.fuel / probe.maxFuel * 100f;
        ResultParameters.fuelPercentage = fuelPercentage;
        // clearResultText.text = "Time:" + _timerMinutes.ToString("00") + ":" + ((int)_timerSeconds).ToString("00") + "\n"
        //                   + "Remaining Fuel:" + fuelPercentage.ToString("00") + "%\n";
        
        int timeInSeconds = _timerMinutes * 60 + (int)_timerSeconds;
        ResultParameters.time = timeInSeconds;
        ResultParameters.timeScoreInterval = timeScoreInterval;
        // int score = probe.fuel / probe.maxFuel * 1000;
        // score += (10000 / timeInSeconds);
        //
        // if (probe.damagePercentage > 0f)
        // {
        //     clearResultText.text+="Damage:" + probe.damagePercentage.ToString("F1") + "%\n";
        //     score -= (int)(probe.damagePercentage * 10); // 損害率に応じてスコアを減点
        //     score = Math.Max(score, 0);//負の数になったらスコアは0にする
        // }

        ResultParameters.damage = probe.damagePercentage;
        ResultParameters.sceneName = SceneManager.GetActiveScene().name;

        blackBackground.DOFade(255f,0.5f).SetEase(Ease.OutCubic).SetUpdate(true);// 背景をフェードイン
        yield return new WaitForSecondsRealtime(0.5f);

        AsyncOperation async = SceneManager.LoadSceneAsync("ResultScene");
        while (!async.isDone)
        {
            yield return null;
        }
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
    
    private void ChangeVirtualCamera(CinemachineVirtualCameraBase oldCamera, CinemachineVirtualCameraBase newCamera)
    {
        if (oldCamera != null)
        {
            oldCamera.Priority = 0; // 古いカメラの優先度を下げる
        }
        
        if (newCamera != null)
        {
            newCamera.Priority = 10; // 新しいカメラの優先度を上げる
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
        if ((int)_timerSeconds != (int)_formerSeconds && normalFollowingCanvas.gameObject.activeSelf)
        {
            timerText.text = _timerMinutes.ToString("00") + ":" + ((int)_timerSeconds).ToString("00");
        }
        _formerSeconds = _timerSeconds; // 前の秒数を更新
    }

    
    private void ShowMessage(TMP_Text messageField, bool willDisappear, float duration)
    {
        //willDisappearがtrueならメッセージを表示してからduration経ってから消すCoroutineを開始
        if (willDisappear)
        {
            messageField.gameObject.SetActive(true);

            if (_showingMessageCoroutine == null)
                _showingMessageCoroutine = StartCoroutine(HideMessage(messageField, duration));
            else return;// すでにメッセージを表示している場合は何もしない

        }
        //falseならメッセージを表示するだけ
        else if(!willDisappear && !messageField.gameObject.activeSelf)
        {
            messageField.gameObject.SetActive(true);
        }
    }
    
    private void ShowMessage(TMP_Text messageField, bool willDisappear)
    {
        if(!willDisappear && !messageField.gameObject.activeSelf)
            messageField.gameObject.SetActive(true);//falseならメッセージを表示するだけ
    }

    private IEnumerator HideMessage(TMP_Text messageField, float duration)
    {
        yield return new WaitForSeconds(duration);
        messageField.gameObject.SetActive(false);
        _showingMessageCoroutine = null;
    }

}
