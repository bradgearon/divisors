using strange.extensions.context.api;
using strange.extensions.context.impl;
using UnityEngine;
using System.Collections;

public class GameContext : MVCSContext
{

    public GameContext(MonoBehaviour view)
        : base(view)
    {

    }

    public GameContext(MonoBehaviour view, ContextStartupFlags flags)
        : base(view, flags)
    {

    }

    protected override void mapBindings()
    {
        injectionBinder.Bind<ITileManager>().To<TileManager>();
    }
}
