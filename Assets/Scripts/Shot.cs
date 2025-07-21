using UnityEngine;

public class Shot : MonoBehaviour
{
    public int speed;
    public int attack;
    private Transform _probe;
    public int maxDistance;
    public float shotInterval;

    void Start()
    {
        _probe = GameObject.FindGameObjectWithTag("Probe").transform;
    }
    
    void Update()
    {
        if (Vector3.Distance(transform.position, _probe.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }
}
