using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Networking.Transport;

public class LoginForm : MonoBehaviour
{
    private TMP_InputField usernameInputField;

    private TMP_InputField passwordInputField;

    private GameObject errorMessageTMP;

    private GameObject welcomeMessageTMP;

    private GameObject signInMenu;

    private GameObject signOutMenu;

    private Toggle toggle;

    private string username;

    private string password;

    private string usernameKey = "Username";

    private string passwordKey = "Password";

    private bool isToggleOn = false;

    AccountHandler accountHandler;

    private void Start()
    {
        accountHandler = AccountHandler.GetInstance();
        SetObjects();
        AddUsernameInputFieldListener();
        AddPasswordInputFieldListener();
        AddToggleListener();
        RegisterEvents();

        ToggleGameObject(errorMessageTMP, false);
        ToggleGameObject(signInMenu, true);
        ToggleGameObject(signOutMenu, false);

        ResetToggle();
        ResetInputFields();

        LoadPrefs();
    }

    private void SetObjects()
    {
        usernameInputField =
            GameObject
                .Find("Username_InputField")?
                .GetComponent<TMP_InputField>();
        passwordInputField =
            GameObject
                .Find("Password_InputField")?
                .GetComponent<TMP_InputField>();
        errorMessageTMP = GameObject.Find("Error");
        welcomeMessageTMP = GameObject.Find("Welcome");
        signInMenu = GameObject.Find("Sign In Menu");
        signOutMenu = GameObject.Find("Sign Out Menu");
        toggle = GameObject.Find("Check Box")?.GetComponent<Toggle>();
    }

    private void SetUsernameAndPassword()
    {
        accountHandler.SetUsernameAndPassword (username, password);
    }

    private void SetMessage(GameObject gameObject, string text)
    {
        TMP_Text textComponent = gameObject.GetComponent<TMP_Text>();
        textComponent.text = text;
        ToggleGameObject(gameObject, true);
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

    private void AddToggleListener()
    {
        toggle.onValueChanged.AddListener (ToggleValueChanged);
    }

    private void ToggleGameObject(GameObject gameObject, bool isActive)
    {
        gameObject.SetActive (isActive);
    }

    private void ToggleValueChanged(bool isOn)
    {
        isToggleOn = isOn;
    }

    private void SavePrefs(string username, string password)
    {
        PlayerPrefs.SetString (usernameKey, username);
        PlayerPrefs.SetString (passwordKey, password);
    }

    private void SetPrefs()
    {
        if (isToggleOn == false)
        {
            SavePrefs("", "");
            return;
        }
        SavePrefs (username, password);
    }

    private void LoadPrefs()
    {
        string newUsername = PlayerPrefs.GetString(usernameKey);
        string newPassword = PlayerPrefs.GetString(passwordKey);
        usernameInputField.text = newUsername;
        passwordInputField.text = newPassword;
        SetUsernameValue (newUsername);
        SetPasswordValue (newPassword);
    }

    private void ResetToggle()
    {
        toggle.GetComponent<Toggle>().isOn = false;
    }

    private void ResetInputFields()
    {
        usernameInputField.text = "";
        passwordInputField.text = "";
    }

    private void RegisterEvents()
    {
        EventBus.SIGN_IN += HandleSignIn;
        EventBus.UNSUCCESSFUL_SIGN_IN += HandleUnsuccessfulSignIn;
        EventBus.UNSUCCESSFUL_SIGN_UP += HandleUnsuccessfulSignUp;
    }

    private void UnregisterEvents()
    {
        EventBus.SIGN_IN -= HandleSignIn;
        EventBus.UNSUCCESSFUL_SIGN_IN -= HandleUnsuccessfulSignIn;
        EventBus.UNSUCCESSFUL_SIGN_UP -= HandleUnsuccessfulSignUp;
    }

    private void HandleUnsuccessfulSignIn()
    {
        SetMessage(errorMessageTMP, "Incorrect username or password!");
    }

    private void HandleUnsuccessfulSignUp()
    {
        SetMessage(errorMessageTMP, username + " is already taken!");
    }

    private void HandleSignIn()
    {
        SetPrefs();
        SetMessage(welcomeMessageTMP, "Welcome, " + username + "!");
        ToggleGameObject(errorMessageTMP, false);
        ToggleGameObject(signInMenu, false);
        ToggleGameObject(signOutMenu, true);
    }

    public void OnSignInButton()
    {
        SetUsernameAndPassword();
        accountHandler.SignIn();
    }

    public void OnSignUpButton()
    {
        SetUsernameAndPassword();
        accountHandler.SignUp();
    }

    public void OnSignOutButton()
    {
        ToggleGameObject(signInMenu, true);
        ToggleGameObject(signOutMenu, false);
        ResetInputFields();
        SavePrefs("", "");
    }
}
