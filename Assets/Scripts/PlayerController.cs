using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
	public Action<int> OnLiveChanged;

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

	[Header("PlayerData")]
	public TMPro.TMP_Text username;
	public GameObject uiPanel;
	public GameObject heartContainer;
	public GameObject heartPrefab;
	public UnityEngine.UI.Slider rechargeSlider;

	[Header("Particles")]
	public ParticleSystem walkParticle;
	public ParticleSystem launchedParticle;
	public ParticleSystem hitParticle;
	public ParticleSystem speakerParticle;

	private bool grounded = false;
	private Vector2 lastDir;
	private Vector2 movement;
	private bool isMoving;
	private bool canPush;
	private float currentPushCooldown = 0f;
	private Renderer[] renderers;
	private Collider collider;
	private int currentLife;

	void Awake()
	{
		rigidBody.freezeRotation = true;
		rigidBody.useGravity = false;
		renderers = GetComponentsInChildren<Renderer>();
		collider = GetComponent<Collider>();

		rechargeSlider.gameObject.SetActive(photonView.IsMine);

		ChangeHealth(4);

		username.text = photonView.Owner.NickName;
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
		if (lastDir != Vector2.zero) conePivot.transform.rotation = Quaternion.LookRotation(new Vector3(lastDir.x, 0, lastDir.y));
		//conePivot.transform.rotation = Quaternion.RotateTowards(conePivot.transform.rotation, Quaternion.LookRotation(lastDir), rotationSpeed * Time.deltaTime);

		if (grounded && isMoving)
		{
			this.transform.position += new Vector3(movement.x, 0, movement.y) * Time.deltaTime * speed;
			if (movement.magnitude >= 0.01f) lastDir = movement;
		}

		if (currentPushCooldown < pushCooldown)
		{
			currentPushCooldown += Time.deltaTime;
			rechargeSlider.value = currentPushCooldown / pushCooldown;
			canPush = false;
		}
		else canPush = true;

		walkParticle.gameObject.SetActive(grounded);
		launchedParticle.gameObject.SetActive(!grounded);
    }

	private IEnumerator Respawn()
    {
		transform.position = GameManager.Instance.spawnpoints[PhotonNetwork.LocalPlayer.GetPlayerNumber()].position;

		yield return new WaitForSeconds(3f);

		photonView.RPC(nameof(RPC_Respawn), RpcTarget.AllViaServer);
    }

	[PunRPC]
	private void RPC_Respawn()
    {
		Debug.Log("Respawning...");
		SetEnabled(renderers, true);
		collider.enabled = true;
		uiPanel.SetActive(true);
		rigidBody.isKinematic = false;

		object lives;
		if(photonView.Controller.CustomProperties.TryGetValue(GameConstants.PLAYER_LIVES, out lives))
        {
			ChangeHealth((int)lives);
			Debug.Log("Changing Health to " + (int)lives);
        }
        else
        {
			Debug.Log("FAIL GET LIVES ON RESPAWN");
        }

	}

	[PunRPC]
	private void DestroyPlayer()
    {
		SetEnabled(renderers, false);
		collider.enabled = false;
		uiPanel.SetActive(false);
		rigidBody.isKinematic = true;

        //Play Death Particle

        if (photonView.IsMine)
        {
			object lives;
			if(PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(GameConstants.PLAYER_LIVES, out lives))
            {
				int intLives = (int)lives;

				if (intLives > 1)
				{
					Debug.Log("Starting Respawn Coroutine...");
					StartCoroutine(Respawn());
					intLives--;
				}

				PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { GameConstants.PLAYER_LIVES, intLives } });
				currentLife = intLives;
			}
		}
    }

	private void ChangeHealth(int val)
	{
		foreach (Transform transform in heartContainer.transform)
		{
			Destroy(transform.gameObject);
		}

		if (val <= 0) return;

		for (int i = 0; i < val - 1; i++)
		{
			Instantiate(heartPrefab, heartContainer.transform);
		}
	}

	public void Move(float horizontal, float vertical)
    {
		movement.x = horizontal;
		movement.y = vertical;
		isMoving = movement.magnitude > 0.01;
	}

	public void Knockback(Vector2 direction, float amountForward, float amountUp, bool playHitParticle = true)
	{
		var vec = new Vector3(direction.normalized.x * amountForward, amountUp, direction.normalized.y * amountForward) - rigidBody.velocity;
		rigidBody.AddForce(vec, ForceMode.VelocityChange);
		if (playHitParticle) hitParticle.Play();
		Debug.Log("<color=red>PUSHED: " + vec + "</color>" + $", a: {direction.normalized.x * amountForward}, amtFwd: {amountForward}");
	}


	public void RPC_Push()
    {
		photonView.RPC("Push", RpcTarget.All);
    }

	[PunRPC]
	public void Push()
	{
		if (canPush)
		{
			Debug.Log("Pushing");

			speakerParticle.Play();

			currentPushCooldown = 0f;

			var objs = coneScript.objInsideCollision;

			if (objs == null) return;

			foreach (GameObject obj in objs)
			{
				var pc = obj.GetComponent<PlayerController>();
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

			Knockback(-lastDir, selfKnockback, selfKnockbackUp, false);

			if (!GameManager.Instance.isCamShaking) StartCoroutine(GameManager.Instance.StartShake(0.2f, 6f));
		}
	}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("KillsPlayer"))
        {
			Debug.Log("Player Should Destroy");
			photonView.RPC("DestroyPlayer", RpcTarget.AllViaServer);
        }
    }

    void OnCollisionStay()
	{
		grounded = true;
	}

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(lastDir.x);
            stream.SendNext(lastDir.y);
        }
        else
        {
            var x = (float)stream.ReceiveNext();
            var y = (float)stream.ReceiveNext();
            this.lastDir.x = x;
            this.lastDir.y = y;
        }
    }

	//============================ UTILITIES ================================

	private void SetEnabled(Renderer[] renderers, bool value)
    {
		foreach (Renderer renderer in renderers)
		{
			renderer.enabled = value;
		}
	}
}
