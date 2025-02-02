using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class DuelTableSocket : DuelSocket
{
	[SerializeField] bool isFrontRow;
	[SerializeField] ParticleSystem particles;
	[SerializeField] SpriteParticlePlayer spriteParticles;

	public bool GetIsFrontRow()
	{
		return isFrontRow;
	}

	public void ParticleBurst()
	{
		particles.Play();
	}

	public void PlaySpriteParticle(SpriteParticle _spriteParticle)
	{
		spriteParticles.Play(_spriteParticle);
	}

	public void PlaySpriteParticle()
	{
		spriteParticles.Play();
	}
}