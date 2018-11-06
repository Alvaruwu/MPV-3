using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

[System.Serializable]
public class FPlayerSaveState
{
    public float x, y, z;
    public int _sceneIndex;
    public int _currentHealth;

    public FPlayerSaveState(Vector3 position, int currentHealth, int sceneIndex)
    {
        x = position.x;
        y = position.y;
        z = position.z;
        _currentHealth = currentHealth;
        _sceneIndex = sceneIndex;
    }
}

public class Inventory
{
	List<string> lElements = new List<string>();

	public void Add(string newElement)
	{
		if (!lElements.Contains(newElement))
		{
			lElements.Add(newElement);
		}
	}

	public void Remove(string removedElement)
	{
		lElements.Remove(removedElement);
	}

	public bool HasItem(string element)
	{
		return lElements.Contains(element);
	}
}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Health))]
public class Player : BaseCharacter 
{
    [Header("Movement")]
    [SerializeField] float _velocityWalkMovement;
    [SerializeField] float _velocityRunMovement;
    [SerializeField] float _aimingVelocity;

    [Header("Weapons")]
    [SerializeField] Transform _weaponDirection;
    [SerializeField] GameObject _genericImpact;
    [SerializeField] ParticleSystem _muzzleParticle;
    [SerializeField] AudioClip _shotAudio;
    [SerializeField] float _rateAttack;
    [SerializeField] float _distanceIK;

	[Header("LookAt")]
	[SerializeField] Cinemachine.CinemachineFreeLook virtualCamera;
    [SerializeField] Transform _headJoint;
    [SerializeField] float _headOrientationVelocity = 30;
    [SerializeField] float _maxAngleLookAt = 60;
    [SerializeField] float _maxDistanceToLookAt = 10;

    [Header("Dead Values")]
    [SerializeField] Canvas _deadCanvas;
    [SerializeField] AudioClip _deadSound;
    [SerializeField] float _waitStepsDead;


	GameObject _playerCamera;

	Animator _animator;
    CharacterController _controller;
	public Inventory _inventory { get; private set; }

    Collider[] _lookables = new Collider[5];
    Vector3 _currentHeadJointDirection;
    Vector3 weaponIKPosition;
    float _currentLayerAimWeight;
    bool bAimingActive;
    bool bIsRunning;
    float _nextCanShoot;


    public FPlayerSaveState GetSaveState()
    {
        int sceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        Vector3 position = transform.position;


        return new FPlayerSaveState(position, HealthReference.GetCurrentLife(), sceneIndex);
    }

    public void ApplySaveState(FPlayerSaveState saveState)
    {
        transform.position = new Vector3(saveState.x, saveState.y, saveState.z);
        HealthReference.SetCurrentLife(saveState._currentHealth);
    }


    public void Initialize() 
    {
        base.Awake();

		virtualCamera.Follow = this.transform.Find("Hips/Spine/Spine1/Spine2/WeaponDirection");
		virtualCamera.LookAt = this.transform.Find("Hips/Spine/Spine1/Spine2/WeaponDirection");

		_playerCamera = virtualCamera.gameObject;
        _animator   = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
		_inventory = new Inventory();
	}

    protected override void Start()
    {
        _currentHeadJointDirection = transform.forward;
    }

    public void SetPause(bool bIsPaused)
    {
        Cursor.visible = bIsPaused;
        Cursor.lockState = bIsPaused ? CursorLockMode.Confined : CursorLockMode.Locked;
        _playerCamera.SetActive(!bIsPaused);
    }

    protected override void Update () 
    {
        if(HealthReference.IsDead)
        {
            return;
        }

        if(Input.GetButtonDown("Inventory"))
        {
            if(PauseMenu.IsOpen)
            {
                PauseMenu.CloseMenu();
            }
            else
            {
                PauseMenu.ShowMenu();    
            }
        }



        if (PauseMenu.IsOpen) return;

        //=========================================
        if(Input.GetKeyDown(KeyCode.Space))
        {
            print("Aplicando bomba nuclear >:)");
            HealthReference.Damage(int.MaxValue);    
        }
        //=========================================

        bAimingActive = Input.GetMouseButton(1);
        bIsRunning = Input.GetKey(KeyCode.LeftShift);

        if(Input.GetMouseButtonDown(1))
        {
            _nextCanShoot = Time.time + 1 / _aimingVelocity;

            weaponIKPosition = _weaponDirection.position + transform.forward * _distanceIK;
        }

        //Camera control
        _playerCamera.SetActive(!bAimingActive);

        //Cursor visibility
        Cursor.visible = bAimingActive;
        Cursor.lockState = bAimingActive ? CursorLockMode.Confined : CursorLockMode.Locked;

        //Movement and rotation control
        float verticalAxis = Input.GetAxis("Vertical")      * (bAimingActive ? 0 : 1);
        float horizontalAxis = Input.GetAxis("Horizontal")  * (bAimingActive ? 0 : 1);

        Vector3 forwardXZ = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
        Vector3 rightXZ = Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up);
        Vector3 directionXZ = forwardXZ * verticalAxis
                            + rightXZ * horizontalAxis;

