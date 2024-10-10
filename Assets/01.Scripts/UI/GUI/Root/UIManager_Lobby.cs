using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager_Lobby : UI_Root
{
    public static UIManager_Lobby Instance;

    protected override void Awake()
    {
        base.Awake();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

    }
}
