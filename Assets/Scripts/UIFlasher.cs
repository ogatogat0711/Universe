using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIFlasher : MonoBehaviour
{
    public Image flashImage;
    
    [ColorUsage(true,true)]
    public Color flashColor;
    private float _flashDuration = 0.1f; // 点滅間隔
    private int _flashTimes = 3; // 点滅回数
    private Color _originalColor;//元の色
    private Coroutine _flashCoroutine;
    
    void Start()
    {
        _originalColor = flashImage.color; // 元の色を保存
    }

    public void Flash()
    {
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }

        _flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        for(int i = 0; i < _flashTimes; i++)
        {
            flashImage.color = flashColor; // 点滅色に変更
            yield return new WaitForSeconds(_flashDuration);
            flashImage.color = _originalColor; // 元の色に戻す
            yield return new WaitForSeconds(_flashDuration);
        }

        _flashCoroutine = null;
    }

}
