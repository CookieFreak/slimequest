using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour 
{

	public Texture2D background , logo;
	public GUISkin mySkin;
	private string clicked = "";
	
	
	
	
	private void OnGUI()
	{
		//background
		if (background != null)
			GUI.DrawTexture(new Rect (0,-50, 750, 750), background);
		
		if (clicked == "")  // I want the logo to disappear once the Scores button has been clicked
		{
			//logo
			if (logo != null)
				GUI.DrawTexture(new Rect (Screen.width/2-100, 30, 200,200), logo);
		}
		
		
		
		GUI.skin = mySkin;
		
		if(clicked == "")
		{
			//buttons
			if (GUI.Button (new Rect(Screen.width/2 - 100, Screen.height/2 - 100, 200,35), "Play Game"))
			{
				Application.LoadLevel("Login Screen");
			}
			
			if (GUI.Button (new Rect(Screen.width/2 - 100, Screen.height/2 -50, 200,35), "High Scores"))
			{
				clicked = "scores";
			}
			
			if (GUI.Button (new Rect(Screen.width/2 - 100, Screen.height/2 , 200,35), "Quit Game"))
			{
				Application.Quit();
			}
		} 

		else
		{
			GUI.Box (new Rect (0,0,Screen.width,Screen.height/2 +500), "Press Esc to go back!");
			GUI.Label (new Rect (Screen .width/2, Screen.height/2, 300,300), "Score: " + PlayerPrefs.GetInt("collectedMunni"));
		}  
	}  
	
	/*	private void OptionsFunc (int id)
	{
		GUILayout.Box ("Volume");
		
		if (GUILayout.Button("Back"))
		{
			clicked = "";
		}
	}  */
	
	private void Update ()
	{
		if (clicked == "scores" && Input.GetKey (KeyCode.Escape))  //to go back to the main menu
		{
			clicked = "";
		}
	}
}
