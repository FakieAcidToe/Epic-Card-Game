using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ButtonFollowVisual : MonoBehaviour
{
	public Transform visualTarget;
	public Vector3 localAxis;
	public float resetSpeed = 5;
	public float followAngleThreshold = 45;

	public float buttonPressAmount = 0.03f;

	public UnityEvent onPress;
	public UnityEvent onRelease;

	bool freeze = false;

	Vector3 initialLocalPos;

	Vector3 offset;
	Transform pokeAttachTransform;

	XRBaseInteractable interactable;
	bool isFollowing = false;

	void Awake()
	{
		initialLocalPos = visualTarget.localPosition;

		interactable = GetComponent<XRBaseInteractable>();
		interactable.hoverEntered.AddListener(Follow);
		interactable.hoverExited.AddListener(Reset);
		interactable.selectEntered.AddListener(PressButton); // on grab
		interactable.selectExited.AddListener(UnPressButton); // on grab release
	}

	void OnDestroy()
	{
		interactable.hoverEntered.RemoveListener(Follow);
		interactable.hoverExited.RemoveListener(Reset);
		interactable.selectEntered.RemoveListener(PressButton);
		interactable.selectExited.RemoveListener(UnPressButton);
	}

	void Follow(BaseInteractionEventArgs hover)
	{
		if (hover.interactorObject is XRPokeInteractor)
		{
			XRPokeInteractor interactor = (XRPokeInteractor)hover.interactorObject;

			pokeAttachTransform = interactor.attachTransform;
			offset = visualTarget.position - pokeAttachTransform.position;

			if (Vector3.Angle(offset, visualTarget.TransformDirection(localAxis)) < followAngleThreshold)
			{
				isFollowing = true;
				freeze = false;
			}
		}
	}

	void Reset(BaseInteractionEventArgs hover)
	{
		if (hover.interactorObject is XRPokeInteractor)
		{
			if (freeze)
			{
				freeze = false;
				onRelease.Invoke();
			}
			isFollowing = false;
		}
	}

	public void ForcePressButton()
	{
		PressButton(null);
		UnPressButton(null);
	}

	public void PressButton(SelectEnterEventArgs arg)
	{
		onPress.Invoke();
		visualTarget.localPosition = new Vector3(visualTarget.localPosition.x, initialLocalPos.y - buttonPressAmount, visualTarget.localPosition.z);
		freeze = true;
		isFollowing = false;
	}

	public void UnPressButton(SelectExitEventArgs arg)
	{
		if (freeze)
		{
			freeze = false;
			onRelease.Invoke();
		}
		isFollowing = false;
	}

	void Update()
	{
		if (freeze) return;

		if (isFollowing)
		{
			Vector3 localTargetPosition = visualTarget.InverseTransformPoint(pokeAttachTransform.position + offset);
			Vector3 constrainedLocalTargetPosition = Vector3.Project(localTargetPosition, localAxis);

			visualTarget.position = visualTarget.TransformPoint(constrainedLocalTargetPosition);

			if (initialLocalPos.y < visualTarget.localPosition.y)
			{
				isFollowing = false;
				visualTarget.localPosition = new Vector3(visualTarget.localPosition.x, initialLocalPos.y, visualTarget.localPosition.z);
			}
			else if (initialLocalPos.y - visualTarget.localPosition.y >= buttonPressAmount)
			{
				freeze = true;
				visualTarget.localPosition = new Vector3(visualTarget.localPosition.x, initialLocalPos.y - buttonPressAmount, visualTarget.localPosition.z);
				onPress.Invoke();
			}
		}
		else
		{
			visualTarget.localPosition = Vector3.Lerp(visualTarget.localPosition, initialLocalPos, Time.deltaTime * resetSpeed);
		}
	}
}