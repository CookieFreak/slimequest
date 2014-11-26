using UnityEngine;
using System.Collections;

public class PerspectiveFaker : MonoBehaviour 
{
	public GameObject player;
	private Vector3 pos;

	void Update () 
	{
		pos = transform.position;
		PositionChecker ();
	}

	void PositionChecker()
	{
		if (pos.y >= player.transform.position.y) 
		{
			pos.z =0.1f;
			transform.position = pos;
			Debug.Log ("fro");
		}

		else if (pos.y < player.transform.position.y) 
		{
			pos.z = -0.1f;
			transform.position = pos;
			Debug.Log ("bacl");
		}

	}
}
