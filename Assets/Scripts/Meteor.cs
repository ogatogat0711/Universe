using System;
using Unity.VisualScripting;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    public int speed;
    public int hp;
    private Transform _probe;
    public int maxDistance;
    public GameObject explosionPrefab;

    void Start()
    {
        UnityEngine.Random.InitState(DateTime.Now.Millisecond);
        _probe = GameObject.FindGameObjectWithTag("Probe").transform;
    }
    
    void Update()
    {
        if (hp <= 0)
        {
            Destroy(gameObject);
        }
        
        if( Vector3.Distance(transform.position, _probe.position) > maxDistance)//遠くに離れたときに削除
        {
            Destroy(gameObject);
        }

    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Shot"))
        {
            Shot shot = other.gameObject.GetComponent<Shot>();
            
            GameObject explosion = Instantiate(explosionPrefab, shot.transform.position, Quaternion.identity); // 爆発エフェクトを生成
            
            Destroy(shot.gameObject);
            Destroy(explosion,2f);
            
            hp -= shot.attack;
        }
    }
}
