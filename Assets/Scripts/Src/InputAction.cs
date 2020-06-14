using UnityEngine;

public class InputAction
{
    enum InputState
    {
        Released,
        Down,
        Held,
        Up,
    };

    InputState state;

    // Returns true if action is pressed
    public static implicit operator bool(InputAction action)
    {
        return action.state == InputState.Down || action.state == InputState.Held;
    }

    public bool Released() => state == InputState.Released;
    public bool Down() => state == InputState.Down;
    public bool Held() => state == InputState.Held;
    public bool Up() => state == InputState.Up;

    public void Update(string axisName)
    {
        bool isHold = Input.GetAxisRaw(axisName) == 1f;

        switch (state)
        {
            case InputState.Released:
                if (isHold)
                    state = InputState.Down;
                break;
            case InputState.Down:
                if (isHold)
                    state = InputState.Held;
                else
                    state = InputState.Up;
                break;
            case InputState.Held:
                if (!isHold)
                    state = InputState.Up;
                break;
            case InputState.Up:
                if (isHold)
                    state = InputState.Down;
                else
                    state = InputState.Released;
                break;
        }
    }
}
