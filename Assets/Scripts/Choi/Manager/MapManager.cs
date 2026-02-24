using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.cheol.Manager
{
    public class MapManager : BaseManager
    {
        [SerializeField] private Transform[] flagPoints;
        [SerializeField] private Transform[] spawnPoint;


        public Transform[] FlagPoints { get => flagPoints; set => flagPoints = value; }

        public override void HandleEvent(string data)
        {

        }
    }

}
