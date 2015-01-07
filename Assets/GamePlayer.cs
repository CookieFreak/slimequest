using UnityEngine;
using TNet;

public class GamePlayer : TNBehaviour 
{
	static public GamePlayer instance;

	Vector3 mTarget = Vector3.zero;

	public Vector3 target
	{
		set
		{
			tno.Send(5, TNet.Target.AllSaved,value);
		}
	}

	void Awake () 
	{
		if(TNManager.isThisMyObject)
		{
			instance = this;
		}
	}

	void Update()
	{
	//	transform.position = Vector3.MoveTowards (transform.position, mTarget, 3f * Time.deltaTime);
	}

//	[RFC(5)]
	//void  OnSetTarget (Vector3 pos)
	//{
	//	mTarget = pos;
//	}

	void OnNetworkPlayerJoin (Player p)
	{
		tno.Send (6, p, transform.position);
	}

	[RFC(6)]
	void OnSetTargetImmediate (Vector3 pos)
	{
		transform.position = pos;
	}
}
