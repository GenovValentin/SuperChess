using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Networking.Transport;

public class AccountHandler : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField usernameInputField;

    [SerializeField]
    private TMP_InputField passwordInputField;

    [SerializeField]
    private Button loginButton;

    [SerializeField]
    private GameObject errorMessageTMP;

    [SerializeField]
    private GameObject welcomeMessageTMP;

    [SerializeField]
    private GameObject signInMenu;

    [SerializeField]
    private GameObject signOutMenu;

    private string username;

    private string password;

    private void Start()
    {
        AddUsernameInputFieldListener();
        AddPasswordInputFieldListener();
        ToggleGameObject(errorMessageTMP, false);
        ToggleGameObject(signInMenu, true);
        ToggleGameObject(signOutMenu, false);

        ResetInputFields();
    }

    private void SetUsernameValue(string usernameInputFieldValue)
    {
        username = usernameInputFieldValue;
    }

    private void SetPasswordValue(string passwordInputFieldValue)
    {
        password = passwordInputFieldValue;
    }

    private void AddUsernameInputFieldListener()
    {
        usernameInputField.onEndEdit.AddListener (SetUsernameValue);
    }

    private void AddPasswordInputFieldListener()
    {
        passwordInputField.onEndEdit.AddListener (SetPasswordValue);
    }

    private void SetMessage(GameObject gameObject, string text)
    {
        TMP_Text textComponent = gameObject.GetComponent<TMP_Text>();
        textComponent.text = text;
        ToggleGameObject(gameObject, true);
    }

    private void ToggleGameObject(GameObject gameObject, bool isActive)
    {
        gameObject.SetActive (isActive);
    }

    private void OnSignIn()
    {
        SetMessage(welcomeMessageTMP, "Welcome, " + username);
        ToggleGameObject(errorMessageTMP, false);
        ToggleGameObject(signInMenu, false);
        ToggleGameObject(signOutMenu, true);
    }

    private void ResetInputFields()
    {
        usernameInputField.text = "";
        passwordInputField.text = "";
    }

    public void OnSignUpButton()
    {
        MongoClientWrapper mongoClient = MongoClientWrapper.GetInstance();
        if (mongoClient.IsUsernameTaken(username))
        {
            SetMessage(errorMessageTMP, username + " is already taken!");
            return;
        }
        mongoClient.CreateUser (username, password);
        OnSignIn();
    }

    public void OnSignInButton()
    {
        MongoClientWrapper mongoClient = MongoClientWrapper.GetInstance();
        if (mongoClient.DoesUserExist(username, password) == false)
        {
            SetMessage(errorMessageTMP, "Incorrect username or password!");
            return;
        }
        OnSignIn();
    }

    public void OnSignOutButton()
    {
        ToggleGameObject(signInMenu, true);
        ToggleGameObject(signOutMenu, false);
        ResetInputFields();
    }
}
