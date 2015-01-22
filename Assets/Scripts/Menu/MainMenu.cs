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

			if (GUI.Button (new Rect(Screen.width/2 - 100, Screen.height/2 -50, 200,35), "About the Game"))
			{
				clicked = "info";
			}
			
			if (GUI.Button (new Rect(Screen.width/2 - 100, Screen.height/2, 200,35), "Quit Game"))
			{
				Application.Quit();
			}
		} 

		else
		{
			GUI.Box (new Rect (0,0,Screen.width,Screen.height/2 +500), "Press Esc to go back!");
			GUI.Label (new Rect (Screen .width*0.2f, Screen.height*0.1f, 600,300), "In a hidden town, lives a little slime where all neighbors are victims of MMO players. Monsters and animals alike are actually friendly creatures trying to live peacefully. You are to live among and help them with whatever trouble they may have and discover your inner slime.");
		}  
	}  
	
	private void Update ()
	{
		if (clicked == "info" && Input.GetKey (KeyCode.Escape))  //to go back to the main menu
		{
			clicked = "";
		}
	}
}
