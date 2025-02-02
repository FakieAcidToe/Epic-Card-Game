using UnityEngine;

public class Trigger : MonoBehaviour
{
	[SerializeField] GameObject[] targets;

	void OnTriggerEnter(Collider other)
	{
		foreach (GameObject target in targets)
		{
			target.SendMessage("Activate");
		}
	}
	void OnTriggerExit(Collider other)
	{
		foreach (GameObject target in targets)
		{
			target.SendMessage("Deactivate");
		}
	}
}