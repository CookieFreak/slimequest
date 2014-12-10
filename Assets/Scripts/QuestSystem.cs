using UnityEngine;
using System.Collections;

public class QuestSystem : MonoBehaviour 
{
	private bool dialogueActive = false;
	private Rect windowRect = new Rect (0, Screen.height*0.8f, Screen.width, Screen.height*0.2f);
	private GameObject player;
	public QuestLog questController;
	
	void OnGUI ()
	{
		if (dialogueActive)
		{
			windowRect = GUI.Window (3,windowRect, DoMyWindow, "");
			player.GetComponent<GridMove>().stopMovement = true;
		}
	}

	void DoMyWindow (int windowId)
	{
		GUI.Label(new Rect(10,0,100,30),"Chicken");

		GUI.Label(new Rect(10,30,windowRect.width,windowRect.height),"BakBak bakabkabkabkabkabkabakbakbakak!");

		if(GUI.Button(new Rect (windowRect.width *0.8f,windowRect.height*0.8f,windowRect.width*0.1f,windowRect.height*0.1f), "Uhhh...Okay"))
		{
			dialogueActive = false;
			player.GetComponent<GridMove>().stopMovement = false;
			questController.quest1Get = true;

		}
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		if (other.gameObject.tag == "Player")
		{
			player = other.gameObject;
			dialogueActive = true;
		}
	}
}
