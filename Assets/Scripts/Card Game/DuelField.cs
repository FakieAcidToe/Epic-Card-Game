using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DuelField : MonoBehaviour
{
	public bool isPlayer;
	public DuelDisk duelDisk;
	public DrawCard deck;
	public ButtonFollowVisual button;
	public TextMeshProUGUI lifeText;
	public List<DuelTableSocket> frontTableSockets;
	public List<DuelTableSocket> backTableSockets;
	public DuelDiscardSocket discardSocket;

	[SerializeField] uint lifePoints = 20;
	[SerializeField] uint mana = 4;

	[SerializeField] string lifePointsPrependText = "Life: ";
	[SerializeField] string manaPrependText = "Mana: ";

	int playerNumber;

	void OnValidate()
	{
		UpdateLifePointText();
	}

	public void DamagePlayer(int _damage)
	{
		lifePoints = lifePoints > _damage ? (uint)(lifePoints - _damage) : 0;
		UpdateLifePointText();
	}

	public bool ConsumeMana(int _amount)
	{
		if (_amount <= mana)
		{
			mana = (uint)(mana - _amount);
			UpdateLifePointText();

			return true;
		}
		return false;
	}

	public void InitLifeMana(uint _life, uint _mana)
	{
		lifePoints = _life;
		mana = _mana;

		UpdateLifePointText();
	}

	public void UpdateLifePointText()
	{
		lifeText.text = lifePointsPrependText + lifePoints.ToString() + '\n' + manaPrependText + mana.ToString();
	}

	public uint GetLife()
	{
		return lifePoints;
	}

	public uint GetMana()
	{
		return mana;
	}

	public void SetPlayerNumber(int _playerNumber)
	{
		playerNumber = _playerNumber;
		duelDisk.SetPlayerNumber(playerNumber);
		deck.SetPlayerNumber(playerNumber);
	}

	public int GetPlayerNumber()
	{
		return playerNumber;
	}
}