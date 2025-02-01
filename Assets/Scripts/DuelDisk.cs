using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DuelDisk : MonoBehaviour
{
	[Tooltip("The transform that will be scaled."), SerializeField] Transform scalableModel;
	[Tooltip("The socket prefab that will be spawned."), SerializeField] DuelDiskSocket socketPrefab;
	[Tooltip("The InteractionLayerMask of the spawned socket."), SerializeField] InteractionLayerMask socketLayerMask;
	[Tooltip("The socket prefab's parent on spawn."), SerializeField] Transform socketSpawnParent;

	[SerializeField] float cardWidth = 0.15f;
	[SerializeField] float cardPaddingWidth = 0.02f;

	int playerNumber;

	public List<DuelDiskSocket> sockets;

	public DuelDiskSocket AddSocket()
	{
		DuelDiskSocket newSocket = Instantiate(socketPrefab, socketSpawnParent);
		newSocket.SetDuelDisk(this);
		newSocket.SetInteractionLayerMask(socketLayerMask);
		sockets.Add(newSocket);

		UpdateDuelDisk();

		return newSocket;
	}

	public void UpdateDuelDisk()
	{
		RepositionSockets();
		RescaleModel();
	}

	void RepositionSockets()
	{
		for (int i = 0; i < sockets.Count; ++i)
		{
			if (sockets[i] != null)
			{
				Vector3 newPos = Vector3.zero;
				newPos.x = cardWidth * i - cardWidth / 2 * (sockets.Count-1);
				sockets[i].transform.localPosition = newPos;
			}
		}
	}

	void RescaleModel()
	{
		if (scalableModel != null)
		{
			if (sockets.Count > 0)
			{
				scalableModel.gameObject.SetActive(true);
				Vector3 newScale = scalableModel.localScale;
				newScale.z = cardWidth * sockets.Count + cardPaddingWidth;
				scalableModel.localScale = newScale;
			}
			else
				scalableModel.gameObject.SetActive(false);
		}
	}

	void OnValidate()
	{
		UpdateDuelDisk();
	}

	public void SetPlayerNumber(int _playerNumber)
	{
		playerNumber = _playerNumber;
	}

	public int GetPlayerNumber()
	{
		return playerNumber;
	}

	public List<Card> GetAllCardsInHand()
	{
		List<Card> cards = new List<Card>();
		foreach (DuelSocket socket in sockets)
			cards.Add(socket.GetSocketedCard());
		return cards;
	}
}