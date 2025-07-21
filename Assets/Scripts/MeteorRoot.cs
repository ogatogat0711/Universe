using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using Random = System.Random;

public class MeteorRoot : MonoBehaviour
{
    public Meteor[] meteorPrefabs;
    public CinemachineVirtualCameraBase vCamera;
    public CinemachineVirtualCameraBase fpsCamera;
    public Camera mainCamera;
    public Probe probe;
    public int distanceFromProbe = 20;//Probeからの距離
    public float spawnInterval = 5f; // 隕石生成の間隔
    public float transferInterval = 10f; // 生成位置変更の間隔
    private float _spawnTimer;//生成のタイマー
    private float _positionTransferTimer;//生成位置変更のタイマー 
    public Transform meteorParent;
    public bool isSpawning;

    void Start()
    {
        _spawnTimer = 0f;
        _positionTransferTimer = 0f;
        isSpawning = false;
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);
    }

    void FixedUpdate()
    {
        if (isSpawning)
        {
            _spawnTimer += Time.fixedDeltaTime;
            _positionTransferTimer += Time.fixedDeltaTime;

            if (vCamera.IsLive)
            {
                distanceFromProbe = 15;
            }
            else if (fpsCamera.IsLive)
            {
                distanceFromProbe = 20;
            }
            
            if (_positionTransferTimer >= transferInterval)
            {
                Vector3 cameraDirection = mainCamera.transform.forward;
                Vector3 rootPosition = probe.transform.position - cameraDirection * distanceFromProbe; // カメラの後方に生成位置を設定
                transform.position = rootPosition;
                _positionTransferTimer = 0f; // 生成位置変更のタイマーリセット
            }
            
            if (_spawnTimer >= spawnInterval)
            {
                int spawnCount = UnityEngine.Random.Range(1, 7); // 1〜6個の隕石をランダムに生成
                Vector3 spawnDirection = probe.transform.position - transform.position; // Probeに向かう方向を取得
                spawnDirection.Normalize();
                StartCoroutine(SpawnMeteors(spawnCount, spawnDirection));
                _spawnTimer = 0f;//生成タイマーリセット
            }
        }
    }

    IEnumerator SpawnMeteors(int count, Vector3 direction)
    {
        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, meteorPrefabs.Length);
            Vector3 offset = new Vector3(
                UnityEngine.Random.Range(-1f, 1f), // X軸のオフセット
                0f, // Y軸は0に固定
                UnityEngine.Random.Range(-1f, 1f) // Z軸のオフセット
            );
            Meteor meteor = Instantiate(meteorPrefabs[index], transform.position + offset, Quaternion.identity);
            meteor.transform.SetParent(meteorParent); // MeteorをMeteorRootの子オブジェクトに設定
            Rigidbody rb = meteor.GetComponent<Rigidbody>();
            rb.AddForce(direction * meteor.speed, ForceMode.Impulse);//カメラの向いている方向に発射
            yield return new WaitForSeconds(0.2f); // 次の隕石を生成するまでの待機時間
        }
    }
}
