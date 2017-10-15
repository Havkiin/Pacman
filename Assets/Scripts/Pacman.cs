using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pacman : Photon.MonoBehaviour
{
	[SerializeField]
	AudioSource pellet;

	[SerializeField]
	AudioSource specialPellet;

	public int score { get; private set; }
	bool readyToTurn;
	float speed, powerUpTime, powerUpDuration;
	Vector3 movement;
	GameObject currentNode, scoreBox, spawn;

	float lastSynchronizationTime, syncDelay, syncTime;
	Vector3 syncStartPosition, syncEndPosition;

	void Start()
	{
		score = 0;
		speed = 5.0f;
		powerUpDuration = 4.0f;
		scoreBox = GameObject.Find("ScoreBox");

		lastSynchronizationTime = 0.0f;
		syncDelay = 0.0f;
		syncTime = 0.0f;
		syncStartPosition = Vector3.zero;
		syncEndPosition = Vector3.zero;

		PhotonNetwork.sendRate = 160;
		PhotonNetwork.sendRateOnSerialize = 80;

		if (!photonView.isMine)
		{
			GetComponent<Renderer>().material.color = Color.red;
		}
	}

	void Update()
	{
		if (photonView.isMine)
		{
			Move();
		}
		else
		{
			SyncedMove();
		}

		if (powerUpTime + powerUpDuration < Time.time)
		{
			speed = 5.0f;
		}

		if (photonView.isMine)
		{
			scoreBox.GetComponent<Text>().text = "Score: " + score;
		}
	}

	public void setMovement(Vector3 move)
	{
		movement = move;
	}

	private void Move()
	{
		Vector3 temp = movement;

		if (Input.GetAxis("Horizontal") < 0 && currentNode.GetComponent<Node>().leftNode)
		{
			movement = new Vector3(-speed, 0f, 0f);
		}
		else if (Input.GetAxis("Horizontal") > 0 && currentNode.GetComponent<Node>().rightNode)
		{
			movement = new Vector3(speed, 0f, 0f);
		}
		else if (Input.GetAxis("Vertical") < 0 && currentNode.GetComponent<Node>().downNode)
		{
			movement = new Vector3(0f, 0f, -speed);
		}
		else if (Input.GetAxis("Vertical") > 0 && currentNode.GetComponent<Node>().upNode)
		{
			movement = new Vector3(0f, 0f, speed);
		}

		//If the player turns, snaps them to the grid
		if (temp != movement)
		{
			transform.position = currentNode.transform.position;
			temp = movement;
		}

		//If the player hits a wall, snaps them to grid and stops them
		if ((movement.x > 0f && !currentNode.GetComponent<Node>().rightNode && transform.position.x >= currentNode.transform.position.x)
			|| (movement.x < 0f && !currentNode.GetComponent<Node>().leftNode && transform.position.x <= currentNode.transform.position.x)
			|| (movement.z > 0f && !currentNode.GetComponent<Node>().upNode && transform.position.z >= currentNode.transform.position.z)
			|| (movement.z < 0f && !currentNode.GetComponent<Node>().downNode && transform.position.z <= currentNode.transform.position.z))
		{
			movement = Vector3.zero;
			transform.position = currentNode.transform.position;
		}

		transform.position += movement * Time.deltaTime;

		//Warp to the other end of the map when you reach an end
		if (transform.position.x < -14.0f || transform.position.x > 14.0f)
		{
			transform.position = new Vector3(-transform.position.x, transform.position.y, transform.position.z);
		}
	}

	void SyncedMove()
	{
		syncTime += Time.deltaTime;
		GetComponent<Rigidbody>().position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
	}

	public void setSpawn(GameObject s)
	{
		spawn = s;
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Pellet")
		{
			score += 10;
			pellet.Play();
		}

		if (col.gameObject.tag == "specialPellet")
		{
			score += 50;
			powerUpTime = Time.time;
			speed = 8.0f;
			specialPellet.Play();
		}

		if (col.gameObject.tag == "Node")
		{
			currentNode = col.gameObject;
		}

		if (col.gameObject.tag == "Ghost")
		{
			if (spawn)
			{
				transform.position = spawn.transform.position;
			}

			movement = Vector3.zero;
		}
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.SendNext(GetComponent<Rigidbody>().position);
		}
		else
		{
			syncEndPosition = (Vector3)stream.ReceiveNext();
			syncStartPosition = GetComponent<Rigidbody>().position;
			syncTime = 0.0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;
		}
	}
}
