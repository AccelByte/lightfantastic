using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;

namespace Game
{
    /// <summary>
    ///     This class will be in charge of player spawn location
    /// </summary>
    public class BasePlayerStart : MonoBehaviour
    {
        public bool Occupied { get; set; }
    }
}
