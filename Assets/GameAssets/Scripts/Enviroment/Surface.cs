using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surface : MonoBehaviour {

	[SerializeField] ParticleSystem hitParticles;
	[SerializeField] AudioClip hitAudio;

	public void CreateParticles(Vector3 point, Vector3 normal)
	{
		Instantiate(hitParticles, point, Quaternion.LookRotation(normal));

		if(hitAudio != null)
		{
			AudioSource.PlayClipAtPoint(hitAudio, point);
		}
	}
}
