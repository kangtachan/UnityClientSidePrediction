using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InputMessage
{
    public Inputs input;
}

public struct Inputs
{
    public bool up;
    public bool down;
    public bool left;
    public bool right;
}

public struct ServerStateMessage
{
    public RigidbodyState[] rigidbody_states;
    public uint tick;
    public uint score;
}

public struct RigidbodyState
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 angular_velocity;
}
