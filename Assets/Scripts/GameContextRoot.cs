using strange.extensions.context.impl;
using UnityEngine;
using System.Collections;

public class GameContextRoot : ContextView
{

    void Awake()
    {
        context = new GameContext(this);
    }

}
