using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

	void OnTriggerEnter(Collider other)
	{
		if(other.transform.root.CompareTag("Player"))
		{
			GameManager.Instance.PlayerReference._inventory.Add(gameObject.name);
			Destroy(gameObject);
		}
	}
}
