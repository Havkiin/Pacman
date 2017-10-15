using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pellet : Photon.MonoBehaviour {

	void Start () {

		if (gameObject.tag == "specialPellet")
		{
			StartCoroutine(flash());
		}
	}

	private IEnumerator flash ()
	{
		while (true)
		{
			GetComponent<Renderer>().enabled = false;
			yield return new WaitForSecondsRealtime(0.5f);
			GetComponent<Renderer>().enabled = true;
			yield return new WaitForSecondsRealtime(0.5f);
		}
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.tag == "Player")
		{
			photonView.RPC("destroyPellet", PhotonTargets.AllBuffered);
		}
	}

	[PunRPC]
	void destroyPellet ()
	{
		if (photonView.isMine)
		{
			PhotonNetwork.Destroy(gameObject);
		}
	}
}
