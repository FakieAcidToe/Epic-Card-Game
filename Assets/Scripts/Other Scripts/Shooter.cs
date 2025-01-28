using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
	private bool isDrawingRay = false;
	public float impulseStrength = 5.0f;

	public GameObject bulletPrefab;
	public float bulletImpulse = 5.0f;

	public GameObject particleSysPrefab;

	private void Update()
	{
		if (isDrawingRay)
		{
			Debug.DrawRay(transform.position, transform.forward, Color.red);
		}
	}

	private IEnumerator InstantiateBullet()
	{
		GameObject bullet = Instantiate(bulletPrefab, transform.position + transform.forward * 2, transform.rotation);
		Rigidbody target = bullet.GetComponent<Rigidbody>();
		Vector3 impulse = transform.forward * bulletImpulse;
		target.AddForceAtPosition(impulse, transform.position, ForceMode.Impulse);
		yield return new WaitForSeconds(5);
		Destroy(bullet);
	}

	private IEnumerator GeneratePS(RaycastHit hit)
	{
		GameObject ps = Instantiate(particleSysPrefab, hit.point,
		Quaternion.LookRotation(hit.normal));
		yield return new WaitForSeconds(1);
		Destroy(ps);
	}

	public void Shoot()
	{
		StartCoroutine(InstantiateBullet());
		
		// create a ray from the point in the forward direction of the GameObject
		Ray ray = new Ray(transform.position, transform.forward);

		RaycastHit hit; // stores ray intersection information

		// ray cast will obtain hit information if it intersects anything
		if (Physics.Raycast(ray, out hit))
		{
			// get the GameObject that was hit
			GameObject hitObject = hit.transform.gameObject;

			// check if the object has rigidbody
			Rigidbody rb = hitObject.GetComponent<Rigidbody>();
			if (rb != null)
			{
				Vector3 impulse = Vector3.Normalize(hit.point - transform.position) * impulseStrength;
				rb.AddForceAtPosition(impulse, hit.point, ForceMode.Impulse);
				
				StartCoroutine(GeneratePS(hit));
			}
		}
	}

	public void DrawRay(bool isEnabled)
	{
		isDrawingRay = isEnabled;
	}
}
