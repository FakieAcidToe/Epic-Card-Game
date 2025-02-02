using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteParticlePlayer : MonoBehaviour
{
	[SerializeField] SpriteParticle spriteParticle;

	SpriteRenderer spriteRenderer;
	IEnumerator lastCoroutine;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void SetSpriteParticle(SpriteParticle _spriteParticle)
	{
		spriteParticle = _spriteParticle;
	}

	public void Play(SpriteParticle _spriteParticle)
	{
		SetSpriteParticle(_spriteParticle);
		Play();
	}

	public void Play()
	{
		if (lastCoroutine != null) StopCoroutine(lastCoroutine);
		lastCoroutine = PlayAnimation(spriteParticle);

		StartCoroutine(lastCoroutine);
	}

	IEnumerator PlayAnimation(SpriteParticle _spriteParticle)
	{
		for (int i = 0; i < _spriteParticle.spriteAnimation.Count; ++i)
		{
			spriteRenderer.sprite = _spriteParticle.spriteAnimation[i];
			yield return new WaitForSecondsRealtime(_spriteParticle.timeSecondsPerFrame);
		}
		spriteRenderer.sprite = null;
	}
}
