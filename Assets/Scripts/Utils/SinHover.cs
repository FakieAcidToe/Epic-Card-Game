using UnityEngine;

public class SinHover : MonoBehaviour
{
	[SerializeField] float amplitude;
	[SerializeField] float frequency;

	Vector3 originalTransform;

	void Start()
	{
		originalTransform = transform.localPosition;
	}

	void Update()
	{
		transform.localPosition = originalTransform + amplitude * Mathf.Sin(2 * Mathf.PI * frequency * Time.time) * transform.up;
	}
}