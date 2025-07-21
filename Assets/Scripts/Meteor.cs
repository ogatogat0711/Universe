using System;
using Unity.VisualScripting;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    public int speed;
    public int hp;
    private Transform _probe;
    public int maxDistance;

    void Start()
    {
        _probe = GameObject.FindGameObjectWithTag("Probe").transform;
    }
    
    void Update()
    {
        if (hp <= 0)
        {
            Destroy(this);
        }
        
        if( Vector3.Distance(transform.position, _probe.position) > maxDistance)//遠くに離れたときに削除
        {
            Destroy(this);
        }

    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Shot"))
        {
            Shot shot = other.gameObject.GetComponent<Shot>();
            hp -= shot.attack;
            
            Destroy(shot.gameObject);
        }
    }
}
