using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class AnimationAction
{
	public string animationParameterName;
	public InputAction action;
}
public class InputActionAnimate : MonoBehaviour
{
	public List<AnimationAction> animationActions;
	public Animator animator;
	private void Start()
	{
		foreach (AnimationAction actionItem in animationActions)
		{
			actionItem.action.Enable();
		}
	}
	void Update()
	{
		foreach (AnimationAction actionItem in animationActions)
		{
			float actionValue = actionItem.action.ReadValue<float>();
			animator.SetFloat(actionItem.animationParameterName, actionValue);
		}
	}
}