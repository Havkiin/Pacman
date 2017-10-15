using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {

	[SerializeField]
	public GameObject upNode;

	[SerializeField]
	public GameObject leftNode;

	[SerializeField]
	public GameObject rightNode;

	[SerializeField]
	public GameObject downNode;

	[SerializeField]
	public bool special;

	//Get the neighbour nodes
	void Awake () {

		RaycastHit hit;

		if (Physics.Raycast(transform.position, Vector3.forward, out hit, 1f) && hit.transform.gameObject.tag == "Node")
		{
			upNode = hit.transform.gameObject;
		}

		if (Physics.Raycast(transform.position, Vector3.left, out hit, 1f) && hit.transform.gameObject.tag == "Node")
		{
			leftNode = hit.transform.gameObject;
		}

		if (Physics.Raycast(transform.position, Vector3.right, out hit, 1f) && hit.transform.gameObject.tag == "Node")
		{
			rightNode = hit.transform.gameObject;
		}

		if (Physics.Raycast(transform.position, Vector3.back, out hit, 1f) && hit.transform.gameObject.tag == "Node")
		{
			downNode = hit.transform.gameObject;
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.DrawSphere(transform.position, 0.2f);
	}
}
