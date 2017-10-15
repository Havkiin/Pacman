using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : Photon.MonoBehaviour {

	int direction;
	bool inSpawn, goOut, cornerNode;
	float speed;
	Vector3 movement;
	GameObject currentNode, target;

	float lastSynchronizationTime, syncDelay, syncTime;
	Vector3 syncStartPosition, syncEndPosition;

	void Start () {

		speed = 5.0f;
		inSpawn = true;
		goOut = false;

		lastSynchronizationTime = 0.0f;
		syncDelay = 0.0f;
		syncTime = 0.0f;
		syncStartPosition = Vector3.zero;
		syncEndPosition = Vector3.zero;

		PhotonNetwork.sendRate = 160;
		PhotonNetwork.sendRateOnSerialize = 80;

		if (photonView.isMine)
		{
			randomColor();
		}

		StartCoroutine(assignTarget());
	}
	
	void Update () {

		//If still in spawn, move left and right
		if (inSpawn && !goOut)
		{
			movement = new Vector3(direction * speed, 0f, 0f);
		}
		//Go out of spawn
		else if (inSpawn && goOut)
		{
			transform.position = Vector3.Lerp(transform.position, new Vector3(0.5f, 0.1f, 4.5f), 0.5f);

			if (currentNode.transform.parent.name != "GhostNodes")
			{
				inSpawn = false;
			}
		}
		//Move
		else
		{
			if (photonView.isMine)
			{
				Move(target);
			}
			else
			{
				SyncedMove();
			}
		}

		//If the ghost hits a wall, snaps them to grid and stops them
		if ((movement.x > 0f && !currentNode.GetComponent<Node>().rightNode && transform.position.x >= currentNode.transform.position.x)
			|| (movement.x < 0f && !currentNode.GetComponent<Node>().leftNode && transform.position.x <= currentNode.transform.position.x)
			|| (movement.z > 0f && !currentNode.GetComponent<Node>().upNode && transform.position.z >= currentNode.transform.position.z)
			|| (movement.z < 0f && !currentNode.GetComponent<Node>().downNode && transform.position.z <= currentNode.transform.position.z))
		{
			movement = Vector3.zero;
			transform.position = currentNode.transform.position;

			if (inSpawn)
			{
				direction = -direction;
			}
			else
			{
				cornerNode = true;
			}
		}
		else if ((movement.x != 0f && (currentNode.GetComponent<Node>().upNode || currentNode.GetComponent<Node>().downNode))
			|| (movement.z != 0f && (currentNode.GetComponent<Node>().leftNode || currentNode.GetComponent<Node>().rightNode)))
		{
			cornerNode = true;
		}

		transform.position += movement * Time.deltaTime;

		//Warp to the other end of the map when you reach an end
		if (transform.position.x < -14.0f || transform.position.x > 14.0f)
		{
			transform.position = new Vector3(-transform.position.x, transform.position.y, transform.position.z);
		}
	}

	public void timeToGoOut ()
	{
		goOut = true;
	}

	public void setMovement (Vector3 move)
	{
		movement = move;
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

	//Finds the closest player and assigns it as a target
	private IEnumerator assignTarget ()
	{
		while (true)
		{
			GameObject player1 = GameObject.FindGameObjectsWithTag("Player")[0];

			if (GameObject.FindGameObjectsWithTag("Player").Length > 1)
			{
				GameObject player2 = GameObject.FindGameObjectsWithTag("Player")[1];
				float distanceP1 = Vector3.Distance(transform.position, player1.transform.position);
				float distanceP2 = Vector3.Distance(transform.position, player2.transform.position);

				if (distanceP1 < distanceP2)
				{
					target = player1;
				}
				else
				{
					target = player2;
				}
			}
			else
			{
				target = player1;
			}

			yield return new WaitForSecondsRealtime(10f);
		}
	}

	private void Move(GameObject t)
	{
		Vector3 targetPos = t.transform.position;
		Vector3 dir = movement;

		if (cornerNode)
		{
			//Target is on the left
			if (targetPos.x < transform.position.x)
			{
				if (currentNode.GetComponent<Node>().leftNode)
				{
					movement = new Vector3(-speed, 0f, 0f);
				}
				else
				{
					//Bottom corner
					if (targetPos.y < transform.position.y)
					{
						if (currentNode.GetComponent<Node>().downNode)
						{
							movement = new Vector3(0f, 0f, -speed);
						}
						else
						{
							movement = new Vector3(0f, 0f, speed);
						}
					}
					//Top corner
					else if (targetPos.y > transform.position.y)
					{
						if (currentNode.GetComponent<Node>().upNode)
						{
							movement = new Vector3(0f, 0f, speed);
						}
						else
						{
							movement = new Vector3(0f, 0f, -speed);
						}
					}
				}
			}
			//Target is on the right
			else if (targetPos.x > transform.position.x)
			{
				if (currentNode.GetComponent<Node>().rightNode)
				{
					movement = new Vector3(speed, 0f, 0f);
				}
				else
				{
					//Bottom corner
					if (targetPos.y < transform.position.y)
					{
						if (currentNode.GetComponent<Node>().downNode)
						{
							movement = new Vector3(0f, 0f, -speed);
						}
						else
						{
							movement = new Vector3(0f, 0f, speed);
						}
					}
					//Top corner
					else if (targetPos.y > transform.position.y)
					{
						if (currentNode.GetComponent<Node>().upNode)
						{
							movement = new Vector3(0f, 0f, speed);
						}
						else
						{
							movement = new Vector3(0f, 0f, -speed);
						}
					}
				}
			}
			else
			{
				//Bottom corner
				if (targetPos.y < transform.position.y)
				{
					if (currentNode.GetComponent<Node>().downNode)
					{
						movement = new Vector3(0f, 0f, -speed);
					}
					else
					{
						movement = new Vector3(0f, 0f, speed);
					}
				}
				//Top corner
				else if (targetPos.y > transform.position.y)
				{
					if (currentNode.GetComponent<Node>().upNode)
					{
						movement = new Vector3(0f, 0f, speed);
					}
					else
					{
						movement = new Vector3(0f, 0f, -speed);
					}
				}
			}

			if (dir != movement)
			{
				transform.position = currentNode.transform.position;
			}
		}
	}

	private void SyncedMove()
	{
		syncTime += Time.deltaTime;
		GetComponent<Rigidbody>().position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
	}

	void OnTriggerEnter (Collider col)
	{
		if (col.gameObject.tag == "Node")
		{
			currentNode = col.gameObject;

			if (inSpawn)
			{
				direction = Random.Range(0, 2);
				direction = (direction == 0) ? -1 : direction;
			}

			if (cornerNode)
			{
				cornerNode = false;
			}
		}

		if (col.gameObject.tag == "Player")
		{
			GetComponent<AudioSource>().Play();
			assignTarget();
		}
	}

	//Set a random color to the ghost on game start
	public void randomColor()
	{
		float red = Random.Range(0f, 1000f) / 1000f;
		float green = Random.Range(0f, 1000f) / 1000f;
		float blue = Random.Range(0f, 1000f) / 1000f;

		changeColor(new Vector3(red, blue, green));
	}

	[PunRPC]
	private void changeColor(Vector3 col)
	{
		GetComponent<Renderer>().material.color = new Color(col.x, col.y, col.z);

		if (photonView.isMine)
		{
			photonView.RPC("changeColor", PhotonTargets.OthersBuffered, col);
		}
	}
}
