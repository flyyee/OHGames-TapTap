using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tail : MonoBehaviour
{
    public float speed;
    public int length;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void setSpeed(float new_speed)
    {
        speed = new_speed;
    }

    public float getSpeed()
    {
        return speed;
    }

    public void setLength(int l)
    {
      length = l;
      gameObject.transform.localScale = new Vector3(1, length, 1);
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
