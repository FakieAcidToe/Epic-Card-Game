using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SpriteParticle", menuName = "ScriptableObjects/Sprite Particle", order = 3)]
public class SpriteParticle : ScriptableObject
{
	public List<Sprite> spriteAnimation;
	public float timeSecondsPerFrame = 0.01f;
}