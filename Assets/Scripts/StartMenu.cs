using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    public Button whiteButton;
    public Button blackButton;
    public Button startButton;

    private PieceColor selectedColor;

    void Start()
    {
        whiteButton.onClick.AddListener(() => SelectColor(PieceColor.WHITE));
        blackButton.onClick.AddListener(() => SelectColor(PieceColor.BLACK));
        startButton.onClick.AddListener(StartGame);
        startButton.interactable = false;
    }

    private void SelectColor(PieceColor color)
    {
        selectedColor = color;
        startButton.interactable = true;
    }

    private void StartGame()
    {
        PlayerPrefs.SetInt("SelectedColor", (int)selectedColor);
        SceneManager.LoadScene("GameScene");
    }
}
