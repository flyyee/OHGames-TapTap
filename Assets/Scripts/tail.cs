using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tail : MonoBehaviour
{
    public float speed;
    public float length;
    public bool fading = false;
    public float spoty;
    GameObject cam;

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

    public void setLength(float l)
    {
      length = l;
      gameObject.transform.localScale = new Vector3(1, length, 1);
    }

    // Update is called once per frame
    void Update()
    {
      Vector3 speedvector;
      speedvector.x = 0;
      speedvector.y = -1 * (fading? speed/2 : speed) * Time.deltaTime;
      speedvector.z = 0;
      gameObject.transform.position += speedvector;
      if (fading)
      {
        float rem_distance = gameObject.transform.position.y-spoty;
        if (rem_distance > 0)
        {
          length = gameObject.transform.localScale.y/2f + rem_distance;
          gameObject.transform.localScale = new Vector3(1, length, 1);
        }
      }
    }
}
