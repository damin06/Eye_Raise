using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField m_IpAdressInput;
    [SerializeField] private TMP_InputField m_nameInput;

    public void OnConnectedToServerWithButton()
    {
        ConnectedToServer(m_nameInput.text);
        
    }

    public void ConnectedToServer(string username)
    {
        UserData userData = new UserData
        {
            username = username,
            color = Random.ColorHSV()
        };

        ClientSingleton.Instance.StartClient(userData);
    }
}
