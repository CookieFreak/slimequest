﻿using System.Collections;
using UnityEngine;

public enum PlayerState
{
	Idle = 0,
	WalkLeft = 1,
	WalkRight = 2,
	WalkUp = 3,
	WalkDown = 4,
}

public class GridMove : MonoBehaviour 
{
	public PlayerState _characterState;
	
	private float moveSpeed = 0.5f;
	private float gridSize = 0.32f;
	private Animator animator;
	
	private enum Orientation 
	{
		Horizontal,
		Vertical
	};
	
	private Vector2 input;
	private bool isMoving = false;
	private Vector3 startPosition;
	private Vector3 endPosition;
	private Vector3 prevPosition;
	private bool movingBack = false;
	private float t;
	private float factor = 1;
	
	public bool stopMovement = false;
	
	public bool isControllable = true;
	
	
	void Start()
	{
		animator = this.GetComponent<Animator>();
		//		Camera.main.GetComponent<CameraController>().target = this.gameObject;
		
	}
	
	
	
	public void Update() 
	{
		if (isControllable)
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
					_characterState = PlayerState.WalkLeft;
					//				animator.SetInteger ("Direction", 2);
					//				animator.SetFloat ("Speed", 1.0f);
				} else if (vertical < 0) {
					_characterState = PlayerState.WalkRight;
					//				animator.SetInteger ("Direction", 0);
					//				animator.SetFloat ("Speed", 1.0f);
				} else if (horizontal < 0) {
					_characterState = PlayerState.WalkDown;
					//				animator.SetInteger ("Direction", 1);
					//				animator.SetFloat ("Speed", 1.0f);
				} else if (horizontal > 0) {
					_characterState = PlayerState.WalkUp;
					//				animator.SetInteger ("Direction", 3);
					//				animator.SetFloat ("Speed", 1.0f);
				} else {
					
					_characterState = PlayerState.Idle;
					//				animator.SetFloat ("Speed", 0.0f);
				}
			}
		}
		
		if (this.isControllable && !isMoving)
		{
			animator.SetFloat ("Speed", 0.0f);
		}
		
		else
		{
			if (_characterState == PlayerState.Idle)
			{
				animator.SetFloat ("Speed", 0.0f);
			}
			
			else if (_characterState == PlayerState.WalkLeft)
			{
				animator.SetInteger ("Direction", 2);
				animator.SetFloat ("Speed", 1.0f);
			}
			
			else if (_characterState == PlayerState.WalkRight)
			{
				animator.SetInteger ("Direction", 0);
				animator.SetFloat ("Speed", 1.0f);
			}
			
			else if (_characterState == PlayerState.WalkDown)
			{
				animator.SetInteger ("Direction", 1);
				animator.SetFloat ("Speed", 1.0f);
			}
			
			else if (_characterState == PlayerState.WalkUp)
			{
				animator.SetInteger ("Direction", 3);
				animator.SetFloat ("Speed", 1.0f);
			}
		}
	}
	
	void OnCollisionStay2D(Collision2D coll){
		if(coll.gameObject.tag == "Collide")
		{	
			//		Debug.Log("Collided");
			if(!movingBack)
				StopCoroutine("move");
			StartCoroutine(moveBack(transform));
		}
		
	}
	public IEnumerator move(Transform transform) {
		isMoving = true;
		startPosition = transform.position;
		t = 0;
		endPosition = new Vector3(startPosition.x + System.Math.Sign(input.x) * gridSize,
		                          startPosition.y + System.Math.Sign(input.y) * gridSize);
		
		
		
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