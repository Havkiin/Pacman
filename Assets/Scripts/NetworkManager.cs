using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : Photon.MonoBehaviour {

	[SerializeField]
	GameObject player;

	[SerializeField]
	GameObject pellet;

	[SerializeField]
	GameObject specialPellet;

	[SerializeField]
	GameObject ghost;

	[SerializeField]
	GameObject nodeList;

	[SerializeField]
	GameObject ghostNodeList;

	[SerializeField]
	GameObject spawn1;

	[SerializeField]
	GameObject spawn2;

	float ghostTimer;
	string roomName = "Pacman";
	RoomInfo[] roomsList;
	GameObject player1, player2, endText;
	GameObject[] ghostsInSpawn;

	void Start () {

		Screen.SetResolution(460, 620, false);
		PhotonNetwork.ConnectUsingSettings("0.1");
		ghostsInSpawn = new GameObject[4];
		endText = GameObject.Find("EndText");
	}
	
	void Update () {

		if (ghostTimer > 0f)
		{
			if (Time.time > ghostTimer + 5f)
			{
				ghostsInSpawn[0].GetComponent<Ghost>().timeToGoOut();
			}

			if (Time.time > ghostTimer + 10f)
			{
				ghostsInSpawn[1].GetComponent<Ghost>().timeToGoOut();
			}

			if (Time.time > ghostTimer + 15f)
			{
				ghostsInSpawn[2].GetComponent<Ghost>().timeToGoOut();
			}

			if (Time.time > ghostTimer + 20f)
			{
				ghostsInSpawn[3].GetComponent<Ghost>().timeToGoOut();
			}

			//All pellets have been consumed, GAME OVER
			if (GameObject.FindGameObjectsWithTag("Pellet").Length == 0 && Time.time > ghostTimer + 5f)
			{
				GameOver();
			}
		}
	}

	void OnGUI ()
	{
		if (!PhotonNetwork.connected)
		{
			GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
		}
		else if (PhotonNetwork.room == null)
		{
			//Create room
			if (GUI.Button(new Rect(100, 100, 250, 100), "Start Server"))
			{
				PhotonNetwork.CreateRoom(roomName + Guid.NewGuid().ToString("N"));
				ghostTimer = Time.time;
			}

			//Join Room
			if (roomsList != null)
			{
				for (int i = 0; i < roomsList.Length; i++)
				{
					if (GUI.Button(new Rect(100, 250 + (110 * i), 250, 100), "Join " + roomsList[i].Name))
					{
						PhotonNetwork.JoinRoom(roomsList[i].Name);
					}
				}
			}
		}
	}

	void OnReceivedRoomListUpdate()
	{
		roomsList = PhotonNetwork.GetRoomList();
	}

	void OnCreatedRoom()
	{
		//Spawn pac-dots
		foreach (Transform node in nodeList.transform)
		{
			if (node.gameObject.GetComponent<Node>().special)
			{
				PhotonNetwork.Instantiate(specialPellet.name, node.position, Quaternion.identity, 0);
			}
			else
			{
				PhotonNetwork.Instantiate(pellet.name, node.position, Quaternion.identity, 0);
			}
		}

		//Spawn ghosts
		for (int i = 0; i < 4; i++)
		{
			int rand = UnityEngine.Random.Range(0, 6);
			ghostsInSpawn[i] = PhotonNetwork.Instantiate(ghost.name, ghostNodeList.transform.GetChild(rand).transform.position, ghost.transform.rotation, 0);
		}
	}

	void OnJoinedRoom()
	{
		int playerID = PhotonNetwork.playerList.Length;

		if (playerID == 1)
		{
			player1 = PhotonNetwork.Instantiate(player.name, spawn1.transform.position, Quaternion.identity, 0);
			player1.GetComponent<Pacman>().setSpawn(spawn1);
		}
		else if (playerID == 2)
		{
			player2 = PhotonNetwork.Instantiate(player.name, spawn2.transform.position, Quaternion.identity, 0);
			player2.GetComponent<Pacman>().setSpawn(spawn2);
		}
	}

	[PunRPC]
	private void GameOver()
	{
		GameObject player1 = GameObject.FindGameObjectsWithTag("Player")[0];
		GameObject player2 = GameObject.FindGameObjectsWithTag("Player")[1];

		endText.GetComponent<Text>().text = "Game Over!";

		player1.GetComponent<Pacman>().setMovement(Vector3.zero);
		player1.GetComponent<Pacman>().enabled = false;
		player2.GetComponent<Pacman>().setMovement(Vector3.zero);
		player2.GetComponent<Pacman>().enabled = false;

		foreach (GameObject ghost in ghostsInSpawn)
		{
			ghost.GetComponent<Ghost>().setMovement(Vector3.zero);
			ghost.GetComponent<Ghost>().enabled = false;
		}

		if (photonView.isMine)
		{
			photonView.RPC("GameOver", PhotonTargets.OthersBuffered);
		}

		GetComponent<AudioSource>().Play();

		enabled = false;
	}
}
