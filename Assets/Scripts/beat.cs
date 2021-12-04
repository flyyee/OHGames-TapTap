using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class beat : MonoBehaviour
{
    public float speed;
    public bool hasTail = false;
    public Rigidbody2D rb;
    public GameObject beatTail;

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

    public void setTail(GameObject t, int tailLength = 0)
    {
      hasTail = true;
      beatTail = t;
      tail tail_script = t.GetComponent<tail>();
      tail_script.setLength(tailLength);
      tail_script.setSpeed(speed);
      t.transform.position = new Vector3(
        gameObject.transform.position.x,
        gameObject.transform.position.y + tailLength/2.0f,
        1
      );
    }

    // public void setBarBeat(bool barOrBeat, int length = 0)
    // {
    //   barBeat = barOrBeat;
    //   if (barOrBeat)
    //   {
    //     GameObject barObj = Instantiate(barPrefab) as GameObject;
    //     barObj.transform.position = new Vector3(
    //       gameObject.transform.position.x,
    //       gameObject.transform.position.y + length/2.0f,
    //       gameObject.transform.position.z
    //     );
    //     beatbar bar_script = barObj.GetComponent<beatbar>();
    //     bar_script.setLength(length);
    //     bar_script.setSpeed(speed);
    //   }
    // }

    public GameObject getTail()
    {
      return beatTail;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 speedvector;
        speedvector.x = 0;
        speedvector.y = -1 * speed * Time.deltaTime;
        speedvector.z = 0;
        gameObject.transform.position += speedvector;

        // GameObject bar = Instantiate(barPreFab) as GameObject;
        // bar bar_script = bar.GetComponent<bar>();
        // bar.transform.position = new Vector3(
        //   gameObject.transform.position.x,
        //   gameObject.transform.position.y + bar_script.getLength()/2.0f,
        //   gameObject.transform.position.z
        // );
    }

    // void onDestroy()
    // {
    //   Destroy(bar);
    // }
}