        _controller.SimpleMove(directionXZ * (bIsRunning ? _velocityRunMovement : _velocityWalkMovement));

        Vector3 localDirection = transform.InverseTransformDirection(_controller.velocity);
        _animator.SetFloat("MovX", localDirection.x);
        _animator.SetFloat("MovY", localDirection.z);

        if(Mathf.Abs(verticalAxis) > 0 || Mathf.Abs(horizontalAxis) > 0)
        {
            transform.rotation = Quaternion.LookRotation(forwardXZ);
        }

        UpdateLayerAimWeight();
        UpdateAttack();

        _animator.SetFloat("Velocity", Vector3.ProjectOnPlane(_controller.velocity, Vector3.up).magnitude);
	}

    protected override void LateUpdate()
    {
        if (HealthReference.IsDead)
        {
            return;
        }


        int count = Physics.OverlapSphereNonAlloc(_headJoint.position, _maxDistanceToLookAt, _lookables);
        count = Mathf.Min(_lookables.Length, count);

        float minDistance = float.MaxValue;
        Collider reference = null;
        for (int i = 0; i < count; i++)
        {
            float distance = Vector3.Distance(transform.position, _lookables[i].transform.position);
            if (distance < minDistance)  
            {
                minDistance = distance;
                reference = _lookables[i];
            }
        }

        //=^.^=

        Vector3 direction =  Vector3.Normalize(reference.transform.position - _headJoint.position);
        Vector3 directionXZ = Vector3.ProjectOnPlane(direction, Vector3.up);

        float angle = Vector3.Angle(directionXZ, transform.forward);

        _headJoint.rotation = Quaternion.LookRotation(_currentHeadJointDirection);

        _currentHeadJointDirection = Vector3.RotateTowards(
            _currentHeadJointDirection,
            angle <= _maxAngleLookAt ? direction : transform.forward,
            _headOrientationVelocity * Mathf.Deg2Rad * Time.deltaTime,
            1
        );
    }

    protected override void OnAnimatorIK(int layerIndex)
    {
        if(!HealthReference.IsDead && layerIndex == 1)
        {
            _animator.SetIKPosition(AvatarIKGoal.LeftHand, weaponIKPosition);
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, _currentLayerAimWeight);

            _animator.SetIKPosition(AvatarIKGoal.RightHand, weaponIKPosition);
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, _currentLayerAimWeight);
        }
    }




    void UpdateLayerAimWeight()
    {
        _currentLayerAimWeight += Time.deltaTime * (bAimingActive ? 1 : -1) * _aimingVelocity;
        _currentLayerAimWeight = Mathf.Clamp(_currentLayerAimWeight, 0, 1);


        _animator.SetLayerWeight(1, _currentLayerAimWeight);
    }

    void UpdateAttack()
    {

        _animator.SetBool("bShooting", false);
        bool bCanAttack = Input.GetButton("Fire1") && bAimingActive && Time.time > _nextCanShoot;
        if (!bCanAttack) return;

        _nextCanShoot = Time.time + _rateAttack;


        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray.origin, ray.direction, out hit, float.MaxValue))
        {
            Vector3 directionFromPlayer = (hit.point - _weaponDirection.position).normalized;
            if (Physics.Raycast(_weaponDirection.position, directionFromPlayer, out hit, float.MaxValue))
            {
                Surface hitSurface = hit.collider.GetComponent<Surface>();
                if (hitSurface != null)
                {
                    hitSurface.CreateParticles(hit.point, hit.normal);
                }
                else
                {
                    Instantiate(_genericImpact, hit.point, Quaternion.LookRotation(hit.normal));
                }

                Vector3 impactDirection = (hit.point - transform.position).normalized;
                impactDirection = Vector3.ProjectOnPlane(impactDirection, Vector3.up);
                transform.rotation = Quaternion.LookRotation(impactDirection);


                Vector3 weaponIKDirection = (hit.point - _weaponDirection.position).normalized;
                weaponIKDirection *= _distanceIK;

                weaponIKPosition = _weaponDirection.position + weaponIKDirection;

                Health health = hit.collider.GetComponent<Health>();
                if (health)
                {
                    //TODO: Damage
                    print("Dealing damage");
                }
            }
        }


        _animator.SetBool("bShooting", true);
        _muzzleParticle.Play();
        AudioSource.PlayClipAtPoint(_shotAudio, _muzzleParticle.transform.position);
    }


    protected override void OnDead()
    {
        bAimingActive = false;
        bIsRunning = false;

        StartCoroutine(OnDead_Corrutine());
    }


    IEnumerator OnDead_Corrutine()
    {
        var wait = new WaitForSeconds(_waitStepsDead);

        _animator.SetTrigger("Kill");
        yield return wait;

        _deadCanvas.gameObject.SetActive(true);
        AudioSource.PlayClipAtPoint(_deadSound, transform.position);
        yield return wait;

        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
