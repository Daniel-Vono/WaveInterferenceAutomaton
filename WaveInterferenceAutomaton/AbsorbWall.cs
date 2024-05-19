// File Name: AbsorbWall.cs
// Creation Date: May. 19, 2024
// Description: A tile that stops all incoming particles

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;


class AbsorbWall : Tile
{
    public const float SUPERPOSITION = -1;
    public AbsorbWall(byte row, byte column) : base(row, column)
    {

    }

    public override void Update(double elapsedTotalMilliseconds)
    {
        
    }

    public override void AddParticle(PropagationState state, PropagationState emitterState, float energyLevel, byte updateId)
    {
        
    }

    public override float Superposition()
    {
        return SUPERPOSITION;
    }
}