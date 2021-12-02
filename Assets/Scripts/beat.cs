using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class beat : MonoBehaviour
{
    public float speed;
    public Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        // speed must be assigned in the spawner
    }

    public void setSpeed(float new_speed)
    {
        speed = new_speed;
    }

    public float getSpeed()
    {
        return speed;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 speedvector;
        speedvector.x = 0;
        speedvector.y = -1 * speed * Time.deltaTime;
        speedvector.z = 0;
        gameObject.transform.position += speedvector;
    }
}
