using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _MessageType;

public class PlayersData : MonoBehaviour
{

    public static string[] names;
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        names = new string[Server.MAX_PLAYERS];
       

        if (MessageManager.messageDistribute.Count == 0) return;
        MessageManager.messageDistribute[MessageType.SETTINGS] += MessageSettings;
    }

    private void OnDestroy()
    {
        if (MessageManager.messageDistribute.Count == 0) return;
        MessageManager.messageDistribute[MessageType.POSITION] -= MessageSettings;
    }

    public void MessageSettings(Message m)
    {
        Settings s = m as Settings;

        SaveDataTank(s.playerID, s.tankName, s.color);
        SetSettingsTanks();
    }

    public void SaveDataTank(int id, string name, Color color)
    {
        names[id] = name;
   
    }

    public void SetSettingsTanks()
    {
        //for (int i = 0; i < GameState.tanks.Length; i++)
        //{
        //    ColorTank tankSettings = GameState.tanks[i].GetComponentInChildren<ColorTank>();

        //    if (names[i] == "" || names[i] == null) tankSettings.SetName("Player " + (i + 1));
        //    else tankSettings.SetName(names[i]);

        //    tankSettings.SetColorInGame(colors[i]);
        //}
    }
}
