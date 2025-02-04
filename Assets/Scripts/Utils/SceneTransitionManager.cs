using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
	public FadeScreen fadeScreen;

	void Start()
	{
		StartScene();
	}

	void StartScene()
	{
		StartCoroutine(StartSceneRoutine());
	}

	IEnumerator StartSceneRoutine()
	{
		fadeScreen.gameObject.SetActive(true);
		yield return StartCoroutine(fadeScreen.FadeInRoutine());

		// set FadeScreen to inactive, to prevent blocking ray interactors and UI canvas
		fadeScreen.gameObject.SetActive(false);
	}

	public void GoToScene(int sceneIndex)
	{
		StartCoroutine(GoToSceneRoutine(sceneIndex));
	}

	IEnumerator GoToSceneRoutine(int sceneIndex)
	{
		fadeScreen.gameObject.SetActive(true);
		yield return StartCoroutine(fadeScreen.FadeOutRoutine());

		SceneManager.LoadScene(sceneIndex);
	}
}