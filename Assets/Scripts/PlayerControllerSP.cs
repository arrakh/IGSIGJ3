using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class PlayerControllerSP : MonoBehaviour
{

	public float speed = 10.0f;
	public float gravity = 10.0f;
	public float maxVelocityChange = 10.0f;
	public float rotationSpeed = 5f;
	public float selfKnockback = 2.0f;
	public float selfKnockbackUp = 2.0f;
	public float pushCooldown = 3f;
	public Vector2 otherKnockback;
	public Vector2 otherKnockbackUp;
	public float jumpHeight = 2.0f;
	public Rigidbody rigidBody;
	public GameObject conePivot;
	public Transform coneApex;
	public ConeScript coneScript;

	private bool grounded = false;
	private Vector2 lastDir;
	private Vector2 movement;
	private bool isMoving;
	private bool canPush;
	private float currentPushCooldown = 0f;

	public bool isPlayer = false;

	void Awake()
	{
		rigidBody.freezeRotation = true;
		rigidBody.useGravity = false;
	}

    void FixedUpdate()
	{
		if (grounded && isMoving)
		{
			//// Calculate how fast we should be moving
			//Vector3 targetVelocity = new Vector3(movement.x, 0, movement.y);
			//targetVelocity = transform.TransformDirection(targetVelocity);
			//targetVelocity *= speed;
			//if (movement.magnitude >= 0.01f) lastDir = movement;

			////Debug.Log("<color=blue>-= Is Moving =-</color>");

			//// Apply a force that attempts to reach our target velocity
			//Vector3 velocity = rigidBody.velocity;
			//Vector3 velocityChange = (targetVelocity - velocity);
			//velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
			//velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
			//velocityChange.y = 0;
			//rigidBody.AddForce(velocityChange, ForceMode.VelocityChange);
		}

		// We apply gravity manually for more tuning control
		rigidBody.AddForce(new Vector3(0, -gravity * rigidBody.mass, 0));

		grounded = false;
	}

    private void Update()
    {
		InputDetect();

		if(grounded && isMoving)
        {
			this.transform.position += new Vector3(movement.x, 0, movement.y) * Time.deltaTime * speed;
			if (movement.magnitude >= 0.01f) lastDir = movement;
		}

		if (lastDir != Vector2.zero) conePivot.transform.rotation = Quaternion.LookRotation(new Vector3(lastDir.x, 0, lastDir.y));
		//conePivot.transform.rotation = Quaternion.RotateTowards(conePivot.transform.rotation, Quaternion.LookRotation(lastDir), rotationSpeed * Time.deltaTime);


		if (currentPushCooldown < pushCooldown)
		{
			currentPushCooldown += Time.deltaTime;
			canPush = false;
		}
		else canPush = true;

    }

    void InputDetect()
    {
		if (!isPlayer) return;

        if (Input.GetButtonDown("Push"))
        {
			Push();
        }

		Move(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

	}

	public void Move(float horizontal, float vertical)
    {
		movement.x = horizontal;
		movement.y = vertical;
		isMoving = movement.magnitude > 0.9;
	}

	public void Knockback(Vector2 direction, float amountForward, float amountUp)
    {
		var vec = new Vector3(direction.normalized.x * amountForward, amountUp, direction.normalized.y * amountForward) - rigidBody.velocity;
		rigidBody.AddForce(vec, ForceMode.VelocityChange);
		Debug.Log("<color=red>" + vec + "</color>" + $", a: {direction.normalized.x * amountForward}, amtFwd: {amountForward}");
    }

	
	public void Push()
    {
		if(canPush)
        {
			Debug.Log("Pushing");

			currentPushCooldown = 0f;

			Knockback(-lastDir, selfKnockback, selfKnockbackUp);

			var objs = coneScript.objInsideCollision;

			if (objs == null) return;


			foreach (GameObject obj in objs)
			{
				var pc = obj.GetComponent<PlayerControllerSP>();
				var distToObj = Vector3.Distance(transform.position, obj.transform.position);
				var distToApex = Vector3.Distance(transform.position, coneApex.position);

				if (pc != null && pc != this)
				{
					float amountForward = Mathf.Lerp(otherKnockback.x, otherKnockback.y, distToObj / distToApex);
					float amountUp = Mathf.Lerp(otherKnockbackUp.x, otherKnockbackUp.y, distToObj / distToApex);

					pc.Knockback(lastDir, amountForward, amountUp);

					Debug.Log($"Pushing, fwd: {amountForward}, up: {amountUp}");

				}
			}
		}
    }

	void OnCollisionStay()
	{
		grounded = true;
	}

}
