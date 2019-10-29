using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomLightPulse : MonoBehaviour {
	private Animation anim;

	void Start() {
		anim = GetComponent<Animation>();

		foreach (AnimationState state in anim) {
			state.time = Random.Range(0.0f, 0.99f);
			InvokeRepeating(nameof(ChangeSpeed), 2.0f, 2.0f);
		}
	}

    private void ChangeSpeed() {
		foreach (AnimationState state in anim) {
			state.speed = Random.Range(0.4f, 0.8f);
		}
	}
}
