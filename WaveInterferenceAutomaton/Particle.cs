// File Name: Particle.cs
// Creation Date: May. 03, 2024
// Description: One part of a wave which contains its own energy value

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

public enum PropagationState
{
    Up,
    Down,
    Left,
    Right,
    UpRight,
    DownRight,
    DownLeft,
    UpLeft
}

class Particle
{
    private const float ENERGY_DISSIPATION = 0.5f;
    private const float ENERGY_TOLERANCE = 0.05f;

    public byte UpdateId { get; private set; }
    private PropagationState state;
    private Tuple<byte, byte> gridLoc;

    public float ELevel { get; private set; }

    public Particle(PropagationState state, float energyLevel, byte gridX, byte gridY, byte updateId)
    {
        UpdateId = updateId;

        this.state = state;
        ELevel = energyLevel;

        gridLoc = new Tuple<byte, byte>(gridX, gridY);
    }

    private void CopyParticle(PropagationState newState,short xShift, short yShift)
    {
        //TODO: Check if valid indices

        Game1.Grid[gridLoc.Item1 + yShift, gridLoc.Item2 + xShift].AddParticle(newState, ELevel * ENERGY_DISSIPATION, (byte)(UpdateId + 1));
    }

    public void Update()
    {
        if (Math.Abs(ELevel) < ENERGY_TOLERANCE) 
            return;

        switch(state)
        {
            case PropagationState.Up:
                CopyParticle(PropagationState.UpLeft, -1, -1);
                CopyParticle(PropagationState.Up, 0, -1);
                CopyParticle(PropagationState.UpRight, 1, -1);
                break;

            case PropagationState.Down:
                CopyParticle(PropagationState.DownLeft, -1, 1);
                CopyParticle(PropagationState.Down, 0, 1);
                CopyParticle(PropagationState.DownRight, 1, 1);
                break;

            case PropagationState.Left:
                break;

            case PropagationState.Right:
                CopyParticle(PropagationState.UpRight, 1, -1);
                CopyParticle(PropagationState.Right, 1, 0);
                CopyParticle(PropagationState.DownRight, 1, 1);
                break;

            case PropagationState.UpRight:
                CopyParticle(PropagationState.Up, 0, -1);
                CopyParticle(PropagationState.UpRight, 1, -1);
                CopyParticle(PropagationState.Right, 1, 0);
                break;

            case PropagationState.DownRight:
                CopyParticle(PropagationState.Down, 0, 1);
                CopyParticle(PropagationState.DownRight, 1, 1);
                CopyParticle(PropagationState.Right, 1, 0);
                break;

            case PropagationState.DownLeft:
                break;

            case PropagationState.UpLeft:
                break;
        }
    }
}

