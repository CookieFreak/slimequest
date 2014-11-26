using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour 
{

	public GameObject character;
	
	void Update () 
	{
		gameObject.transform.position = new Vector3 (character.transform.position.x, character.transform.position.y, camera.transform.position.z);
	}
}
