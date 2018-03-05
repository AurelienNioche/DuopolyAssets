using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.AI;

public class AvatarConsumerController : MonoBehaviour 
{
	public float rotationSpeed = 50f;
	public float toleranceForConsuming = 1f;
	public float toleranceForSitting = 0.1f;
	public float timeFading = 0.5f;

	Animator anim;
	NavMeshAgent agent;

	Vector3 goal;
	Vector3 initialScale;

	bool isConsuming = false;
	bool hasAppeared = false;
	bool isWalking = false;

	void Awake () {

		// Get components.
		anim = GetComponent <Animator> ();
		agent = GetComponent <NavMeshAgent> ();

		// Stock initial scale
		initialScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}

	void Start () {
		transform.localScale = new Vector3 (0, 0, 0);
	}

	void Update () {
			
		// If an avatar walks but is arrived, he should stop walking.
		if (IsWalking()) {

			bool isArrived;
			if (isConsuming){
				isArrived = IsArrived (toleranceForConsuming);
			} else {
				isArrived = IsArrived (toleranceForSitting);
			}	

			if (isArrived) {
				StopWalking ();
			}

		} else {
			
			if (isConsuming) {
				LookTowardsCamera (false);
			} else {
				LookTowardsCamera (true);
			}
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
		
	bool IsArrived (float tolerance) {			
		return agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance < tolerance;
	}
		
	void StopWalking () {

		agent.isStopped = true;

		// Used  in order to make an avatar stop walking.
		anim.SetBool ("Walk", false);

		if (!isConsuming) {
			anim.SetBool ("Sit", true);
		}
		isWalking = false;
	}

	IEnumerator Walk () {

		while (!agent.isOnNavMesh) {
			yield return new WaitForEndOfFrame ();
		}

		// Used in order to make an avatar walk
		anim.SetBool ("Walk", true);
		// Used in order to make an avatar walk.
		agent.isStopped = false;
		anim.SetBool ("Sit", false);
		anim.SetBool ("Walk", true);
		agent.SetDestination (goal);
}

	public void MoveToFirm (Vector3 position) {

		// Set goal and make the avatar walk.
		goal = position;
		isConsuming = true;
		isWalking = true;
		StartCoroutine(Walk ());
	}

	public void ComeBack (Vector3 position) {

		if (isConsuming) {
			goal = position;
			isConsuming = false;
			isWalking = true;
			StartCoroutine(Walk ());
		}
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

		if (!isConsuming) {
			anim.SetBool ("Sit", true);
		}
	}

	private IEnumerator FadeOut () {

		float elapsedTime = 0f;

		while (elapsedTime < timeFading) {

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
