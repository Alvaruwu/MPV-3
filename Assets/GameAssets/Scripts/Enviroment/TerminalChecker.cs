using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TerminalChecker : MonoBehaviour {

	[SerializeField] PlayableDirector _director;
	[SerializeField] string _requiredKey;

	bool _bActivated;


	void OnTriggerStay(Collider other)
	{
		bool bCanOpen = !_bActivated
						& other.transform.root.CompareTag("Player")
						& Input.GetButtonDown("Interact")
						& GameManager.Instance.PlayerReference._inventory.HasItem(_requiredKey);

		if (bCanOpen)
		{
			_director.Play();
			_bActivated = true;
		}
	}
}