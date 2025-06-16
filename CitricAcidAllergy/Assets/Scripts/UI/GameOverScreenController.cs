using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreenController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _contributionPointText;

    [SerializeField]
    private TMP_Text _wonText;

    private void Start()
    {
        var points = PlayerPrefs.GetFloat("ContributionValue");
        
        if (PlayerPrefs.GetInt("Won") > 0)
        {
            _wonText.gameObject.SetActive(true);
        }
        _contributionPointText.text = points.ToString();
    }

    public void OnClickRestart()
    {
        SceneManager.LoadScene("StartMenu");
    }
}
