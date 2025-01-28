using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))] // enforces dependency on character controller
[AddComponentMenu("Control Script/FPS Input")]  // add to the Unity editor's component menu
public class FPSInput : MonoBehaviour
{
	// movement sensitivity
	public float speed = 6.0f;

	// gravity setting
	public float gravity = -9.8f;

	// reference to the character controller
	private CharacterController charController;

	Camera cam;

	// Start is called before the first frame update
	void Start()
	{
		// get the character controller component
		charController = GetComponent<CharacterController>();

		cam = Camera.main;
	}

	// Update is called once per frame
	void Update()
	{
		// changes based on WASD keys
		float deltaX = Input.GetAxis("Horizontal") * speed;
		float deltaZ = Input.GetAxis("Vertical") * speed;
		Vector3 movement = new Vector3(deltaX, 0, deltaZ);
		movement = Quaternion.AngleAxis(cam.transform.rotation.eulerAngles.y, Vector3.up) * movement;

		// make diagonal movement consistent
		movement = Vector3.ClampMagnitude(movement, speed);

		// add gravity in the vertical direction
		movement.y = gravity;

		// ensure movement is independent of the framerate
		movement *= Time.deltaTime;

		// transform from local space to global space
		movement = transform.TransformDirection(movement);

		// pass the movement to the character controller
		charController.Move(movement);
	}
}
