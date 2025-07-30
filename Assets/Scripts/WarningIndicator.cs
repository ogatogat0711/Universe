using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarningIndicator : MonoBehaviour
{
    public GameObject probe;
    public Camera mainCamera;
    public RectTransform canvasRect;
    public GameObject indicatorPrefab;
    public float edgeBuffer = 50f;
    public float minBlinkInterval = 0.1f;//点滅秒数(最小)
    public float maxBlinkInterval = 0.5f;//点滅秒数(最大)

    class Entry //近づいてくるMeteorと、それに対応するindicatorを保持する内部クラス
    {
        public GameObject meteor;
        public RectTransform indicator;
        public Coroutine blinkCoroutine;
    }
    
    private List<Entry> entries = new List<Entry>();//Entryのリスト

    public void AddMeteor(GameObject meteor)//リストに追加する
    {
        var indicatorInstance = Instantiate(indicatorPrefab, canvasRect);
        var rect = indicatorInstance.GetComponent<RectTransform>();

        var entry = new Entry
        {
            meteor = meteor,
            indicator = rect
        };

        entry.blinkCoroutine = StartCoroutine(Blink(entry));
        entries.Add(entry);
    }

    IEnumerator Blink(Entry e)
    {
        Image image = e.indicator.GetComponent<Image>();

        while (true)
        {
            if (e.meteor == null) yield break;

            float distance = Vector3.Distance(e.meteor.transform.position, probe.transform.position);
            float t = Mathf.InverseLerp(50f, 1f, distance);// 距離に応じて点滅の速さを調整(近いと1になる)
            float blinkInterval = Mathf.Lerp(maxBlinkInterval, minBlinkInterval, t);// 点滅の速さを計算

            image.enabled = !image.enabled; // 点滅
            yield return new WaitForSeconds(blinkInterval);
        }
        
    }

    void Update()
    {
        if (!GameManager.isPlaying)//プレイ終了したら全削除
        {
            entries.Clear();
        }
        
        for( int i = entries.Count - 1; i >= 0; i--)//途中で削除する可能性があるため逆順のfor利用
        {
            var e = entries[i];
            if (e.meteor == null || !IsApproaching(e.meteor))
            {
                //meteorがない、または遠ざかっているならばindicatorを削除
                if (e.blinkCoroutine != null) StopCoroutine(e.blinkCoroutine);
                Destroy(e.indicator.gameObject);
                entries.RemoveAt(i);
                continue;
            }
            UpdateIndicatorPosition(e);
        }
    }

    void UpdateIndicatorPosition(Entry e)
    {
        Vector3 worldPosition = e.meteor.transform.position;
        Vector3 viewPosition = mainCamera.WorldToViewportPoint(worldPosition);
        
        float halfW = canvasRect.sizeDelta.x * 0.5f - edgeBuffer;
        float halfH = canvasRect.sizeDelta.y * 0.5f - edgeBuffer;

        Vector2 anchored = new Vector2();

        if (viewPosition.z > 0)
        {
            //前方ならそのまま表示座標を計算
            anchored.x = (viewPosition.x - 0.5f) * canvasRect.sizeDelta.x;
            anchored.y = (viewPosition.y - 0.5f) * canvasRect.sizeDelta.y;
            anchored.x = Mathf.Clamp(anchored.x, -halfW, halfW);
            anchored.y = Mathf.Clamp(anchored.y, -halfH, halfH);
        }
        else
        {
            //後方なら左右反転、Y座標は下に固定
            float flippedX = 1f - viewPosition.x;
            anchored.x = (flippedX - 0.5f) * canvasRect.sizeDelta.x;
            anchored.x = Mathf.Clamp(anchored.x, -halfW, halfW);
            anchored.y = -halfH;
        }
        
        e.indicator.anchoredPosition = anchored;
    }

    private bool IsApproaching(GameObject meteor)
    {
        Rigidbody meteorRigidbody = meteor.GetComponent<Rigidbody>();
        Rigidbody probeRigidbody = probe.GetComponent<Rigidbody>();
        
        Vector3 relativePosition = meteor.transform.position - probe.transform.position; // 相対位置を計算
        Vector3 relativeVelocity= meteorRigidbody.linearVelocity - probeRigidbody.linearVelocity; // 相対速度を計算
        
        float dot = Vector3.Dot(relativePosition.normalized, relativeVelocity.normalized); // 相対位置と相対速度のドット積を計算

        return dot < 0; // 相対位置と相対速度のドット積が負ならば、MeteorはProbeに近づいている
    }
}
