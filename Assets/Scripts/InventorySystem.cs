using UnityEngine;
using System.Collections;
using System.Collections.Generic;  //need this to access List

public class InventorySystem : MonoBehaviour 
{
	public List<Items> Inventory;  
	public Items[] Bag;
	private Rect windowRect = new Rect (0,0,300,600);
	private RaycastHit hit;
	private Ray ray;
	public GUISkin mySkin;
	private bool inventoryActive = false;
	public float maxRange = 0.4f;
	

	
	//awake then start then update
	void Awake () 
	{
		Inventory = new List<Items>();
		
	}
	
	
	void Update () 
	{
		ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		
		if(Input.GetMouseButtonDown(0))
		{		
			if (Physics.Raycast(ray,out hit,20))
			{
				Debug.Log (hit.collider.gameObject);
				for (int i= 0; i< Bag.Length; i++)
				{
					Debug.Log(Vector2.Distance(transform.position,hit.collider.transform.position));
					if(hit.collider.tag == Bag[i].id.ToString() && Vector2.Distance(transform.position,hit.collider.transform.position) < maxRange)
					{
						Inventory.Add (Bag[i]);
						Destroy(hit.collider.gameObject);
					}
				}
			}
		}
	}
	
	void OnGUI ()
	{
		GUI.skin = mySkin;
		
		if (Input.GetKey(KeyCode.I))
		{
			inventoryActive = true;
		}
		
		if (inventoryActive == true)
		{
			windowRect = GUI.Window (0,windowRect, DoMyWindow, "Inventory");
		}
		
		if (Input.GetKey(KeyCode.Escape) & inventoryActive == true)
		{
			inventoryActive = false;
		}
	}
	
	void DoMyWindow (int windowId)
	{
		int y = 100;
		GUI.DragWindow (new Rect (0,0,600,80));
		//GUI.Button (new Rect (20,20,64,64), "HAI!");
		for (int i = 0; i < Inventory.Count; i++)
		{
			if (GUI.Button(new Rect (20,y,64,64), Inventory[i].icon))
			{
				
				if (Inventory [i].id  == 1)
				{
					CharacterGUI ch = gameObject.GetComponent<CharacterGUI>();
					ch.AdjustCurrentHealth (+25);
					Inventory.Remove(Bag[i]);
					i--;
				}
				
				if (Inventory [i].id == 2)
				{
					CharacterGUI ch = gameObject.GetComponent<CharacterGUI>();
					ch.AdjustCurrentMana (+10);
					Inventory.Remove (Bag[i+1]);
					i--;
				}
			}
			GUI.Label (new Rect (89,y,100,40), Inventory[i].name);
			GUI.Label (new Rect (89,y+20,80,40), Inventory[i].description);
			y+=70;
			
		}
	}
}
