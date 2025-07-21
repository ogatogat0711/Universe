using UnityEngine;

public class Shot : MonoBehaviour
{
    public int speed;
    public int attack;
    private Probe _probe;
    public int maxDistance;
    public float shotInterval;

    void Start()
    {
        _probe = GameObject.FindGameObjectWithTag("Probe").GetComponent<Probe>();
    }
    
    void Update()
    {
        if (Vector3.Distance(transform.position, _probe.transform.position) > maxDistance)
        {
            Destroy(this);
        }
    }
}
