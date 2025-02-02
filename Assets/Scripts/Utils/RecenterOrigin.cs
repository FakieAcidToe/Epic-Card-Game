using UnityEngine;
using Unity.XR.CoreUtils;

public class RecenterOrigin : MonoBehaviour
{
	[SerializeField] bool alsoRecenterOnStart = true;
	[SerializeField] Transform target;

	public void Recenter()
	{
		XROrigin xrOrigin = GetComponent<XROrigin>();
		xrOrigin.MoveCameraToWorldLocation(target.position);
		xrOrigin.MatchOriginUpCameraForward(target.up, target.forward);
	}

	void Start()
	{
		if (alsoRecenterOnStart)
			Recenter();
	}
}