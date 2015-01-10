using UnityEngine;
using System.Collections;

public class CharacterGUI : MonoBehaviour 
{
	public int maxHealth = 100;
	public int curHealth = 100;
	
	public int maxMana = 50;
	public float curMana = 50;

	public float healthBarLength;
	public float manaBarLength;
	
	public GUISkin mySkin;
	
	
	void Start () 
	{
		healthBarLength = Screen.width / 4;
		manaBarLength = Screen.width / 8;
	
	}
	
	void OnGUI()
	{
		GUI.skin = mySkin;
		
		
		GUI.Box(new Rect(10, 10, healthBarLength, 20), curHealth + "/" + maxHealth);
		
		GUI.Box(new Rect(10,35, manaBarLength, 20),curMana  + "/" + maxMana);
		
	}
	
	public void AdjustCurrentHealth(int adj)
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
	
	public void AdjustCurrentMana(int adjM)
	{
		curMana += adjM;
		
		if(curMana > maxMana)
			curMana = maxMana;
		
		manaBarLength = (Screen.width /8) * (curMana / (float) maxMana);
		
		if (curMana  < 0)
		{
			curMana = 0;
		}
	}
}
