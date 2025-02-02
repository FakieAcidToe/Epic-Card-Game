using UnityEngine;
using UnityEngine.AI;

public class MoveTo : MonoBehaviour
{
	public Transform goal;
	NavMeshAgent agent;

	[SerializeField] string rayLayerName = "Ground";

	void Start()
	{
		agent = GetComponent<NavMeshAgent>();
	}

	void Update()
	{
		if (Input.GetMouseButton(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit mouseHit;
			if (Physics.Raycast(ray, out mouseHit))
			{
				GameObject hitObject = mouseHit.transform.gameObject;
				if (hitObject.layer == LayerMask.NameToLayer(rayLayerName))
				{
					goal.position = mouseHit.point;
					agent.destination = goal.position;
				}
			}
		}
	}
	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2);
	}
}
