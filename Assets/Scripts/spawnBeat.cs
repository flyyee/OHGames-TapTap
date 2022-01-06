using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.IO;

struct beatTiming
{
	public int lane;
	public int length;
}

struct infinity
{
	public Queue<speedup> speedups;
	public Queue<penalty> penalties;
	public infinity(bool param = true)
	{
		speedups = new Queue<speedup>();
		penalties = new Queue<penalty>();
	}
}

struct speedup
{
	public float elapsedtime;
	public float finaltimescale;
	public float transitionduration;
	public speedup(float e, float f, float t)
	{
		elapsedtime = e;
		finaltimescale = f;
		transitionduration = t;
	}
}

struct penalty
{
	public int deduction;
	public float elapsedtime;
	public penalty(float e, int d)
	{
		deduction = d;
		elapsedtime = e;
	}
}

public class spawnBeat : MonoBehaviour
{
	public TextAsset beatTimingFileE;
	public TextAsset beatTimingFileD;

	bool game_started = false;
	bool game_ready = false; // prevents game from going straight to end screen after countdown

	AudioSource audioSource;
	public Button Pause;
	public Button Resume;
	public Button Exit;
	public bool m_Play;
	private float timescale;
	AudioMixerGroup pitchBendGroup;
	float bpm;
	// bool m_ToggleChange;

	float[] timings;
	public int current_timing_idx;
	public float time_elapsed;
	public float song_time_elapsed;
	public List<List<GameObject>> beats;
	public float beats_boundary;

	public GameObject beatPrefab;
	public GameObject tailPrefab;
	public GameObject darkScreen;
	public GameObject[] pauseObjects;

	float next_spawn_time;

	// const float beat_spawn_x1 = (float)-3.9;
	// const float beat_spawn_x2 = (float)-1.3;
	// const float beat_spawn_x3 = (float)+1.3;
	// const float beat_spawn_x4 = (float)+3.9;
	static readonly float[] beat_spawn_xlist = { -3.9f, -1.3f, 1.3f, 3.9f };
	const float beat_spawn_y = 4;
	const float beat_base_speed = 4;

	public Color col_white = new Color(1, 1, 1);
	public Color col_dblue = new Color(49, 77, 121);
	static readonly string[] input_asdf = { "a", "s", "d", "f" };
	static readonly string[] input_hjkl = { "h", "j", "k", "l" };
	const float long_beat_prob = 0.1f;

	public float curr_score = 0;
	public int curr_streak = 0;
	const int x2_streak = 20;
	const int x3_streak = 50;
	public int curr_combo = 1;
	const float bar_height = 6.8f;
	public Text DispScore;
	public Text DispStreak;
	public Text Perfect;
	public Text CountdownText;
	public bool counting_down = false;
	public bool perfect_on = false;
	public float perfect_timer = 0;
	const float perfect_maxtime = 3;

	List<beatTiming> beatTimingsE = new List<beatTiming>();
	List<beatTiming> beatTimingsD = new List<beatTiming>();
	List<beatTiming> beatTimings = new List<beatTiming>();
	infinity infinityTimings;

	// some infinity parameters
	bool infinityFlag;
	int infinityPenalty;
	float infinity_time_elapsed;

	private void setupBeatTiming(TextAsset file, List<beatTiming> lbeatTimings)
	{
		string text = file.text;
		string[] lines = text.Split(
			new[] { Environment.NewLine },
			StringSplitOptions.None
		);
		for (int x = 0; x < lines.Length; x++)
		{
			beatTiming bt = new beatTiming();
			string line = lines[x];
			switch (line[0])
			{
				case 'a':
					bt.lane = 1;
					break;
				case 'b':
					bt.lane = 2;
					break;
				case 'c':
					bt.lane = 3;
					break;
				case 'd':
					bt.lane = 4;
					break;
			}
			bt.length = 0;
			if (line.Length > 1)
			{
				if (line[1] == 'l')
				{
					bt.length = line[2] - '0';
				}
			}
			lbeatTimings.Add(bt);
		}

		infinityTimings = new infinity(true);
		// hardcode cuz lazy
		//print(infinityTimings.penalties);
		//print(new penalty(20f, 10));
		infinityTimings.penalties.Enqueue(new penalty(20f, 10));
		infinityTimings.speedups.Enqueue(new speedup(20f, 1.1f, 2f)); // elapsed, new scale, transition
		infinityTimings.penalties.Enqueue(new penalty(40f, 15));
		infinityTimings.speedups.Enqueue(new speedup(40f, 1.25f, 1f)); // elapsed, new scale, transition
		infinityTimings.penalties.Enqueue(new penalty(70f, 20));
		infinityTimings.speedups.Enqueue(new speedup(70f, 1.35f, 1f)); // elapsed, new scale, transition
		infinityTimings.penalties.Enqueue(new penalty(120f, 30));
		infinityTimings.speedups.Enqueue(new speedup(120f, 1.5f, 1f)); // elapsed, new scale, transition
	}

