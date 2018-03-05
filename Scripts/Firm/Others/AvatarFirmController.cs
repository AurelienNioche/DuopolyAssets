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
	public float timeFading = 0.5f;

	Animator anim;
	NavMeshAgent agent;
	NavMeshObstacle obstacle;

	Vector3 goal;
	Vector3 initialScale;

	bool hasAppeared = false;
	bool isWalking = false;

	void Awake () {

		// Get components.
		anim = GetComponent <Animator> ();
		agent = GetComponent <NavMeshAgent> ();
		obstacle = GetComponent<NavMeshObstacle> ();

		// Stock initial scale
		initialScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}

	void Start () {
		transform.localScale = new Vector3 (0, 0, 0);
	}

	void Update () {
			
		// If an avatar walks but is arrived, he should stop walking
		if (IsWalking()) {
			if (IsArrived ()) {
				StopWalking ();
			} 
		} else {
			LookTowardsCamera (true);
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
		return agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance < tolerance;
	}
		
	void StopWalking () {

		// Used  in order to make an avatar stop walking
		agent.enabled = false;
		obstacle.enabled = true;
		anim.SetBool ("Walk", false);
		isWalking = false;
	}

	IEnumerator Walk () {

		obstacle.enabled = false;
		agent.enabled = true;

		while (!agent.isOnNavMesh) {
			yield return new WaitForEndOfFrame ();
		}

		// Used in order to make an avatar walk
		anim.SetBool ("Walk", true);
		agent.SetDestination (goal);
	}

	public void SetGoal (Vector3 position) {

		// Set goal and make the avatar walk
		goal = position;
		isWalking = true;
		StartCoroutine(Walk ());
	}

	public bool IsWalking () {
		return isWalking;
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
		
	public void StopAnimations () {
		anim.SetBool ("Walk", false);
		anim.SetBool ("Sit", false);
		anim.ResetTrigger ("Jump");
	} 

	private IEnumerator FadeIn () {
		
		float elapsedTime = 0f;

		anim.SetTrigger ("Jump");

		while (elapsedTime < timeFading) {
				
			transform.localScale = new Vector3(
				Mathf.Lerp (transform.localScale.x, initialScale.x, (elapsedTime / timeFading)), 
				Mathf.Lerp (transform.localScale.y, initialScale.y, (elapsedTime / timeFading)), 
				Mathf.Lerp (transform.localScale.z, initialScale.z, (elapsedTime / timeFading)) 
			);

			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator FadeOut () {
		
		float elapsedTime = 0f;
		float time = 0.5f;

		while (elapsedTime < time) {

			transform.localScale = new Vector3(
				Mathf.Lerp (transform.localScale.x, 0, (elapsedTime / timeFading)), 
				Mathf.Lerp (transform.localScale.y, 0, (elapsedTime / timeFading)), 
				Mathf.Lerp (transform.localScale.z, 0, (elapsedTime / timeFading)) 
			);

			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
	}

}
