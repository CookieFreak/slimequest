using UnityEngine;
using System.Collections;

//Plan to add a GoodWill

public class CharacterGUI : MonoBehaviour 
{
	private float currentLevel = 1f;
	private float currentExp = 0f;
//	private float maxLevel = 20;
//	private float maxExp = 100;

	private bool toggleExp = true;

	//public int maxHealth = 100;
	//public int curHealth = 100;
	
//	public int maxMana = 50;
//	public float curMana = 50;

	//public float healthBarLength;
//	public float manaBarLength;
	
	public GUISkin mySkin;
	
	
	void Start () 
	{
	//	healthBarLength = Screen.width / 4;
//		manaBarLength = Screen.width / 8;
	
	}

	void Update ()
	{
		if(Input.GetKeyDown("y")){
			toggleExp = !toggleExp;
		}
	}

	void OnGUI()
	{
		GUI.skin = mySkin;
		
		
		//GUI.Box(new Rect(10, 30, healthBarLength, 20), curHealth + "/" + maxHealth);
		
//		GUI.Box(new Rect(10,35, manaBarLength, 20),curMana  + "/" + maxMana);

		if(!toggleExp)
		{
			GUI.Box(new Rect( 1, 130, Screen.width/3, 20), currentExp.ToString());
		}
		GUI.Label(new Rect( 50, 100, 1000, 1000), "Level :");
		GUI.Label(new Rect( 100, 100, 1000, 1000), currentLevel.ToString());

		
	}
	
/*	public void AdjustCurrentHealth(int adj)
	{
	  curHealth += adj;	
		
		if(curHealth < 0)
		{
			curHealth = 0;
			Destroy(gameObject);
		}
		
		if(curHealth > maxHealth)
			curHealth = maxHealth;
		
		if(maxHealth < 1)
			maxHealth = 1;
		
		healthBarLength = (Screen.width / 4) * (curHealth / (float)maxHealth);
		
	}
	
/*	public void AdjustCurrentMana(int adjM)
	{
		curMana += adjM;
		
		if(curMana > maxMana)
			curMana = maxMana;
		
		manaBarLength = (Screen.width /8) * (curMana / (float) maxMana);
		
		if (curMana  < 0)
		{
			curMana = 0;
		}
	} */

	void LevelExp()
	{
		if (currentExp > 100) {
				currentLevel = 2f;
		}

		if (currentExp > 300) {
				currentLevel = 3f;
		}

		if (currentExp > 900) {
				currentLevel = 4f;
		}

		if (currentExp > 2700) {
				currentLevel = 5f;
		}

		if (currentExp > 8100) {
				currentLevel = 6f;
		}
	}
}
