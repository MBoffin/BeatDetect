using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class Beats : MonoBehaviour {

	[Header("Visualization Parameters")]
    [Range(4, 24)]
    public int numberOfSampleGroups;
	[Range(1,2)]
    public float distance;

    public GameObject cubePrefab;
	private GameObject[] cubes;
	public GameObject sliverStartPoint;
    private GameObject[] sliverStartPoints;

    [Header("Audio Analysis Parameters")]
	[Tooltip("How many samples of the currently playing audio to get each frame.")]
    public int sampleSize = 1024;
    private float[] samples;
    [Tooltip("How fast or slow the cubes should shrink back down after getting popped up by the audio.")]
    public float smoothSpeed;
    [Tooltip("How much to scale up or down the size of the cube based on the magnitude of the audio playing.")]
	public float visScale = 0.001f;
    private float logBase;
    private float maxBeatSize;
    
    [Space(20)]
    public Text musicName;

	private AudioSource music;


	void Start() {
        maxBeatSize = 0;
        samples = new float[sampleSize];
		music = GetComponent<AudioSource>();
        musicName.text = music.clip.name;

        // This is all the visual setup stuff. Not actually needed if 
        // you're just going to process the audio programmatically and 
        // need no visualization of the music.
        cubes = new GameObject[numberOfSampleGroups];
		for (int i = 0; i < numberOfSampleGroups; i++) {
			float newX = (i * distance) - ((numberOfSampleGroups * distance) / 2);
			Vector3 newPos = new Vector3(newX, 0, 0);
			cubes[i] = Instantiate(cubePrefab, newPos, Quaternion.identity) as GameObject;
            cubes[i].transform.parent = transform;
        }
		sliverStartPoints = new GameObject[numberOfSampleGroups];
		for (int i = 0; i < numberOfSampleGroups; i++) {
			float newX = (i * distance) - ((numberOfSampleGroups * distance) / 2);
			Vector3 newPos = new Vector3(newX, 0, -1);
			sliverStartPoints[i] = Instantiate(sliverStartPoint, newPos, Quaternion.identity) as GameObject;
            sliverStartPoints[i].transform.parent = transform;
			sliverStartPoints[i].SetActive(false);
		}

        // This is important. This is what sets up the math for splitting
        // the audio frequencies into groups. Lower frequencies need fewer
        // samples in each sample group, while higher frequencies need
        // more samples in each sample group. Using a logarithmic scale
        // gives you a nice smooth exponentially growing grouping of samples.
		logBase = Mathf.Log(sampleSize, numberOfSampleGroups);
	}

	void Update() {
        // What's playing RIGHT NOW from the music
        music.GetSpectrumData(samples, 0, FFTWindow.Hamming);

        // For each sample group....
		for (int i = 0; i < numberOfSampleGroups; i++) {

            // Use the logBase from above to get the low/high frequencies
            // for this particular sample group
			int lowFreq = Mathf.FloorToInt(Mathf.Pow(i, logBase));
			int highFreq = Mathf.FloorToInt(Mathf.Pow(i + 1, logBase)) - 1;

            // Get the size of this sample group's "beat" for this frame
			float thisBeat = GetBeatScale(lowFreq, highFreq);
            
            // This is just for keeping track of what the largest beat size is.
            // Not actually needed for anything other than knowing relative
            // beat sizes.
            if (thisBeat > maxBeatSize) {
                maxBeatSize = thisBeat;
                Debug.Log("New Max Size: " + maxBeatSize);
            }

            // Pop this sample group's cube up, based on beat size, and then
            // let it smoothly shrink back down
            float newY = 0.1f + (visScale * thisBeat);
            float localY = cubes[i].transform.localScale.y;
			if (newY < localY * 1.2f) {
				newY = Mathf.SmoothStep(localY, 0.1f, Time.deltaTime * smoothSpeed);
			}
            Vector3 newScale = new Vector3(1, newY, 1);
            cubes[i].transform.localScale = newScale;
            Vector3 newPos = new Vector3(cubes[i].transform.position.x, newY / 2, cubes[i].transform.position.z);
            cubes[i].transform.position = newPos;

            // Create the green slivers that flow out from the sample group
            // cubes. (Uses the object pooler, since there are a lot.)
            Color thisColor = new Color(0, Mathf.Clamp01(thisBeat / 1.75f), 0);
            GameObject newSliver = ObjectPool.current.getObject();
            newSliver.transform.position = sliverStartPoints[i].transform.position;
            newSliver.transform.rotation = Quaternion.identity;
            newSliver.renderer.material.color = thisColor;
            newSliver.transform.parent = transform;
            newSliver.SetActive(true);
		}
	}

    // Add up the sample sizes for the frequencies currently being played
    // right now and determine how "much" of that sample group is playing.
	float GetBeatScale(int lowFreq, int highFreq) {
		float current = 0;
		for (int i = lowFreq; i <= highFreq; i++) {
			current += samples[i];
		}
		return current;
	}



}
