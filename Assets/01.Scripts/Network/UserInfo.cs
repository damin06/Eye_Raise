using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct UserInfo
{
    public string UserName;
    public int Coin;

    public UserInfo(string UserName, int Coin)
    {
        this.UserName = UserName;
        this.Coin = Coin;
    }
}
