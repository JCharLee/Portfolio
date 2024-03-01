using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour, IInteraction
{
    public string interactionPrompt => null;

    public bool Action(Player interactor)
    {
        Debug.Log("¹Ú½º ¿ÀÇÂ!");
        return true;
    }
}