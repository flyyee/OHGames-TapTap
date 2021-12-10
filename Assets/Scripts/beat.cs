using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class beat : MonoBehaviour
{
    public float speed;
    public bool hasTail = false;
    public Rigidbody2D rb;
    GameObject cam;
    spawnBeat cam_script;
    public GameObject beatTail;
    public int lane;

    // Start is called before the first frame update
    void Start()
    {
      // speed must be assigned in the spawner
      cam = GameObject.Find("Main Camera");
      cam_script = cam.GetComponent<spawnBeat>();
    }

    public void setSpeed(float new_speed)
    {
      speed = new_speed;
    }

    public float getSpeed()
    {
      return speed;
    }

    public void setLane(int curr_lane)
    {
      lane = curr_lane;
    }

    public int getLane()
    {
      return lane;
    }

    public void setTail(GameObject t, float tailLength = 0f)
    {
      hasTail = true;
      beatTail = t;
      tail tail_script = t.GetComponent<tail>();
      tail_script.setLength(tailLength);
      tail_script.setSpeed(speed);
      tail_script.setLane(lane);
      tail_script.alive = true;
      t.transform.position = new Vector3(
        gameObject.transform.position.x,
        gameObject.transform.position.y + tailLength/2.0f,
        1
      );
    }

    public GameObject getTail()
    {
      return beatTail;
    }

    // Update is called once per frame
    void Update()
    {
      if (cam_script.m_Play)
      {
        Vector3 speedvector;
        speedvector.x = 0;
        speedvector.y = -1 * speed * Time.deltaTime;
        speedvector.z = 0;
        gameObject.transform.position += speedvector;
      }
    }
}
