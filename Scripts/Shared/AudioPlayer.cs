using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour {

	public AudioClip [] audioClips;

	AudioSource audioSource;

	int currentClipId = 0;

	void Awake () {
	}
	// Use this for initialization
	void Start () {

		audioSource = GetComponent<AudioSource> ();
		audioSource.loop = false;
		audioSource.Play ();
		
	}

	AudioClip GetRandomClip () {
		int newClipId = currentClipId;
		while (newClipId == currentClipId) {
			newClipId = Random.Range (0, audioClips.Length);
		}
		currentClipId = newClipId;
		return audioClips [newClipId];
	}
	
	// Update is called once per frame
	void Update () {
		if (!audioSource.isPlaying) {
			audioSource.clip = GetRandomClip();
			audioSource.Play ();
		}
	}

	public void Mute () {
		if (!audioSource.mute) {
			audioSource.mute = true;
		} else {
			audioSource.mute = false;
		}
	}
}
