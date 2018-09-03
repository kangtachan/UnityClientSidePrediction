using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IClient
{
    event Action<InputMessage> NewClientMessage;
    void ReceiveServerMessage(ServerStateMessage stateMessage);
    void SetSimulationActive(bool active);
}
