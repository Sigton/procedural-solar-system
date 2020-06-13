using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SettingsIdentifier
{
    int id = -1;
    public int nextID
    {
        get
        {
            id++;
            return id;
        }
    }

    private SettingsIdentifier() { }
    public static SettingsIdentifier Instance { get; } = new SettingsIdentifier();
}
