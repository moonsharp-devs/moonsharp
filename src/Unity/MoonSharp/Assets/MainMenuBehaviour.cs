using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenuBehaviour : MonoBehaviour {

    void OnGUI()
    {
        string text = "";

        string banner = "MoonSharp Test Runner Menu";

        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), banner);

        int X = (Screen.width - 200) / 2;

        if (GUI.Button(new Rect(X, 30, 200, 40), "Unit Tests"))
            SceneManager.LoadScene("UnitTestsRunner");

		if (GUI.Button(new Rect(X, 80, 200, 40), "VsCode Debugger Test"))
			SceneManager.LoadScene("DebuggerTest");

		if (GUI.Button(new Rect(X, 130, 200, 40), "Manual Test #1"))
			SceneManager.LoadScene("CustomTest1");

	}

}
