using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class GameData
{
	public int[] deck;

	public GameData(int[] _deck)
	{
		deck = _deck;
	}
}

public class SaveFileSystem : MonoBehaviour
{
	public static SaveFileSystem Instance;

	[SerializeField] List<CardsScriptableObj> cardDex;
	[SerializeField] List<CardsScriptableObj> starterDeck;

	public List<CardsScriptableObj> deck;

	void Awake()
	{
		// Check if an instance already exists
		if (Instance != null && Instance != this)
		{
			// Destroy the new instance if one already exists
			Destroy(gameObject);
		}
		else
		{
			// Set the instance to this object and ensure it persists across scene loads
			Instance = this;
			DontDestroyOnLoad(gameObject);

			LoadFile();
		}
	}

	public void SaveFile()
	{
		string destination = Application.persistentDataPath + "/epicCardSave.dat";
		FileStream file;

		if (File.Exists(destination)) file = File.OpenWrite(destination);
		else file = File.Create(destination);

		GameData data = new GameData(CardsToInt(deck));
		BinaryFormatter bf = new BinaryFormatter();
		bf.Serialize(file, data);
		file.Close();
	}

	public void LoadFile()
	{
		string destination = Application.persistentDataPath + "/epicCardSave.dat";
		FileStream file;

		if (File.Exists(destination))
		{
			file = File.OpenRead(destination);

			BinaryFormatter bf = new BinaryFormatter();
			GameData data = (GameData)bf.Deserialize(file);
			file.Close();

			deck = IntToCards(data.deck);

			Debug.Log(data.deck);
		}
		else
		{
			deck = new List<CardsScriptableObj>(starterDeck);

			Debug.Log("File not found");
		}
	}

	int[] CardsToInt(List<CardsScriptableObj> _deckList)
	{
		int[] deckInt = new int[_deckList.Count];

		for (int i = 0; i < deckInt.Length; ++i)
		{
			deckInt[i] = cardDex.IndexOf(_deckList[i]);
		}

		return deckInt;
	}

	List<CardsScriptableObj> IntToCards(int[] _deckInt)
	{
		List<CardsScriptableObj> deckList = new List<CardsScriptableObj>();

		foreach (int i in _deckInt)
		{
			if (i < cardDex.Count)
				deckList.Add(cardDex[i]);
		}

		return deckList;
	}

	public List<CardsScriptableObj> GetCardDex()
	{
		return cardDex;
	}

	public List<CardsScriptableObj> GetNewStarterDeck()
	{
		return new List<CardsScriptableObj>(starterDeck);
	}
}