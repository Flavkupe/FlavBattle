using System;

namespace FlavBattle.Components
{
    public interface ITrackableObject : IHasGameObject
    {
        event EventHandler<ITrackableObject> Destroyed;
    }
}
