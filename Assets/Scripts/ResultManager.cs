using System;
using System.Collections;
using Coffee.UIExtensions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public static class ResultParameters
{
    public static GameManager.GameOverType gameOverType= GameManager.GameOverType.RunOut;
    public static int time;
    public static int timeScoreInterval;
    public static float fuelPercentage;
    public static float damage;
    public static string sceneName;
}
public class ResultManager : MonoBehaviour
{
    public TMP_Text timeText;
    public TMP_Text timeScoreText;
    public TMP_Text fuelPercentageText;
    public TMP_Text fuelPercentageScoreText;
    public TMP_Text damageText;
    public TMP_Text damageScoreText;
    public TMP_Text totalText;
    public TMP_Text totalScoreText;
    public Image underlineImage;
    public TMP_Text rateText;
    public Image blackBackground;
    public Button nextButton;
    public Image resultPanel;
    public Image transitionPanel;
    public TMP_Text gameOverText;

    void Awake()
    {
        Time.timeScale = 1;
        
        resultPanel.gameObject.SetActive(true);
        transitionPanel.gameObject.SetActive(true);
        
        blackBackground.gameObject.SetActive(true);
        
        timeText.gameObject.SetActive(false);
        timeScoreText.gameObject.SetActive(false);
        
        fuelPercentageText.gameObject.SetActive(false);
        fuelPercentageScoreText.gameObject.SetActive(false);
        
        damageText.gameObject.SetActive(false);
        damageScoreText.gameObject.SetActive(false);
        
        totalText.gameObject.SetActive(false);
        totalScoreText.gameObject.SetActive(false);
        
        underlineImage.gameObject.SetActive(false);
        rateText.gameObject.SetActive(false);
        
        nextButton.gameObject.SetActive(false);
        
        gameOverText.gameObject.SetActive(false);
    }

    IEnumerator Start()
    {
        if (ResultParameters.gameOverType == GameManager.GameOverType.Cleared)//クリア時の処理
        {
            yield return new WaitForSeconds(0.5f);//少し待機
            blackBackground.rectTransform.DOAnchorPos(new Vector2(-1920f, 0f), 0.8f).SetEase(Ease.OutCubic);//黒背景を左にスライドアウト
            yield return new WaitForSeconds(1.0f);
            
            int time = ResultParameters.time;
            timeText.text += $"{time / 60:D2}:{time % 60:D2}";//分:秒の形式で表示
            timeText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            
            int timeScoreInterval = ResultParameters.timeScoreInterval;
            int timeScore = 5000 - time / timeScoreInterval * 500;//最高点5000点、間隔ごとに500減点
            timeScore = Math.Max(0, timeScore);//0点を下回ったら0点にする
            timeScoreText.text = timeScore.ToString();
            timeScoreText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.8f);
            
            float fuelPercentage = ResultParameters.fuelPercentage;
            fuelPercentage = Mathf.Floor(fuelPercentage);//小数点以下切り捨て
            fuelPercentageText.text += $"{fuelPercentage}%";
            fuelPercentageText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            
            int fuelScore = (int)(2000f * fuelPercentage / 100f);//最高点2000点、得点はそれの百分率
            fuelPercentageScoreText.text = fuelScore.ToString();
            fuelPercentageScoreText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.8f);
            
            float damage = ResultParameters.damage;
            damage = Mathf.Floor(damage);//小数点以下切り捨て
            damageText.text += $"{damage}%";
            damageText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            
            int damageScore = (int)(-4000f * damage / 100f);//最低点-4000点、得点はそれの百分率
            if (damageScore < 0)
            {
                damageScoreText.text = "- "+ Mathf.Abs(damageScore).ToString();
            }
            else
            {
                damageScoreText.text = damageScore.ToString();
            }
            // damageScoreText.text = damageScore.ToString();
            damageScoreText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.8f);

            int resultScore = timeScore + fuelScore + damageScore;
            totalText.gameObject.SetActive(true);
            totalScoreText.gameObject.SetActive(true);
            CountUp(resultScore, totalScoreText, 3f);
            
            yield return new WaitForSeconds(4f);
            
            float rateRatio = (float)resultScore / 7000f; // 最高点は7000点
            if(rateRatio <= 1.0f && rateRatio > 0.8f)
            {
                rateText.text = "S";
                rateText.color = Color.yellowNice;
            }
            else if (rateRatio <= 0.8f && rateRatio > 0.6f)
            {
                rateText.text = "A";
                rateText.color = Color.softRed;
            }
            else if (rateRatio <= 0.6f && rateRatio > 0.4f)
            {
                rateText.text = "B";
                rateText.color = Color.cornflowerBlue;
            }
            else if (rateRatio <= 0.4f && rateRatio > 0.2f)
            {
                rateText.text = "C";
                rateText.color = Color.greenYellow;
            }
            else
            {
                rateText.text = "D";
                rateText.color = Color.white;
            }
            
            rateText.gameObject.SetActive(true);
            underlineImage.gameObject.SetActive(true);
            
            nextButton.gameObject.SetActive(true);
        }

        else
        {
            yield return new WaitForSeconds(0.5f);//少し待機
            blackBackground.rectTransform.DOAnchorPos(new Vector2(-1920f, 0f), 0.8f).SetEase(Ease.OutCubic);//黒背景を左にスライドアウト
            yield return StartCoroutine(PanelTransition());//一枚目のパネルはスライドアウト、二枚目のパネルはスライドイン
            
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = ResultParameters.gameOverType switch
            {
                GameManager.GameOverType.RunOut => "Your Probe has Run Out of Fuel...",
                GameManager.GameOverType.Devastated => "Your Probe has been Devastated...",
                _ => "Unknown Error"
            };
        }
        
    }

    private void Update()
    {
        if (rateText.text == "B" || rateText.text == "C" || rateText.text == "D") return;//Bランク以下は何もしない
        if (!rateText.gameObject.activeSelf) return; //レート非表示の時も何もしない
        
    }

    private void CountUp(int targetScore, TMP_Text targetScoreText, float duration)
    {
        int displayScore = 0;
        targetScoreText.text = displayScore.ToString();
        if (targetScore < 0)
        {
            targetScoreText.color = Color.softRed;
        }

        DOTween.To(() => displayScore,
            x => displayScore = x,
            targetScore,
            duration)
            .SetEase(Ease.OutCubic)
            .OnUpdate(() =>
            {
                //値が更新するたびに表示を更新
                targetScoreText.text = displayScore.ToString();
            });
    }

    public void OnNextButtonClicked()
    {
        nextButton.interactable = false;//ボタンを無効化して連打を防ぐ
        StartCoroutine(PanelTransition());
    }

    private IEnumerator PanelTransition()
    {
        resultPanel.rectTransform.DOAnchorPos(new Vector2(-1920f, 0f), 0.8f).SetEase(Ease.OutCubic);//左にスライドアウト
        yield return new WaitForSeconds(0.5f);//食い気味に移動
        transitionPanel.rectTransform.DOAnchorPos(new Vector2(0f, 0f),0.8f).SetEase(Ease.OutCubic);//右からスライドイン
    }
    
    public void Retry()
    {
        
    }
}
