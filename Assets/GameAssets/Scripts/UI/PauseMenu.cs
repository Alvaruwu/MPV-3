using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] Button[] _buttons;


    enum EButtonReason
    {
        EBR_ContinueGame,
        EBR_SaveGame,
        EBR_ReturnToMenu,
        EBR_ExitGame
    }

    public static bool IsOpen { get; private set; }


    void Awake()
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            //_buttons[i].onClick.AddListener(() => OnButtonPressed((EButtonReason)i));
            //_buttons[i].onClick.AddListener(() => OnButtonPressed_Internal(i));
            //printFor(i);

        }

        _buttons[0].onClick.AddListener(() => OnButtonPressed(EButtonReason.EBR_ContinueGame));
        _buttons[1].onClick.AddListener(() => OnButtonPressed(EButtonReason.EBR_SaveGame));
        _buttons[2].onClick.AddListener(() => OnButtonPressed(EButtonReason.EBR_ReturnToMenu));
        _buttons[3].onClick.AddListener(() => OnButtonPressed(EButtonReason.EBR_ExitGame));
    }

    public static void ShowMenu()
    {
        if (!IsOpen)
        {
            GameManager.Instance.PlayerReference.SetPause(true);

            IsOpen = true;
            SceneManager.LoadScene(GameManager.SCENEINDEX_PAUSEMENU, LoadSceneMode.Additive);
        }
    }

    public static void CloseMenu()
    {
        if (IsOpen)
        {
            GameManager.Instance.PlayerReference.SetPause(false);

            IsOpen = false;
            SceneManager.UnloadSceneAsync(GameManager.SCENEINDEX_PAUSEMENU);
        }
    }

    void OnButtonPressed(EButtonReason reason)
    {
        print("OnButtonPressed: " + reason);

        switch (reason)
        {
            case EButtonReason.EBR_ContinueGame:
                CloseMenu();
                break;

            case EButtonReason.EBR_SaveGame:
                FPlayerSaveState state = GameManager.Instance.PlayerReference.GetSaveState();
                string json = JsonUtility.ToJson(state);
                PlayerPrefs.SetString(GameManager.KEYHASH_SAVEDATA, json);
                print(json);

                CloseMenu();
                break;

            case EButtonReason.EBR_ReturnToMenu:
                SceneManager.LoadScene(GameManager.SCENEINDEX_MAINMENU);
                break;

            case EButtonReason.EBR_ExitGame:
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else   
                    Application.Quit();
                #endif


                break;
        }
    }
}
