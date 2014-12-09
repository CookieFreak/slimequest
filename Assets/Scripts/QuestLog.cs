using UnityEngine;
using System.Collections;

public class QuestLog : MonoBehaviour 
{
	public bool quest1 = false;
	private Rect questRect = new Rect (Screen.width*0.8f, 0, Screen.width*0.2f, Screen.height*0.1f);
	private bool toggleImg;
	public Texture2D checkboxUnticked;
	public Texture2D checkboxTicked;


	void OnGUI ()
	{
		questRect = GUI.Window (2, questRect, DoMyWindow01, "Quest Log");
	}

	void DoMyWindow01 (int windowId)
	{
		if (!quest1)
		{
			GUI.DrawTexture(new Rect(10,15,questRect.width*0.1f,questRect.height*0.2f), checkboxUnticked);
		}
		if (quest1)
		{
			GUI.DrawTexture(new Rect(10,15,questRect.width*0.1f,questRect.height*0.2f), checkboxTicked);
		}
	}

}
