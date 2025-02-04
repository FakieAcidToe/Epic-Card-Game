using System.Collections;
using TMPro;
using UnityEngine;

public class DamageNumbers : MonoBehaviour
{
	[SerializeField] TextMeshPro text;
	[SerializeField] float destroyTime = 1.5f;

	void Start()
	{
		StartCoroutine(DestroySelf());
	}

	public void SetDamageNumber(int _damage)
	{
		text.text = _damage.ToString();
	}

	IEnumerator DestroySelf()
	{
		yield return new WaitForSeconds(destroyTime);
		Destroy(gameObject);
	}
}