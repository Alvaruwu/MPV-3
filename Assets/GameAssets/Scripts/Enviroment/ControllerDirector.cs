using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;

public class ControllerDirector : MonoBehaviour {

	public enum EModes
	{
		Awake,
		Trigger,
		Function
	}

	[SerializeField] EModes _modes;
	[SerializeField] bool _blockPlayer;
	[SerializeField] PlayableDirector _director;
	[SerializeField] UnityAction _onComplete;

	float _leftTimeDirector = -1;

	void Start()
	{
		if(_modes == EModes.Awake)
		{
			Play();
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if(_modes == EModes.Trigger)
		{
			Play();
		}
	}

	public void Play()
	{
		if(_blockPlayer)
		{
			GameManager.Instance.PlayerReference.enabled = false;
		}
		_director.Play();

		_leftTimeDirector = (float)_director.duration;
	}

	void Update()
	{
		if (_leftTimeDirector >= 0)
		{
			_leftTimeDirector -= Time.deltaTime;
			if (_leftTimeDirector <= 0)
			{
				OnComplete();
				_leftTimeDirector = -1;
			}
		}
	}

	void OnComplete()
	{
		if(_blockPlayer)
		{
			GameManager.Instance.PlayerReference.enabled = true;
		}

		_onComplete.Invoke();
	}
}