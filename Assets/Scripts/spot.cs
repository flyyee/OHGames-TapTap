using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class spot : MonoBehaviour
{
    public Rigidbody2D rb;
    GameObject cam;
    // bool longBeatPressed = false;
    public int spotNo;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = GameObject.Find("Main Camera");
    }

    // Update is called once per frame
    void Update()
    {
    }

    // void OnMouseDown()
    // {
    //     try
    //     {
    //         spawnBeat cam_script = cam.GetComponent<spawnBeat>();
    //         if (cam_script.beats.Count > 0)
    //         {
    //             GameObject beat = cam_script.beats[0];
    //             //GameObject beat = GameObject.Find("Beat");
    //             float distance = Math.Abs(beat.transform.position.y - rb.position.y);
    //             float accuracyScoreAdd = 10 / distance; // TODO: formula for calculating score
    //             // TODO: to prevent points glitch, prevent spam tapping when the beat is nearing the spot
    //             // to achieve this, once the beat has been tapped and points have been awarded, don't allow the player to gain points from it again
    //             // but if the player loses points from an inaccurate tap, they can still then get points awarded from it later
    //             // TODO: if the player does not manage to tap the beat before it gets destroyed at the spot, subtract points
    //             print("Add score:" + accuracyScoreAdd.ToString("0.00"));
    //         }
    //     }
    //     catch (NullReferenceException err)
    //     {
    //         // beat gameobject has been destroyed, aka out of frame
    //     }
    // }

    public void tapResponse()
    {
        float accuracyScoreAdd = (float)0;
        try
        {
            spawnBeat cam_script = cam.GetComponent<spawnBeat>();
            if (cam_script.beats[spotNo-1].Count > 0)
            {
                GameObject beat = cam_script.beats[spotNo-1][0];
                float distance = Math.Abs(beat.transform.position.y - rb.position.y);
                if (distance < 1) // award points if disk is somewhat touching the spot
                {
                  beat beat_script = beat.GetComponent<beat>();
                  if (beat_script.hasTail)
                  {
                    GameObject tail = beat_script.getTail();
                    cam_script.longBeats[spotNo-1] = tail;
                    tail tail_script = tail.GetComponent<tail>();
                    tail_script.fading = true;
                    tail_script.spoty = rb.position.y;
                  }
                  accuracyScoreAdd = 10*(1-distance); // or 10/distance, or anything else
                  // +1 to streak (maybe in spawnBeat code, see comment below)
                  if (distance < 0.25)
                  {
                    // perfect score – give a high score
                    accuracyScoreAdd = 50;
                    cam_script.perfect_on = true;
                    StartCoroutine(cam_script.FadeTextToFullAlpha(0.5f, cam_script.Perfect));
                  }

                  // add to streak
                  cam_script.currStreak++;

                  // remove the beat
                  Destroy(beat);
                  cam_script.beats[spotNo-1].RemoveAt(0);
                  // print("removal fine, beats left in lane " + spotNo.ToString() + ": " + cam_script.beats[spotNo-1].Count.ToString());

                } else // disk is not touching the spot
                {
                  // deduct points
                  accuracyScoreAdd = -5;
                  // deduct from streak (if >0)
                  if (cam_script.currStreak > 0)
                  {
                    cam_script.currStreak--;
                  }
                }
                // float accuracyScoreAdd = 10 / distance;

                // TODO: formula for calculating score
                // TODO: to prevent points glitch, prevent spam tapping when the beat is nearing the spot
                // to achieve this, once the beat has been tapped and points have been awarded, don't allow the player to gain points from it again
                // but if the player loses points from an inaccurate tap, they can still then get points awarded from it later
                // TODO: if the player does not manage to tap the beat before it gets destroyed at the spot, subtract points
                // print("Add score:" + accuracyScoreAdd.ToString("0.00"));
            }
            cam_script.currScore += accuracyScoreAdd;
        }
        catch (NullReferenceException err)
        {
            // beat gameobject has been destroyed, aka out of frame
        }
        // return accuracyScoreAdd;
    }
}
