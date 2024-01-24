using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Networking.Transport;

public class AccountHandler : MonoBehaviour
{
    MongoClientWrapper mongoClient;

    private string accountUsername;

    private string newPassword;

    static AccountHandler instance = null;

    public static AccountHandler GetInstance()
    {
        if (instance == null)
        {
            GameObject accountHandlerObject = new GameObject("AccountHandler");
            instance = accountHandlerObject.AddComponent<AccountHandler>();
            instance.mongoClient = MongoClientWrapper.GetInstance();
        }

        return instance;
    }

    private void Start()
    {
        mongoClient = MongoClientWrapper.GetInstance();
    }

    public void SetUsernameAndPassword(string username, string password)
    {
        accountUsername = username;
        newPassword = password;
    }

    public string ReturnUsername()
    {
        return accountUsername;
    }

    private void EmitSignIn()
    {
        EventBus.SIGN_IN?.Invoke();
    }

    private void EmitSignOut()
    {
        EventBus.SIGN_OUT?.Invoke();
    }

    private void EmitUnsuccessfulSignIn()
    {
        EventBus.UNSUCCESSFUL_SIGN_IN?.Invoke();
    }

    private void EmitUnsuccessfulSignUp()
    {
        EventBus.UNSUCCESSFUL_SIGN_UP?.Invoke();
    }

    public Settings GetUserSettings()
    {
        return mongoClient.GetUserSettings(accountUsername);
    }

    public int GetUserRating()
    {
        return mongoClient.GetUserRating(accountUsername);
    }

    public void SetUserRating(int rating)
    {
        mongoClient.SetUserRating (accountUsername, rating);
    }

    public void SetVolume(float volume)
    {
        mongoClient.SetUserVolume (accountUsername, volume);
    }

    public void ChangeUsername(string newUsername)
    {
        mongoClient.ChangeUsername (accountUsername, newUsername);
    }

    public void DeleteUser()
    {
        mongoClient.DeleteUser (accountUsername);
    }

    public void SignIn()
    {
        if (mongoClient.DoesUserExist(accountUsername, newPassword) == false)
        {
            EmitUnsuccessfulSignIn();
            return;
        }

        EmitSignIn();
    }

    public void SignUp()
    {
        if (mongoClient.IsUsernameTaken(accountUsername))
        {
            EmitUnsuccessfulSignUp();
            return;
        }

        mongoClient.CreateUser(accountUsername, newPassword, 0.5f);
        EmitSignIn();
    }

    public void SignOut()
    {
        EmitSignOut();
    }
}
