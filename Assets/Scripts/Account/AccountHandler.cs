using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Networking.Transport;

public class AccountHandler : MonoBehaviour
{
    MongoClientWrapper mongoClient;

    private string newUsername;

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
        newUsername = username;
        newPassword = password;
    }

    public string ReturnUsername()
    {
        return newUsername;
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
        return mongoClient.GetUserSettings(newUsername);
    }

    public int GetUserRating()
    {
        return mongoClient.GetUserRating(newUsername);
    }

    public void SetUserRating(int rating)
    {
        mongoClient.SetUserRating (newUsername, rating);
    }

    public void SetVolume(float volume)
    {
        mongoClient.SetUserVolume (newUsername, volume);
    }

    public void SignIn()
    {
        if (mongoClient.DoesUserExist(newUsername, newPassword) == false)
        {
            EmitUnsuccessfulSignIn();
            return;
        }

        EmitSignIn();
    }

    public void SignUp()
    {
        if (mongoClient.IsUsernameTaken(newUsername))
        {
            EmitUnsuccessfulSignUp();
            return;
        }

        mongoClient.CreateUser(newUsername, newPassword, 0.5f);
        EmitSignIn();
    }

    public void SignOut()
    {
        EmitSignOut();
    }
}
