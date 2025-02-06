using UnityEngine;

[System.Serializable]
public class BodyPartMap
{
	public Transform XrTarget;
	public Transform IkTarget;
	public Vector3 trackingPositionOffset;
	public Vector3 trackingRotationOffset;
	public void Map()
	{
		IkTarget.position = XrTarget.TransformPoint(trackingPositionOffset);
		IkTarget.rotation = XrTarget.rotation *
		Quaternion.Euler(trackingRotationOffset);
	}
}

public class MapIKTargetToXRRig : MonoBehaviour
{
	[Range(0, 1)]
	[SerializeField] private float turnSmoothness = 0.1f;
	[SerializeField] private BodyPartMap head;
	[SerializeField] private BodyPartMap leftHand;
	[SerializeField] private BodyPartMap rightHand;
	[SerializeField] private Vector3 headBodyPositionOffset;
	[SerializeField] private float headBodyYawOffset;
	void LateUpdate()
	{
		transform.position = head.IkTarget.position + headBodyPositionOffset;
		float yaw = head.XrTarget.eulerAngles.y;
		transform.rotation = Quaternion.Lerp(transform.rotation,
		Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z),
		turnSmoothness);
		head.Map();
		leftHand.Map();
		rightHand.Map();
	}
}