	// Start is called before the first frame update
	void Start()
	{
		setupBeatTiming(beatTimingFileE, beatTimingsE);
		setupBeatTiming(beatTimingFileD, beatTimingsD);
		audioSource = GetComponent<AudioSource>();
		Button pauseBtn = Pause.GetComponent<Button>();
		pauseBtn.onClick.AddListener(pauseGame);
		Button resumeBtn = Resume.GetComponent<Button>();
		resumeBtn.onClick.AddListener(resumeGame);
		Button exitBtn = Exit.GetComponent<Button>();
		exitBtn.onClick.AddListener(exitGame);
		m_Play = false;

		if (PlayerPrefs.GetInt("songchoice") == 1 || PlayerPrefs.GetInt("songchoice") == 3)
		{
			bpm = 130.0f; // little einsteins
			audioSource.clip = Resources.Load<AudioClip>("Audio/ein_short");
			beatTimings = beatTimingsE;
		} else
		{
			bpm = 114.0f; // dynamite
			audioSource.clip = Resources.Load<AudioClip>("Audio/dyn_short");
			beatTimings = beatTimingsD;
		}

		if (PlayerPrefs.GetInt("songchoice") == 3 || PlayerPrefs.GetInt("songchoice") == 4)
		{
			infinityFlag = true;
			infinityPenalty = 8;
			//audioSource.loop = true;
			curr_streak = 25; // starts off mid-way
			infinity_time_elapsed = 0f;
		}

		timescale = 1.0f;
		pitchBendGroup = Resources.Load<AudioMixerGroup>("PitchBendMixer");
		audioSource.outputAudioMixerGroup = pitchBendGroup;

		// m_ToggleChange = true;
		pauseObjects = GameObject.FindGameObjectsWithTag("OnPause");
		foreach (GameObject g in pauseObjects) g.SetActive(false);

		Time.timeScale = 1;
		timings = new float[0];
		current_timing_idx = 0;
		beats = new List<List<GameObject>>();
		for (int i = 0; i < 4; i++)
		{
			beats.Add(new List<GameObject>());
		}
		time_elapsed = 0f;
		song_time_elapsed = 0f;

		GameObject spot = GameObject.Find("spot1");
		beats_boundary = spot.transform.position.y;

		DispScore.text = "Game Score: 0";
		Perfect.color = new Color(Perfect.color.r, Perfect.color.g, Perfect.color.b, 0);
		CountdownText.text = "";
		//Perfect.enabled = false;
		//next_spawn_time = 0;

		// Set positions of texts relative to screen height & width
		// setTextPos(DispScore, -0.8f, 0.8f);
	}

	private void spawn(int lane, bool hasTail = false, float tailLength = 0f)
	{
		GameObject b = Instantiate(beatPrefab) as GameObject;
		beat beat_script = b.GetComponent<beat>();
		beat_script.setSpeed(beat_base_speed);
		beat_script.setLane(lane);
		Vector3 pos;
		pos.x = (float)beat_spawn_xlist[lane];
		pos.y = beat_spawn_y;
		pos.z = 0;
		b.transform.position = pos;

		if (hasTail)
		{
			GameObject t = Instantiate(tailPrefab) as GameObject;
			beat_script.setTail(t, tailLength);
		}

		beats[lane].Add(b);
	}

	private void dispCombo(int streak)
	{
		float streak_height = bar_height / 50f * streak;
		GameObject comboBar = GameObject.Find("ComboBarFiller");

		Vector3 new_pos = comboBar.transform.position;
		new_pos.y = streak_height / 2 - 3.4f;
		comboBar.transform.position = new_pos;

		Vector3 new_scale = comboBar.transform.localScale;
		new_scale.y = streak_height;
		comboBar.transform.localScale = new_scale;
	}

	// private void setTextPos(Text txt, float x_rel, float y_rel)
	// {
	//   Vector3 new_pos = txt.transform.position;
	//   new_pos.x = Screen.width/2f * x_rel;
	//   new_pos.y = Screen.height/2f * y_rel;
	//   txt.transform.position = new_pos;
	// }

	private void pauseGame()
	{
		if (game_started)
		{
			m_Play = false;
			Time.timeScale = 0;
			audioSource.Pause();
			foreach (GameObject g in pauseObjects) g.SetActive(true);
			// darkScreen.transform.localScale = new Vector3(100,100,1);
		}
	}

	private void resumeGame()
	{
		m_Play = true;
		Time.timeScale = 1;
		audioSource.Play();
		foreach (GameObject g in pauseObjects) g.SetActive(false);
		// darkScreen.transform.localScale = new Vector3(0,0,1);
	}

