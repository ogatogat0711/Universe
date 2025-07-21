using UnityEngine;

public class Shot : MonoBehaviour
{
    public int speed;
    public int attack;
    private Probe _probe;
    public int maxDistance;
    
    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, _probe.transform.position) > maxDistance)
        {
            Destroy(this);
        }
    }
}
