using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CustomProperties
{
    public static int[] playersNumber;
    public static int playerLocalIdx;
    public static int playerOtherIdx;
    public static void SetPlayers()
    {
        playersNumber = new int[2];
        playersNumber[0] = playersNumber[1] = playerLocalIdx = playerOtherIdx= -1;
        int i = 0;
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            int idx = p.ActorNumber;
            if (playersNumber[0] == -1)
                playersNumber[0] = idx;
            else
                playersNumber[1] = idx;
            if (p.IsLocal)
                playerLocalIdx = i;
            else
                playerOtherIdx = i;
            i++;
        }
    }
    public static Player GetPlayerByIdx(int id)
    {
        return PhotonNetwork.CurrentRoom.GetPlayer(playersNumber[id]);
    }
    public static void SetLocalPlayerProp(object key, object val)
    {
        Hashtable a = new Hashtable();
        a[key] = val;
        PhotonNetwork.LocalPlayer.SetCustomProperties(a);
    }
    public static void SetRoomProp(object key, object val)
    {
        Hashtable a = new Hashtable();
        a[key] = val;
        PhotonNetwork.CurrentRoom.SetCustomProperties(a);
    }
    public static void SetPlayerProp(object key,object val,int playerNumber)
    {
        Hashtable a = new Hashtable();
        a[key] = val;
        PhotonNetwork.CurrentRoom.GetPlayer(playerNumber).SetCustomProperties(a);
    }
    public static object GetPlayerProp(object key, bool isLocal = true)
    {
        Player p = isLocal ? PhotonNetwork.CurrentRoom.Players[playersNumber[playerLocalIdx]] : PhotonNetwork.CurrentRoom.Players[playersNumber[playerOtherIdx]];
        object val;
        if (p.CustomProperties.TryGetValue(key, out val))
        {
            return val;
        }
        return null;
    }
    public static object GetPlayerProp(object key, int id)
    {
        Player p = PhotonNetwork.CurrentRoom.Players[playersNumber[id]];
        object val;
        if (p.CustomProperties.TryGetValue(key, out val))
        {
            return val;
        }
        return null;
    }
    public static object GetRoomProp(object key)
    {
        object val;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out val))
        {
            return val;
        }
        return null;
    }

    #region PlayerProp
    public static PlayerManager.PlayerState GetLocalState()
    {
        return (PlayerManager.PlayerState)GetPlayerProp("state");
    }
    public static void SetLocalState(PlayerManager.PlayerState state)
    {
        SetLocalPlayerProp("state", state);
    }
    #endregion
}
