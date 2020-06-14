using UnityEngine;

public class KeyAction
{
    enum AxisState
    {
        Released,
        Down,
        Held,
        Up,
    };

    AxisState state;
    string axisName;

    public KeyAction(string axisName) 
    {
        this.axisName = axisName;
    }

    // Returns true if action is pressed
    public static implicit operator bool(KeyAction action)
    {
        return action.state == AxisState.Down || action.state == AxisState.Held;
    }

    public bool Released() => state == AxisState.Released;
    public bool Down() => state == AxisState.Down;
    public bool Held() => state == AxisState.Held;
    public bool Up() => state == AxisState.Up;

    public void Update()
    {
        bool isHold = Input.GetAxisRaw(axisName) == 1f;

        switch (state)
        {
            case AxisState.Released:
                if (isHold)
                    state = AxisState.Down;
                break;
            case AxisState.Down:
                if (isHold)
                    state = AxisState.Held;
                else
                    state = AxisState.Up;
                break;
            case AxisState.Held:
                if (!isHold)
                    state = AxisState.Up;
                break;
            case AxisState.Up:
                if (isHold)
                    state = AxisState.Down;
                else
                    state = AxisState.Released;
                break;
        }
    }
}
