using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class DuelTableSocket : DuelSocket
{
	[SerializeField] bool isFrontRow;
	[SerializeField] ParticleSystem particles;
	[SerializeField] Gradient particleColourGradient;
	[SerializeField] SpriteParticlePlayer spriteParticles;

	public bool GetIsFrontRow()
	{
		return isFrontRow;
	}

	public void ParticleBurst()
	{
		ParticleSystem.ColorOverLifetimeModule col = particles.colorOverLifetime;
		col.color = particleColourGradient;

		particles.Play();
	}

	public void ParticleBurst(Gradient _particleColourGradient)
	{
		particleColourGradient = _particleColourGradient;
		ParticleBurst();
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