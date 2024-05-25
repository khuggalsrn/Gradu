using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AndroidInputManager : MonoBehaviour
{
    [SerializeField] GameObject QuitUI;
#if UNITY_ANDROID
    private bool _preparedToQuit = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            if (_preparedToQuit == false)
            {
                AndroidToast.I.ShowToastMessage("뒤로가기 버튼을 한 번 더 누르시면 종료됩니다.");
                PrepareToQuit();
            }
            else
            {
                Debug.Log("Quit");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
            }
        }
    }

    private void PrepareToQuit()
    {
        StartCoroutine(PrepareToQuitRoutine());
    }

    private IEnumerator PrepareToQuitRoutine()
    {
        _preparedToQuit = true;
        yield return new WaitForSecondsRealtime(2.5f);
        _preparedToQuit = false;
    }
#endif
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
