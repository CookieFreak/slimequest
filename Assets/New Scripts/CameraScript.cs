// By: Sarah Omar

using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour 
{
	public Transform cameraTransform;
	private Transform _target;
	
	// The distance in the x-z plane to the target
	
	public float distance = 125;
	
	// the height we want the camera to be above the target
	
	public float heightSmoothLag = 0.3f;
	
	public float snapSmoothLag = 0.2f;
	public float snapMaxSpeed = 720.0f;
	
	private Vector3 headOffset = Vector3.zero;
	private Vector3 centerOffset = Vector3.zero;
	
	private float heightVelocity = 0.0f;
	private GridMove controller;
	private float targetHeight = 100000.0f;
	

	void OnEnable()
	{
		if( !cameraTransform && Camera.main )
			cameraTransform = Camera.main.transform;
		if( !cameraTransform )
		{
			Debug.Log( "Please assign a camera to the ThirdPersonCamera script." );
			enabled = false;
		}

		_target = transform;
		if( _target )
		{
			controller = _target.GetComponent<GridMove>();
		}

		Cut( _target, centerOffset );
	}

	void DebugDrawStuff()
	{
		Debug.DrawLine( _target.position, _target.position + headOffset );
		
	}

	void Apply( Transform dummyTarget, Vector3 dummyCenter )
	{
		// Early out if we don't have a target
		if (!controller)
				return;

		Vector3 targetCenter = _target.position + centerOffset;
		targetHeight = targetCenter.y;

		// Damp the height
		float currentHeight = cameraTransform.position.y;
		currentHeight = Mathf.SmoothDamp (currentHeight, targetHeight, ref heightVelocity, heightSmoothLag);

		// Calculate the current & target rotation angles
		float currentAngle = cameraTransform.eulerAngles.y;
		
		// Convert the angle into a rotation, by which we then reposition the camera
		Quaternion currentRotation = Quaternion.Euler( 0, currentAngle, 0 );


		// Set the position of the camera on the x-z plane to:
		// distance meters behind the target
		cameraTransform.position = targetCenter;
		cameraTransform.position += currentRotation * Vector3.back * distance;

		// Set the height of the camera
		cameraTransform.position = new Vector3 (cameraTransform.position.x, currentHeight, cameraTransform.position.z);

		// Always look at the target	
	}
	void LateUpdate()
	{
		Apply( transform, Vector3.zero );
	}
	
	void Cut( Transform dummyTarget, Vector3 dummyCenter )
	{
		float oldHeightSmooth = heightSmoothLag;
		float oldSnapMaxSpeed = snapMaxSpeed;
		float oldSnapSmooth = snapSmoothLag;
		
		snapMaxSpeed = 10000;
		snapSmoothLag = 0.001f;
		heightSmoothLag = 0.001f;
		
		Apply( transform, Vector3.zero );
		
		heightSmoothLag = oldHeightSmooth;
		snapMaxSpeed = oldSnapMaxSpeed;
		snapSmoothLag = oldSnapSmooth;
	}
	
	Vector3 GetCenterOffset()
	{
		return centerOffset;
	}

}
