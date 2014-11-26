using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
	
	private Animator animator;
	public float speed = 2.0f;
//	private Vector2 moveDirection = Vector2.zero;
	
	private Vector3 startPosition;
	private Vector3 endPosition;
	private float t;
	private Vector2 input;
	private bool isMoving = false;
	private CharacterController controller;

	// Use this for initialization
	void Start()
	{
		animator = this.GetComponent<Animator>();
		controller = GetComponent<CharacterController> ();

	}
	
	// Update is called once per frame
	void Update()
	{
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
		
/*
        moveDirection = new Vector2 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical"));
        moveDirection = transform.TransformDirection (moveDirection);
        moveDirection *= speed;


        controller.Move (moveDirection * Time.deltaTime);
        */
		
		
		
		if (!isMoving) {
			input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
			if (Mathf.Abs(input.x) > Mathf.Abs(input.y)) {
				input.y = 0;
			} else {
				input.x = 0;
			}
			
			
			
			if (input != Vector2.zero) {
				StartCoroutine(move(transform));
			}
		}
	}
	
	public IEnumerator move(Transform transform) {
		isMoving = true;
		startPosition = transform.position;
		t = 0;
		endPosition = new Vector3(startPosition.x + System.Math.Sign(input.x) * 0.32f,
		                          startPosition.y + System.Math.Sign(input.y) * 0.32f);
		
		while (t < 1f) {
			t += Time.deltaTime * speed;
		//	transform.position = Vector3.Lerp(startPosition, endPosition, t);
			controller.Move(endPosition - transform.position);
			yield return null;
		}
		
		isMoving = false;
		yield return 0;
	}
}