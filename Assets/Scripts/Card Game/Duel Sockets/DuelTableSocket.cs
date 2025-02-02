using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class DuelTableSocket : DuelSocket
{
	[SerializeField] bool isFrontRow;

	public bool GetIsFrontRow()
	{
		return isFrontRow;
	}
}