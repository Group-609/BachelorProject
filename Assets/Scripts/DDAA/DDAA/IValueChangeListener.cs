using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IValueChangeListener
{
    void OnValueChanged(float value);
}

public class OnValueChangeListener: IValueChangeListener
{
    private readonly Action<float> onValueChanged;

    public OnValueChangeListener(Action<float> onValueChanged)
    {
        this.onValueChanged = onValueChanged;
    }

    public void OnValueChanged(float value)
    {
        onValueChanged.Invoke(value);
    }
}
