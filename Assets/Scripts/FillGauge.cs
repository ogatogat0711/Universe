using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FillGauge : MonoBehaviour
{
   public Image defaultValueImage;
   public Image positiveValueImage;
   public Image negativeValueImage;
   public RectTransform barTransform;
   private float _originalWidth;
   private Coroutine _flashingCoroutine;

   private void Start()
   {
      _originalWidth = barTransform.rect.width;
      _flashingCoroutine = null;
   }

   public void SetFill(float defaultValue, float variableValue)
   {
      if (_flashingCoroutine != null)
      {
         StopCoroutine(_flashingCoroutine);
         _flashingCoroutine = null;
      }
      
      defaultValueImage.gameObject.SetActive(false);
      positiveValueImage.gameObject.SetActive(false);
      negativeValueImage.gameObject.SetActive(false);

      float blueValue = _originalWidth * defaultValue;
      float variable = _originalWidth * Mathf.Abs(variableValue);

      defaultValueImage.rectTransform.sizeDelta =
         new Vector2(blueValue + 3f, defaultValueImage.rectTransform.sizeDelta.y);//空白があくのを防ぐために3pxだけ大きくする
      defaultValueImage.gameObject.SetActive(true);

      if (variableValue > 0f)
      {
         ResetImageAlpha(positiveValueImage);
         
         positiveValueImage.rectTransform.sizeDelta =
            new Vector2(variable, positiveValueImage.rectTransform.sizeDelta.y);
         positiveValueImage.rectTransform.anchoredPosition = new Vector2(blueValue, 0f);
         positiveValueImage.gameObject.SetActive(true);
         
         _flashingCoroutine = StartCoroutine(Flash(positiveValueImage));
      }
      
      else if (variableValue < 0f)
      {
         ResetImageAlpha(negativeValueImage);
         
         negativeValueImage.rectTransform.sizeDelta =
            new Vector2(variable, negativeValueImage.rectTransform.sizeDelta.y);
         negativeValueImage.rectTransform.anchoredPosition = new Vector2(blueValue, 0f);
         negativeValueImage.gameObject.SetActive(true);

         _flashingCoroutine = StartCoroutine(Flash(negativeValueImage));
      }
      
   }

   private void ResetImageAlpha(Image targetImage)
   {
      Color color = targetImage.color;
      color.a = 1f;
      targetImage.color = color;
      targetImage.DOKill();
   }

   private IEnumerator Flash(Image flashImage)
   {
      while (true)
      {
         flashImage.DOFade(0f, 0.5f).SetEase(Ease.Linear);
         yield return new WaitForSeconds(0.5f);
         flashImage.DOFade(1f, 0.5f).SetEase(Ease.Linear);
         yield return new WaitForSeconds(0.5f);
      }
      
   }
}