	private void exitGame()
	{
		SceneManager.LoadScene("LandingScreen");
	}

	private void changeTimescale(float scale) // 1.5f means 50% faster
	{
		audioSource.pitch = scale;
		pitchBendGroup.audioMixer.SetFloat("pitchBend", 1f / scale);
		timescale = scale;
	}

	// Update is called once per frame
	void Update()
	{
		if (!counting_down) StartCoroutine(StartingCountdown());

		// Logic of the variables here
		// intitially counting_down, game_ready and game_started are all false
		// counting_down gets set to true when Coroutine starts, then the game waits for the countdown to finish
		// game_ready gets set to true after countdown ends, which triggers the game to start
		// game_started gets set to true after all the stuff in the if statement below has run
		// so that they won't run on every update loop
		// additionally this ensures that the game doesn't end prematurely

		if (game_ready && !game_started)
		{
			game_started = true;
			// m_ToggleChange = true;
			m_Play = true;
			audioSource.Play();
			timings = new float[500];
			float offset = 0f;
			if (PlayerPrefs.GetInt("songchoice") == 1 || PlayerPrefs.GetInt("songchoice") == 3) {
				offset = 0.16f; // fix for einsteins theme
			}
			for (int x = 0; x < 500; x++)
			{
				timings[x] = offset + ((x + 6) * (float)(60.0 / bpm)) + time_elapsed - (Math.Abs(beat_spawn_y - beats_boundary) / beat_base_speed);
			}
			//next_spawn_time = Time.time + (60 / 130);
		}

		// //Check to see if you just set the toggle to positive
		// if (m_Play == true && m_ToggleChange == true)
		// {
		//     //Play the audio you attach to the AudioSource component
		//     audioSource.Play();
		//     //Ensure audio doesn�t play more than once
		//     m_ToggleChange = false;
		// }
		// // TODO: add pause
		//
		// //Check if you just set the toggle to false
		// if (m_Play == false && m_ToggleChange == true)
		// {
		//     //Stop the audio
		//     audioSource.Pause();
		//     //Ensure audio doesn�t play more than once
		//     m_ToggleChange = false;
		// }

		// Check the combo
		curr_combo = 1 + Convert.ToInt32(curr_streak >= x2_streak) + Convert.ToInt32(curr_streak >= x3_streak);

		// Handle beats for each lane individually
		for (int i = 0; i < 4; i++)
		{
			GameObject spot = GameObject.Find("spot" + (i + 1).ToString());
			spot spot_script = spot.GetComponent<spot>();
			// 1st spot – a/h; 2nd spot – s/j; 3rd spot – d/k; 4th spot – f/l
			bool keydown = Input.GetKey(input_asdf[i]) || Input.GetKey(input_hjkl[i]);
			spot_script.spotTapFill.transform.localScale = keydown ? new Vector3(1, 1, 1) : new Vector3(0, 0, 0);
			while (beats[i].Count > 0 && m_Play)
			{
				// Check for keypresses for spots
				if (Input.GetKeyDown(input_asdf[i]) || Input.GetKeyDown(input_hjkl[i]))
				{
					spot_script.tapResponse();
				}

				// if, due to tapResponse(), there is no more beats in the lane, break the loop
				if (beats[i].Count == 0) break;

				// beat sc = beats[0].GetComponent<beat>();
				// sc.setSpeed(40);
				if (beats[i][0].transform.position.y < beats_boundary)
				{
					beat beat_script = beats[i][0].GetComponent<beat>();
					if (beat_script.hasTail)
					{
						Destroy(beat_script.getTail());
					}
					// Destroy(beats[i][0]); // TODO: replace with beats falling animation
					StartCoroutine(FallingBeats(beats[i][0]));
					beats[i].RemoveAt(0);
					if (infinityFlag)
					{
						curr_streak = Math.Max(0, curr_streak - infinityPenalty);
					} else
					{
						curr_streak = Math.Max(0, curr_streak - 25);
					}
				}
				else
				{
					break;
				}
			}
		}

		// Keep track of game score, streak & combo bar
		DispScore.text = "Game Score: " + curr_score.ToString("0.00");
		DispStreak.text = "Streak: " + curr_streak.ToString();
		dispCombo(Mathf.Min(curr_streak, x3_streak));

		// Display perfect
		if (perfect_on)
		{
			perfect_timer += Time.deltaTime;
			if (perfect_timer > perfect_maxtime)
			{
				StartCoroutine(FadeTextToZeroAlpha(1f, Perfect));
				perfect_on = false;
				perfect_timer = 0;
			}
		}

		// Beat timing
		if (infinityFlag)
		{
			if (infinityTimings.penalties.Count > 0)
			{
				if (time_elapsed >= infinityTimings.penalties.Peek().elapsedtime)
				{
					infinityPenalty = infinityTimings.penalties.Dequeue().deduction;
				}
			}
			if (infinityTimings.speedups.Count > 0)
			{
				if (time_elapsed >= infinityTimings.speedups.Peek().elapsedtime)
				{
					speedup s = infinityTimings.speedups.Dequeue();
					StartCoroutine(transition_timescale(s.finaltimescale, s.transitionduration));
				}
			}
			if (!audioSource.isPlaying && m_Play)
			{
				audioSource.Play();
				song_time_elapsed = 0;
				changeTimescale(timescale);
				current_timing_idx = 0;
			}
		}

		infinity_time_elapsed += Time.deltaTime * timescale; // never reset
		song_time_elapsed += Time.deltaTime * timescale; // reset in between infinity loops
		time_elapsed += Time.deltaTime; // TODO: is this efficient?
		if (current_timing_idx < timings.Length)
		{
			if (song_time_elapsed > timings[current_timing_idx])
			{
				beatTiming bt = beatTimings[current_timing_idx++];
				int next_lane = bt.lane - 1; // 0 to 3
				bool has_tail = (bt.length > 0);
				int tail_length = bt.length;
				print(next_lane);
				print(has_tail);
				spawn(next_lane, has_tail, tail_length);

				//int next_lane = UnityEngine.Random.Range(0, 4);

				//spawn(
				//  next_lane,
				//  Mathf.Floor(UnityEngine.Random.Range(0, 1 / long_beat_prob)) == 0,
				//  UnityEngine.Random.Range(2, 5)
				//);
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

		// Check for end of game
		// When game has started and is still playing but the music has ended

		if (infinityFlag)
		{
			if (curr_streak <= 0 && false) // for testing only
			{
				//audioSource.loop = false;
				audioSource.Stop();
				PlayerPrefs.SetFloat("GameScore", curr_score);
				PlayerPrefs.SetFloat("myScore", curr_score);
				SceneManager.LoadScene("EndingScreen");
			}
		} else
		{
			if (game_started && !audioSource.isPlaying && m_Play)
			{
				PlayerPrefs.SetFloat("GameScore", curr_score);
				PlayerPrefs.SetFloat("myScore", curr_score);
				SceneManager.LoadScene("EndingScreen");
			}
		}
	}

	public IEnumerator StartingCountdown(float lag = 1f)
	{
		counting_down = true; print("Starting countdown");
		darkScreen.SetActive(true);
		audioSource.Stop(); // idk y the audio started playing before the game started, after i added the pause functionality
		for (int i = 3; i > 0; i--)
		{
			CountdownText.text = i.ToString();
			yield return new WaitForSeconds(lag);
		}
		CountdownText.text = "";
		// darkScreen.transform.localScale = new Vector3(0, 0, 1);
		darkScreen.SetActive(false);
		game_ready = true;
	}

	public IEnumerator LongBeatsLongPress(GameObject t, float lag = 0.02f)
	{
		tail tail_script = t.GetComponent<tail>();
		int l = tail_script.lane;

		// TODO: currently tails undergoing long press will disappear once game pauses
		// but not sure if this is something we'll actually need to correct
		while (m_Play && (Input.GetKey(input_asdf[l]) || Input.GetKey(input_hjkl[l])) && tail_script.alive)
		{
			curr_score += curr_combo;
			curr_score = Math.Max(curr_score, 0f);
			yield return new WaitForSeconds(lag);
		}
		tail_script.fading = false;
		Destroy(t);
	}

	public IEnumerator FadeTextToFullAlpha(float t, Text i)
	{
		i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
		while (i.color.a < 1.0f)
		{
			i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / t));
			yield return null;
		}
	}

	public IEnumerator FadeTextToZeroAlpha(float t, Text i)
	{
		i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
		while (i.color.a > 0.0f)
		{
			i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
			yield return null;
		}
	}

	public IEnumerator FallingBeats(GameObject b, float lag = 0.05f)
	{
		int i = 0;
		while (b.transform.localScale.x > 0)
		{
			b.transform.localScale = new Vector3(b.transform.localScale.x - i * 0.1f, b.transform.localScale.y - i * 0.1f, b.transform.localScale.z);
			i++;
			yield return new WaitForSeconds(lag);
		}
		Destroy(b);
	}

	public IEnumerator transition_timescale(float finaltimescale, float duration)
	{
		float changeintimescale = finaltimescale - timescale;
		const float interval = 0.05f; // in seconds
		int count = (int)(duration / interval);
		while (count-- > 0)
		{
			timescale += changeintimescale / (duration / interval);
			changeTimescale(timescale);
			yield return new WaitForSeconds(interval);
		}
	}
}
