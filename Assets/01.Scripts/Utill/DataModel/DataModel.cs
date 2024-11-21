using System;
using UnityEngine;

public class DataModel<T> : ScriptableObject where T : IEquatable<T>
{
    protected T v;
    Action<T> onChange;
    public virtual T val
    {
        get
        {
            return v;
        }
        set
        {
            if ((v != null || value != null) && v.Equals(value)) return;
            v = value;
            onChange.Invoke(v);
        }
    }

    public void Register(Action<T> onChange, Action disposer)
    {
        this.onChange += onChange;
        disposer += () => this.onChange -= onChange;
    }
}
