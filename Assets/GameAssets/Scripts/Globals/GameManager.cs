using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour 
{
    public const int SCENEINDEX_GAMEMANAGER = 0;
    public const int SCENEINDEX_MAINMENU    = 1;
    public const int SCENEINDEX_PAUSEMENU   = 2;
    public const int SCENEINDEX_LEVEL1      = 3;


    public const string KEYHASH_SAVEDATA    = "GameManager.KEYHASH_SAVEDATA";



    static GameManager _instance;
    public static GameManager Instance
    {
        get { return _instance; }
    }




    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(_nameFirstScene);
    }

    public void LoadSavedGame()
    {
        string json = PlayerPrefs.GetString(GameManager.KEYHASH_SAVEDATA);
        _fromLoadingState = JsonUtility.FromJson<FPlayerSaveState>(json);


        SceneManager.LoadScene(_fromLoadingState._sceneIndex);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == GameManager.SCENEINDEX_GAMEMANAGER) return;

        if(scene.buildIndex >= GameManager.SCENEINDEX_LEVEL1)
        {
            PlayerReference = Instantiate(_playerPrefab);

            var virtualCamera = FindObjectOfType<Cinemachine.CinemachineFreeLook>();
            virtualCamera.Follow = PlayerReference.transform.Find("Hips/Spine/Spine1/Spine2/WeaponDirection");
            virtualCamera.LookAt = PlayerReference.transform.Find("Hips/Spine/Spine1/Spine2/WeaponDirection");

			PlayerReference.Initialize(virtualCamera.gameObject);

            if(_fromLoadingState != null)
            {
                PlayerReference.ApplySaveState(_fromLoadingState);
                _fromLoadingState = null;
            }
            else
            {
                var playerStart = FindObjectOfType<PlayerStart>();
                PlayerReference.transform.position = playerStart.transform.position;
                PlayerReference.transform.rotation = playerStart.transform.rotation;


            }

        }
    }

    public Player PlayerReference { get; private set; }


    [SerializeField] string _nameFirstScene;
    [SerializeField] Player _playerPrefab;
    FPlayerSaveState _fromLoadingState;

}
