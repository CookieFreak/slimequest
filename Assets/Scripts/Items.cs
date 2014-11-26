using UnityEngine;
using System.Collections;
[System.Serializable]

public class Items
{
	public string name;
	public string description;
	public int id;
	public Texture2D icon;
	public GameObject player;
	
	public enum Type
	{
		potion, weapon, armor
	}
	
	public Type type;
	
	
}
