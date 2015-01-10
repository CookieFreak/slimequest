using System.Collections;
using UnityEngine;
using TNet;


class GridMove : TNBehaviour 
{
	static public GridMove instance;

	private float moveSpeed = 0.5f;
	private float gridSize = 0.32f;
	private Animator animator;

	private enum Orientation 
	{
		Horizontal,
		Vertical
	};
//	private bool allowDiagonals = false;
//	private bool correctDiagonalSpeed = true;
	private Vector2 input;
	private bool isMoving = false;
	private Vector3 startPosition;
	private Vector3 endPosition;
	private Vector3 prevPosition;
	private bool movingBack = false;
	private float t;
	private float factor = 1;

	public bool stopMovement = false;

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
	void Start()
	{
		animator = this.GetComponent<Animator>();
		Camera.main.GetComponent<CameraController>().target = this.gameObject;

	}



	public void Update() 
	{
		if (tno.isMine)
		{
			if (!stopMovement)
			{
				if (!isMoving) 
				{
					input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
				/*	if (!allowDiagonals) 
					{*/
					if (Mathf.Abs(input.x) > Mathf.Abs(input.y)) 
					{
						input.y = 0;
					} else 
					{
						input.x = 0;
					}
				//	}
					
					if (input != Vector2.zero) 
					{
						prevPosition = transform.position;
						StartCoroutine(move(transform));
					}
				}
			

				var vertical = Input.GetAxis ("Vertical");
				var horizontal = Input.GetAxis ("Horizontal");
				
				if (vertical > 0) {
					animator.SetInteger ("Direction", 2);
					animator.SetFloat ("Speed", 1.0f);
				} else if (vertical < 0) {
					animator.SetInteger ("Direction", 0);
					animator.SetFloat ("Speed", 1.0f);
				} else if (horizontal < 0) {
					animator.SetInteger ("Direction", 1);
					animator.SetFloat ("Speed", 1.0f);
				} else if (horizontal > 0) {
					animator.SetInteger ("Direction", 3);
					animator.SetFloat ("Speed", 1.0f);
				} else {
					animator.SetFloat ("Speed", 0.0f);
				}
			}
		}
	}

	[RFC(5)]
	void  OnSetTarget (Vector3 pos)
	{
		mTarget = pos;
	}

	void OnNetworkPlayerJoin (Player p)
	{
		tno.Send (6, p, transform.position);
	}
	
	[RFC(6)]
	void OnSetTargetImmediate (Vector3 pos)
	{
		transform.position = pos;
	} 

	void OnCollisionStay2D(Collision2D coll){
		if(coll.gameObject.tag == "Collide")
		{	
	//		Debug.Log("Collided");
			if(!movingBack)
			StopCoroutine("move");
			StartCoroutine(moveBack(transform));
		}

		if (coll.gameObject.name == "Chicken") 
		{
			//Activate Dialogue
			// On Trigger Enter
		}
	}
	public IEnumerator move(Transform transform) {
		isMoving = true;
		startPosition = transform.position;
		t = 0;
		endPosition = new Vector3(startPosition.x + System.Math.Sign(input.x) * gridSize,
			                          startPosition.y + System.Math.Sign(input.y) * gridSize);
		
/*		if(allowDiagonals && correctDiagonalSpeed && input.x != 0 && input.y != 0) {
			factor = 0.7071f;
		} else {
			factor = 1f;
		} */
		
		while (t < 1f) {
			t += Time.deltaTime * (moveSpeed/gridSize) * factor;
			transform.position = Vector3.Lerp(startPosition, endPosition, t);
			yield return null;
		}
		
		isMoving = false;
		yield return 0;
	}

	public IEnumerator moveBack(Transform transform) {
		isMoving = true;
		movingBack = true;
		startPosition = transform.position;
		t = 0;
		endPosition = prevPosition;
		
	/*	if(allowDiagonals && correctDiagonalSpeed && input.x != 0 && input.y != 0) {
			factor = 0.7071f;
		} else {
			factor = 1f;
		} */
		
		while (t < 1f) {
			t += Time.deltaTime * (moveSpeed/gridSize);
			transform.position = Vector3.Lerp(startPosition, endPosition, t);
			yield return null;
		}
		movingBack = false;
		isMoving = false;
		yield return 0;
	}
}