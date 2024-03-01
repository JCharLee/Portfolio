using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteraction
{
    string interactionPrompt { get; }
    bool Action(Player interactor);
}