using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NotificationPanelController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _title;

    [SerializeField]
    private TMP_Text _body;

    [SerializeField]
    private Button _confirmButton;

    [SerializeField]
    private Button _cancelButton;

    private void AssignOneTimeCallbackToButton(Button button, UnityAction callback)
    {
        button.onClick.AddListener(callback);
        button.onClick.AddListener(() => gameObject.SetActive(false));
        button.onClick.AddListener(button.onClick.RemoveAllListeners);
    }

    private void OnEnable()
    {
        GameManager.Instance.PauseGame();
    }

    private void OnDisable()
    {
        GameManager.Instance.UnpauseGame();
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void ShowMessage(string titleMessage, string bodyMessage, UnityAction confirmCallback = null, UnityAction cancelCallback = null)
    {
        gameObject.SetActive(true);

        _title.text = titleMessage;
        _body.text = bodyMessage;

        if (confirmCallback != null)
        {
            AssignOneTimeCallbackToButton(_confirmButton, confirmCallback);
            _confirmButton.gameObject.SetActive(true);
        }
        else
        {
            _cancelButton.gameObject.SetActive(false);
        }

        if (cancelCallback != null)
        {
            AssignOneTimeCallbackToButton(_cancelButton, cancelCallback);
            _cancelButton.gameObject.SetActive(true);
        }
        else
        {
            _cancelButton.gameObject.SetActive(false);
        }
    }
}
