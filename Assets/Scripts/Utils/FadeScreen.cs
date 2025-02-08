using System.Collections;
using UnityEngine;

public class FadeScreen : MonoBehaviour
{
	public bool fadeOnStart = true;
	public float fadeDuration = 2;
	public Color fadeColor;
	MeshRenderer rend;

	void Awake()
	{
		rend = GetComponent<MeshRenderer>();
	}

	void Start()
	{
		if (fadeOnStart)
			FadeIn();
	}

	public void FadeIn()
	{
		Fade(1, 0);
	}

	public void FadeOut()
	{
		Fade(0, 1);
	}

	public IEnumerator FadeInRoutine()
	{
		yield return StartCoroutine(FadeRoutine(1, 0));
	}

	public IEnumerator FadeOutRoutine()
	{
		yield return StartCoroutine(FadeRoutine(0, 1));
	}

	public void Fade(float alphaIn, float alphaOut)
	{
		StartCoroutine(FadeRoutine(alphaIn, alphaOut));
	}

	public IEnumerator FadeRoutine(float alphaIn, float alphaOut)
	{
		float timer = 0;
		while (timer <= fadeDuration)
		{
			Color newColor = fadeColor;
			newColor.a = Mathf.Lerp(alphaIn, alphaOut, timer / fadeDuration);

			rend.material.SetColor("_Color", newColor);

			timer += Time.deltaTime;
			yield return null;
		}

		Color newColor2 = fadeColor;
		newColor2.a = alphaOut;
		rend.material.SetColor("_Color", newColor2);
	}
}