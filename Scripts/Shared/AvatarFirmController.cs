using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.AI;

public class AvatarFirmController : MonoBehaviour 
{
	public float rotationSpeed = 10f;
	public float tolerance = 0.5f;

	Animator anim;
	NavMeshAgent nav;
	NavMeshObstacle obstacle;

	Vector3 goal;

	Vector3 initialScale;

	bool isWalking = false;
	bool hasAppeared = false;

	void Awake () {

		// Get components.
		anim = GetComponent <Animator> ();
		nav = GetComponent <UnityEngine.AI.NavMeshAgent> ();
		obstacle = GetComponent<UnityEngine.AI.NavMeshObstacle> ();

		// Stock initial scale
		initialScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}

	void Start () {
		transform.localScale = new Vector3 (0, 0, 0);
	}

	void Update () {
			
		// If an avatar walks but is arrived, he should stop walking.
		if (isWalking) {
			if (IsArrived ()) {
				StopWalking ();
			} 
		} else {
			LookTowardsCamera(true);
		}
	}

	void LookTowardsCamera (bool value) {
		Quaternion goalRotation = Quaternion.identity;;
		if (!value) {
			goalRotation *= Quaternion.Euler (0, 180f, 0);
		} 
		transform.rotation = Quaternion.Slerp (
			transform.rotation, 
			goalRotation, 
			Time.deltaTime * rotationSpeed
		);

	}
		
	bool IsArrived () {

		// Check if the avatar reached his goal.
		Vector3 position = transform.position;

		bool cond1;
		bool cond2;

		cond1 = position.x <= (goal.x + tolerance) && position.x >= (goal.x - tolerance);
		cond2 = position.z <= (goal.z + tolerance) && position.z >= (goal.z - tolerance);	
	
		return cond1 && cond2;
	}
		
	void StopWalking () {

		// Used  in order to make an avatar stop walking.
		nav.isStopped = true;
		anim.SetBool ("Walk", false);
		isWalking = false;
	}

	void Walk () {

		// Used in order to make an avatar walk.
		nav.isStopped = false;
		anim.SetBool ("Walk", true);
		nav.SetDestination (goal);
		isWalking = true;
	}

	public void SetGoal (Vector3 position) {

		// Set goal and make the avatar walk.
		goal = position;
		Walk ();
	}

	public bool GetIsWalking () {
		return isWalking;
	}

	public void SetActive (bool active) {
		if (active) {
			obstacle.enabled = !active;
			nav.enabled = active;	
		} else {
			nav.enabled = active;	
			obstacle.enabled = !active;
		}

	}

	public void Appear () {
		if (!hasAppeared) {
			StartCoroutine (FadeIn ());
			hasAppeared = true;
		}
	}

	public void Disappear () {
		if (hasAppeared) {
			StartCoroutine (FadeOut ());
			hasAppeared = false;
		}
	}

	private IEnumerator FadeIn () {
		
		float elapsedTime = 0f;
		float time = 0.5f;

		anim.SetTrigger ("Jump");

		while (elapsedTime < time)
		{
				
			transform.localScale = new Vector3(
				Mathf.Lerp (transform.localScale.x, initialScale.x, (elapsedTime / time)), 
				Mathf.Lerp (transform.localScale.y, initialScale.y, (elapsedTime / time)), 
				Mathf.Lerp (transform.localScale.z, initialScale.z, (elapsedTime / time)) 
			);

			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator FadeOut () {
		
		float elapsedTime = 0f;
		float time = 0.5f;

		while (elapsedTime < time)
		{

			transform.localScale = new Vector3(
				Mathf.Lerp (transform.localScale.x, 0, (elapsedTime / time)), 
				Mathf.Lerp (transform.localScale.y, 0, (elapsedTime / time)), 
				Mathf.Lerp (transform.localScale.z, 0, (elapsedTime / time)) 
			);

			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
	}

}
