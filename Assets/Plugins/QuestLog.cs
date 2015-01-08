using UnityEngine;
using System.Collections;

public class QuestLog : MonoBehaviour 
{
	public bool quest1Get = false;
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
		if (quest1Get)
		{
			if (quest1)
			{
				GUI.DrawTexture(new Rect(10,15,questRect.width*0.1f,questRect.height*0.2f), checkboxTicked);
				GUI.Label (new Rect(25,15,questRect.width,questRect.height*0.3f), "Use the Health Potion."); 
			}
			 else 
			{
				GUI.DrawTexture(new Rect(10,15,questRect.width*0.1f,questRect.height*0.2f), checkboxUnticked);
				GUI.Label (new Rect(25,15,questRect.width,questRect.height*0.3f), "Use the Health Potion.");
			}
		}
	}

}
