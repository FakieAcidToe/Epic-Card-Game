using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TitleScreenManager : MonoBehaviour
{
	[SerializeField] GameObject hintArrow;
	[SerializeField] Transform cardLocation;
	[SerializeField] Transform playLocation;

	[SerializeField] XRGrabInteractable card;

	IEnumerator arrowCoroutine;

	void Start()
	{
		card.selectEntered.AddListener(OnCardPickup);
		card.selectExited.AddListener(OnCardDrop);

		arrowCoroutine = EnableArrowAtLocationInSomeTime(2, cardLocation);
		StartCoroutine(arrowCoroutine);
	}

	void OnDestroy()
	{
		card.selectEntered.RemoveListener(OnCardPickup);
		card.selectExited.RemoveListener(OnCardDrop);
	}

	void OnCardDrop(SelectExitEventArgs arg0)
	{
		if (arg0.interactorObject is not NearFarInteractor) return;

		if (arrowCoroutine != null) StopCoroutine(arrowCoroutine);
		arrowCoroutine = EnableArrowAtLocationInSomeTime(2, cardLocation);
		StartCoroutine(arrowCoroutine);
	}

	void OnCardPickup(SelectEnterEventArgs arg0)
	{
		if (arg0.interactorObject is not NearFarInteractor) return;

		if (arrowCoroutine != null) StopCoroutine(arrowCoroutine);
		arrowCoroutine = EnableArrowAtLocationInSomeTime(2, playLocation);
		StartCoroutine(arrowCoroutine);
	}

	IEnumerator EnableArrowAtLocationInSomeTime(float _timeSecs, Transform _transform)
	{
		hintArrow.SetActive(false);
		yield return new WaitForSeconds(_timeSecs);
		hintArrow.transform.SetPositionAndRotation(_transform.position, _transform.rotation);
		hintArrow.SetActive(true);
	}
}