using Dev.cheol.Manager;
using Dev.cheol.Model;
using Dev.cheol.Stats;
using Dev.cheol.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.cheol.Model
{

    public class Tower : BaseUnit
    {
        [SerializeField] private TileObject _currentTile;
        [SerializeField] private int _lank;
        [SerializeField] private LankStarUI _starUI;
        public TileObject CurrentTile { get => _currentTile; set => _currentTile = value; }
        public int Lank { get => _lank; set => _lank = value; }
        public LankStarUI StarUI { get => _starUI; set => _starUI = value; }

        public override void ActiveAttack()
        {
            throw new System.NotImplementedException();
        }
        public override void ObjectUpdate()
        {
        }


    }
}