using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using static System.Net.Mime.MediaTypeNames;

public class MainMenu : MonoBehaviour
{
    private int selectedSnakeIndex = 0;

    public void SelectPinkSnake()
    {
        selectedSnakeIndex = 0;
        PlayerPrefs.SetInt("SelectedSnake", selectedSnakeIndex);
    }

    public void SelectBlueSnake()
    {
        selectedSnakeIndex = 1;
        PlayerPrefs.SetInt("SelectedSnake", selectedSnakeIndex);
    }

    public void StartGame()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene("snakegame"); 
    }
    public void QuitGame()
    {
        UnityEngine.Application.Quit();
        UnityEngine.Debug.Log("Quit Game");
    }

}
