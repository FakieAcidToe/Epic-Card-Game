using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Events;

public class TrackedManager : MonoBehaviour
{
	List<InputDevice> devices = new List<InputDevice>();
	[SerializeField] private XRNode controllerNode;
	[SerializeField] private MeshRenderer ball;
	public UnityEvent OnPress;
	public UnityEvent OnRelease;
	private bool isPressed = false;

	public void GetDevice()
	{
		InputDevices.GetDevicesAtXRNode(controllerNode, devices);
	}

	public void ChangeBallColor()
	{
		ball.material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f),
		Random.Range(0f, 1f), 1.0f);
	}

	void Start()
	{
		GetDevice();
	}

	void Update()
	{
        foreach (InputDevice device in devices)
		{
			Debug.Log(device.name + "" + device.characteristics);
			if (device.isValid)
			{
				Vector3 position;
				if (device.TryGetFeatureValue(CommonUsages.devicePosition, out position))
				{
					Debug.Log(device.name + "" + position);
				}
				Quaternion rotation;
				if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out rotation))
				{
					Debug.Log(device.name + "" + rotation);
				}
				bool inputValue;
				if (device.TryGetFeatureValue(CommonUsages.primaryButton, out inputValue))
				{
					if (inputValue)
					{
						if (!isPressed)
						{
							isPressed = true;
							Debug.Log("On Press");
							OnPress.Invoke();
						}
					}
					else if (isPressed)
					{
						isPressed = false;
						Debug.Log("On Release");
						OnRelease.Invoke();
					}
				}
			}
		}
	}
}
