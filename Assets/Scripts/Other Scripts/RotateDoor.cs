using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateDoor : MonoBehaviour
{
	public Vector3 openRotAngle;    // rotation angle when open
	public Vector3 closeRotAngle;   // rotation angle when closed
	public float speed = 1.0f;      // door rotation speed

	bool isRotating = false;        // is the door currently rotating
	int direction = -1;             // rotation direction
	float interpolate = 0.0f;       // interpolation amount

	// Update is called once per frame
	void Update()
	{
		// if rotation is required
		if(isRotating)
		{
			// calculate the interpolation amount
			interpolate += direction * speed * Time.deltaTime;

			// if door completely opened
			if (interpolate > 1)
			{
				interpolate = 1;
				isRotating = false;
			}
			// if door completely closed
			else if (interpolate < 0)
			{
				interpolate = 0;
				isRotating = false;
			}

			// interpolate rotation between open and close rotation angles
			transform.localRotation = Quaternion.Euler(Vector3.Lerp(closeRotAngle, openRotAngle, interpolate));
		}
	}

	// set values to open the door
	void Activate()
	{
		isRotating = true;
		direction = 1;
	}
	// set values to close the door
	void Deactivate()
	{
		isRotating = true;
		direction = -1;
	}
}
