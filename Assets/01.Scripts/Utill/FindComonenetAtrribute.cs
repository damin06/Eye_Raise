using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class FindComonenetAtrribute : System.Attribute
{
    public string _gameObjectName { get; }
    public FindComonenetAtrribute(string gameObjectName)
    {
        _gameObjectName = gameObjectName;
    }
}
