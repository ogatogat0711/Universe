using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Navigation : MonoBehaviour
{
    public TextMeshProUGUI navigationText; //ナビゲーション用のテキスト
    private float _characterInterval = 0.05f;
    private string currentText = ""; // 現在のテキスト
    
    private Coroutine _coroutine;
    
    public void ShowMessage(string text)
    {
        if (currentText == text) return;// 既に表示されているテキストと同じ場合は何もしない
        
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }
        _coroutine = StartCoroutine(DisplayNavigationText(text));
    }
    
    private IEnumerator DisplayNavigationText(string text)
    {
        currentText = text;
        navigationText.text = ""; // テキストをクリア
        
        foreach (char c in text)
        {
            navigationText.text += c; // 1文字ずつ追加
            yield return new WaitForSeconds(_characterInterval);
        }
    }
}
