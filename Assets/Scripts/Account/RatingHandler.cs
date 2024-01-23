using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Networking.Transport;

public class RatingHandler : MonoBehaviour
{
    private GameObject ratingTMP;

    private GameObject whitePlayerRating;

    private GameObject blackPlayerRating;

    private GameObject whitePlayerName;

    private int playerRating;

    private int opponentRating;

    private int playerTeam;

    AccountHandler accountHandler;

    private void Start()
    {
        accountHandler = AccountHandler.GetInstance();
        RegisterEvents();
    }

    private void SetRatingTMP()
    {
        ratingTMP = GameObject.Find("Rating");
    }

    private void SetGameObjects()
    {
        whitePlayerName = GameObject.Find("WhitePlayerName");
        whitePlayerRating = GameObject.Find("WhitePlayerRating");
        blackPlayerRating = GameObject.Find("BlackPlayerRating");
    }

    private void HandleStartGame()
    {
        SetGameObjects();
        IEnumerator coroutineRating = DelayAssignRating(1.0f);
        StartCoroutine (coroutineRating);
    }

    private void HandleGameEnded(Team result)
    {
        CalculateNewRating (result);
        SetUpdatedUserRating();
        SetRatingGameObject (playerRating);
    }

    private IEnumerator DelayAssignRating(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        AssignRatingsToPlayers();
    }

    private void AssignRatingsToPlayers()
    {
        TMP_Text whiteName = whitePlayerName.GetComponent<TMP_Text>();
        TMP_Text whiteRating = whitePlayerRating.GetComponent<TMP_Text>();
        TMP_Text blackRating = blackPlayerRating.GetComponent<TMP_Text>();

        if (whiteName.text == accountHandler.ReturnUsername())
        {
            playerRating = ParseRating(whiteRating.text);
            opponentRating = ParseRating(blackRating.text);
            playerTeam = (int) Team.White;
            return;
        }

        playerRating = ParseRating(blackRating.text);
        opponentRating = ParseRating(whiteRating.text);
        playerTeam = (int) Team.Black;
    }

    private int ParseRating(string text)
    {
        int.TryParse(text, out int result);

        return result;
    }

    private void CalculateNewRating(Team result)
    {
        int coefficient = 40;
        double transformedPlayerRating;
        double transformedOpponentRating;

        double expectedScorePlayer;
        double actualScorePlayer;
        double updatedPlayerRating;

        transformedPlayerRating = Math.Pow(10, playerRating / (double) 400);
        transformedOpponentRating = Math.Pow(10, opponentRating / (double) 400);

        expectedScorePlayer =
            transformedPlayerRating /
            (transformedPlayerRating + transformedOpponentRating);

        actualScorePlayer = SetScore(result);

        updatedPlayerRating =
            playerRating +
            coefficient * (actualScorePlayer - expectedScorePlayer);

        playerRating = (int) updatedPlayerRating;
    }

    private double SetScore(Team result)
    {
        if (result == Team.Draw)
        {
            return 0.5;
        }
        return playerTeam == (int) result ? 1 : 0;
    }

    private void SetUpdatedUserRating()
    {
        accountHandler.SetUserRating (playerRating);
    }

    private void RegisterEvents()
    {
        EventBus.SIGN_IN += HandleSignIn;
        EventBus.START_GAME += HandleStartGame;
        EventBus.GAME_ENDED += HandleGameEnded;
    }

    private void UnregisterEvents()
    {
        EventBus.SIGN_IN -= HandleSignIn;
        EventBus.START_GAME -= HandleStartGame;
        EventBus.GAME_ENDED -= HandleGameEnded;
    }

    private void HandleSignIn()
    {
        SetRatingTMP();
        GetUserRating();
        SetRatingGameObject(GetUserRating());
    }

    private void SetRatingGameObject(int playerRating)
    {
        TMP_Text textComponent = ratingTMP.GetComponent<TMP_Text>();
        textComponent.text = "Your rating is: " + playerRating;
    }

    private int GetUserRating()
    {
        int rating = accountHandler.GetUserRating();
        return rating;
    }
}
