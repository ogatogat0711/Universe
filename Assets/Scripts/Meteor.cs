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
        
        else if (other.gameObject.CompareTag("Probe"))
        {
            Probe probe = other.gameObject.GetComponent<Probe>();
            Rigidbody rb = probe.GetComponent<Rigidbody>();
            
            rb.AddForce(transform.forward * speed, ForceMode.Impulse);
            
            Rigidbody rbOfMeteor = GetComponent<Rigidbody>();
            Vector3 relativeVelocity = rbOfMeteor.linearVelocity - rb.linearVelocity; // 相対速度を計算

            probe.damagePercentage += relativeVelocity.magnitude * rbOfMeteor.mass;//損害率を加算
            Debug.Log("damage: " + probe.damagePercentage);
            
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity); // 爆発エフェクトを生成
            //Destroy(this.gameObject);
            Destroy(explosion, 2f); // 2秒後に爆発エフェクトを削除
        }
    }
}
