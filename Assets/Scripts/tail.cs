using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tail : MonoBehaviour
{
	public float speed;
	public float length;
	public bool fading = false;
	public bool alive;
	public int lane;
	public float spoty;
	GameObject cam;
	spawnBeat cam_script;
	const float width = 0.25f;
	bool firstFade = true;

	// Start is called before the first frame update
	void Start()
	{
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

	public void setLength(float l)
	{
		length = l;
		gameObject.transform.localScale = new Vector3(width, length, 1);
	}

	public void setLane(int curr_lane)
	{
		lane = curr_lane;
	}

	// Update is called once per frame
	void Update()
	{
		Vector3 speedvector;
		speedvector.x = 0;
		speedvector.y = -1 * (fading ? speed / 2 : speed) * Time.deltaTime;
		speedvector.z = 0;
		gameObject.transform.position += speedvector;
		if (fading)
		{
			if (firstFade)
			{
				firstFade = false;
				Vector3 offsetvector;
				// TODO: bug where if you press (a bit) early for beats with tails, the tail becomes longer
			}

			float rem_distance = gameObject.transform.position.y - spoty;
			alive = rem_distance > 0;
			if (alive)
			{
        float curr_length = length;
        length = Mathf.Min(gameObject.transform.localScale.y / 2f + rem_distance, curr_length);
				gameObject.transform.localScale = new Vector3(width, length, 1);
			}
		}
	}
}
