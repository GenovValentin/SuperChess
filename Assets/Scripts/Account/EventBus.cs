using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public static class EventBus
{
    public static Action SIGN_IN;

    public static Action SIGN_OUT;

    public static Action UNSUCCESSFUL_SIGN_IN;

    public static Action UNSUCCESSFUL_SIGN_UP;

    public static Action START_GAME;

    public static Action<Team> GAME_ENDED;
}
