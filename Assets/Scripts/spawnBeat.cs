using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnBeat : MonoBehaviour
{
    bool game_started = false;

    AudioSource audioSource;
    bool m_Play;
    bool m_ToggleChange;

    float[] timings;
    public int current_timing_idx;
    public float time_elapsed;
    public List<GameObject> beats;
    public float beats_boundary;

    public GameObject beatPrefab;

    float next_spawn_time;

    const float beat_spawn_x1 = (float)-3.9;
    const float beat_spawn_y = 4;
    const float beat_base_speed = 4;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        m_Play = false;
        m_ToggleChange = true;

        timings = new float[0];
        current_timing_idx = 0;
        beats = new List<GameObject>();

        GameObject spot = GameObject.Find("spot1");
        beats_boundary = spot.transform.position.y;

        //next_spawn_time = 0;
    }

    private void spawn(float x, float y = beat_spawn_y, float z = 0)
    {
        GameObject b = Instantiate(beatPrefab) as GameObject;
        beat beat_script = b.GetComponent<beat>();
        beat_script.setSpeed(beat_base_speed);
        Vector3 pos;
        pos.x = (float)x;
        pos.y = y;
        pos.z = z;
        b.transform.position = pos;
        beats.Add(b);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !game_started)
        {
            game_started = true;
            m_ToggleChange = true;
            m_Play = !m_Play;
            timings = new float[500];
            for (int x = 0; x < 500; x++)
            {
                timings[x] = ((x + 6) * (float)(60.0 / 130.0)) + time_elapsed - (Math.Abs(beat_spawn_y - beats_boundary) / beat_base_speed);
            }
            //next_spawn_time = Time.time + (60 / 130);
        }

        //Check to see if you just set the toggle to positive
        if (m_Play == true && m_ToggleChange == true)
        {
            //Play the audio you attach to the AudioSource component
            audioSource.Play();
            //Ensure audio doesn’t play more than once
            m_ToggleChange = false;
        }
        // TODO: add pause
        //Check if you just set the toggle to false
        if (m_Play == false && m_ToggleChange == true)
        {
            //Stop the audio
            audioSource.Stop();
            //Ensure audio doesn’t play more than once
            m_ToggleChange = false;
        }

        while (beats.Count > 0)
        {
            //beat sc = beats[0].GetComponent<beat>();
            //sc.setSpeed(40);
            if (beats[0].transform.position.y < beats_boundary)
            {
                Destroy(beats[0]); // TODO: replace with beats falling animation
                beats.RemoveAt(0);
            } else
            {
                break;
            }
        }

        //print(time_elapsed);
        time_elapsed += Time.deltaTime; // TODO: is this efficient?
        if (current_timing_idx < timings.Length)
        {
            if (time_elapsed > timings[current_timing_idx])
            {
                current_timing_idx++;
                // spawn a new beat!
                spawn((float)beat_spawn_x1);
            }
        }

        // another beat timing system
        //if (Time.time > next_spawn_time && next_spawn_time != 0.0)
        //{
        //    print(Time.time);
        //    print(next_spawn_time);
        //    next_spawn_time = Time.time + (float)(60.0 / 130.0);
        //    spawn((float)-3.9);
        //}
    }
}
