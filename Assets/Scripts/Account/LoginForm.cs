using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Networking.Transport;

public class LoginForm : MonoBehaviour
{
    private TMP_InputField usernameInputField;

    private TMP_InputField passwordInputField;

    private TMP_InputField newUsernameInputField;

    private TMP_InputField repeatUsernameInputField;

    private GameObject errorMessageTMP;

    private GameObject welcomeMessageTMP;

    private GameObject errorChangeMessageTMP;

    private GameObject signInMenu;

    private GameObject signOutMenu;

    private GameObject deleteMenu;

    private GameObject changeUsernameMenu;

    private Toggle toggle;

    private string username;

    private string password;

    private string newUsername;

    private string repeatUsername;

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
        AddNewUsernameInputFieldListener();
        AddRepeatUsernameInputFieldListener();
        AddToggleListener();

        RegisterEvents();

        ToggleGameObject(errorMessageTMP, false);
        ToggleGameObject(errorChangeMessageTMP, false);
        ToggleGameObject(signInMenu, true);
        ToggleGameObject(signOutMenu, false);
        ToggleGameObject(deleteMenu, false);
        ToggleGameObject(changeUsernameMenu, false);

        ResetToggle();
        ResetInputFields();

        LoadPrefs();
    }

    private void SetObjects()
    {
        usernameInputField = GetTMP_InputField("Username_InputField");
        passwordInputField = GetTMP_InputField("Password_InputField");
        newUsernameInputField = GetTMP_InputField("New Username_InputField");
        repeatUsernameInputField =
            GetTMP_InputField("Repeat Username_InputField");

        errorMessageTMP = GameObject.Find("Error");
        welcomeMessageTMP = GameObject.Find("Welcome");
        errorChangeMessageTMP = GameObject.Find("Error Change");
        signInMenu = GameObject.Find("Sign In Menu");
        signOutMenu = GameObject.Find("Sign Out Menu");
        deleteMenu = GameObject.Find("Delete Menu");
        changeUsernameMenu = GameObject.Find("Change Username Menu");
        toggle = GetToggle("Check Box");
    }

    private TMP_InputField GetTMP_InputField(string inputFIeldName)
    {
        return GetComponent<TMP_InputField>(inputFIeldName);
    }

    private Toggle GetToggle(string toggle)
    {
        return GetComponent<Toggle>(toggle);
    }

    private T GetComponent<T>(string inputFIeldName)
    {
        GameObject gameObject = GameObject.Find(inputFIeldName);
        if (gameObject == null)
        {
            throw new Exception();
        }
        return gameObject.GetComponent<T>();
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

    private void SetNewUsernameValue(string newUsernameInputFieldValue)
    {
        newUsername = newUsernameInputFieldValue;
    }

    private void SetRepeatUsernameValue(string repeatUsernameInputField)
    {
        repeatUsername = repeatUsernameInputField;
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

    private void AddNewUsernameInputFieldListener()
    {
        newUsernameInputField.onEndEdit.AddListener (SetNewUsernameValue);
    }

    private void AddRepeatUsernameInputFieldListener()
    {
        repeatUsernameInputField.onEndEdit.AddListener (SetRepeatUsernameValue);
    }

    private void ToggleGameObject(GameObject gameObject, bool isActive)
    {
        if (gameObject == null)
        {
            return;
        }
        gameObject.SetActive (isActive);
    }

    private void ToggleValueChanged(bool isOn)
    {
        isToggleOn = isOn;
    }

    private void ChangeUsername(string newUsername)
    {
        accountHandler.ChangeUsername (newUsername);
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

    private void ResetChangeInputFields()
    {
        newUsernameInputField.text = "";
        repeatUsernameInputField.text = "";
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

    public void SignOut()
    {
        accountHandler.SignOut();
        ToggleGameObject(signInMenu, true);
        ToggleGameObject(signOutMenu, false);
        ResetInputFields();
        ResetToggle();
        SavePrefs("", "");
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
        SignOut();
    }

    public void OnDeleteAccountButton()
    {
        ToggleGameObject(deleteMenu, true);
    }

    public void OnDeleteButton()
    {
        ToggleGameObject(deleteMenu, false);

        accountHandler.DeleteUser();
        SignOut();
    }

    public void OnKeepButton()
    {
        ToggleGameObject(deleteMenu, false);
    }

    public void OnChangeButton()
    {
        if (newUsername != repeatUsername)
        {
            SetMessage(errorChangeMessageTMP, "Usernames do not match!");
            return;
        }
        username = newUsername;
        ChangeUsername (newUsername);
        ToggleGameObject(changeUsernameMenu, false);
        ToggleGameObject(signOutMenu, true);
        SetMessage(welcomeMessageTMP, "Welcome, " + username + "!");
        ResetChangeInputFields();
    }

    public void OnChangeUsernameButton()
    {
        ToggleGameObject(signOutMenu, false);
        ToggleGameObject(changeUsernameMenu, true);
    }

    public void OnCancelButton()
    {
        ToggleGameObject(changeUsernameMenu, false);
        ToggleGameObject(signOutMenu, true);
        ResetChangeInputFields();
    }
}
