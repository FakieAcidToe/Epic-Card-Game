using UnityEngine;
using Unity.XR.CoreUtils;
using System.Collections;
using Unity.VisualScripting;

public class RecenterOrigin : MonoBehaviour
{
	[SerializeField] bool recenterOnStart = true;
	[SerializeField] Transform target;

	public void Recenter()
	{
		XROrigin xrOrigin = GetComponent<XROrigin>();
		xrOrigin.MoveCameraToWorldLocation(target.position);
		xrOrigin.MatchOriginUpCameraForward(target.up, target.forward);
	}

	void Start()
	{
		if (recenterOnStart)
			StartCoroutine(RecenterNextFrame());
	}

	public IEnumerator RecenterNextFrame()
	{
		yield return new WaitForNextFrameUnit();
		Recenter();
	}
}