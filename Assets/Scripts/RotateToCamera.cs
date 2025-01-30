using UnityEngine;

public class RotateToCamera : MonoBehaviour
{
	[SerializeField] float speed = 10.0f;
	Transform mainCameraTransform;

	void Awake()
	{
		mainCameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
	}

	void Update()
	{
		transform.rotation = Quaternion.Lerp(gameObject.transform.rotation,
			Quaternion.LookRotation(transform.position - mainCameraTransform.position),
			speed * Time.deltaTime);
	}
}