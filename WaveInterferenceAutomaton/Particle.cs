// File Name: Particle.cs
// Creation Date: May. 03, 2024
// Description: One part of a wave which contains its own energy value

using System;

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
    //The starting energy value of a particle
    //This value must be 1 for drawing to work as intended
    public const float STARTING_ENERGY = 1;

    private const float ENERGY_DISSIPATION = 0.75f;
    private const float ENERGY_DISSIPATION_DIAG = ENERGY_DISSIPATION * 0.85f;
    private const float ENERGY_IMPROPER_DIR_MULTIPLIER = 0.25f;
    private const float ENERGY_TOLERANCE = 0.005f;

    public byte UpdateId { get; private set; }
    private PropagationState emitterState;
    private PropagationState state;
    private Tuple<byte, byte> gridLoc;

    public float ELevel { get; private set; }

    public Particle(PropagationState state, PropagationState emitterState, float energyLevel, byte gridY, byte gridX, byte updateId, bool superpositionAccountedFor=true)
    {
        UpdateId = updateId;

        this.state = state;
        this.emitterState = emitterState;
        ELevel = energyLevel;

        gridLoc = new Tuple<byte, byte>(gridY, gridX);

        if(!superpositionAccountedFor) Game1.Superpositions[gridLoc.Item1, gridLoc.Item2] += ELevel;
    }

    private float CalcNewEnergyLevel()
    {
        bool similarDirection = false;

        //TODO: Add all cases

        switch(emitterState)
        {
            case PropagationState.Right:

                if(state == PropagationState.Right || state == PropagationState.UpRight || state == PropagationState.DownRight)
                {
                    similarDirection = true;
                }
                else
                {
                    similarDirection = false;
                }
                break;

            case PropagationState.Left:

                if (state == PropagationState.Left || state == PropagationState.UpLeft || state == PropagationState.DownLeft)
                {
                    similarDirection = true;
                }
                else
                {
                    similarDirection = false;
                }

                break;

            case PropagationState.Up:

                if (state == PropagationState.Up || state == PropagationState.UpLeft || state == PropagationState.UpRight)
                {
                    similarDirection = true;
                }
                else
                {
                    similarDirection = false;
                }

                break;

            case PropagationState.Down:

                if (state == PropagationState.Down || state == PropagationState.DownLeft || state == PropagationState.DownRight)
                {
                    similarDirection = true;
                }
                else
                {
                    similarDirection = false;
                }

                break;

            case PropagationState.UpRight:

                if (state == PropagationState.UpRight || state == PropagationState.Right || state == PropagationState.Up)
                {
                    similarDirection = true;
                }
                else
                {
                    similarDirection = false;
                }
                break;

            case PropagationState.DownRight:

                if (state == PropagationState.DownRight || state == PropagationState.Right || state == PropagationState.Down)
                {
                    similarDirection = true;
                }
                else
                {
                    similarDirection = false;
                }
                break;
        }

        if(similarDirection) return ELevel * ENERGY_DISSIPATION;

        return ELevel * ENERGY_DISSIPATION * ENERGY_IMPROPER_DIR_MULTIPLIER;
    }

    private void CopyParticle(PropagationState newState)
    {
        int yIndex = 0;
        int xIndex = 0;

        switch (newState)
        {
            case PropagationState.Up:

                yIndex = gridLoc.Item1 - 1;
                xIndex = gridLoc.Item2;

                if (yIndex < 0 || yIndex >= Game1.GetGridHeight() || xIndex < 0 || xIndex >= Game1.GetGridWidth()) return;

                Game1.Grid[yIndex, xIndex].AddParticle(newState, emitterState, CalcNewEnergyLevel(), (byte)(UpdateId + 1));
                break;

            case PropagationState.Down:

                yIndex = gridLoc.Item1 + 1;
                xIndex = gridLoc.Item2;

                if (yIndex < 0 || yIndex >= Game1.GetGridHeight() || xIndex < 0 || xIndex >= Game1.GetGridWidth()) return;

                Game1.Grid[yIndex, xIndex].AddParticle(newState, emitterState, CalcNewEnergyLevel(), (byte)(UpdateId + 1));
                break;

            case PropagationState.Left:

                yIndex = gridLoc.Item1;
                xIndex = gridLoc.Item2 - 1;

                if (yIndex < 0 || yIndex >= Game1.GetGridHeight() || xIndex < 0 || xIndex >= Game1.GetGridWidth()) return;

                Game1.Grid[yIndex, xIndex].AddParticle(newState, emitterState, CalcNewEnergyLevel(), (byte)(UpdateId + 1));
                break;

            case PropagationState.Right:

                yIndex = gridLoc.Item1;
                xIndex = gridLoc.Item2 + 1;

                if (yIndex < 0 || yIndex >= Game1.GetGridHeight() || xIndex < 0 || xIndex >= Game1.GetGridWidth()) return;

                Game1.Grid[yIndex, xIndex].AddParticle(newState, emitterState, CalcNewEnergyLevel(), (byte)(UpdateId + 1));
                break;

            case PropagationState.UpRight:

                yIndex = gridLoc.Item1 - 1;
                xIndex = gridLoc.Item2 + 1;

                if (yIndex < 0 || yIndex >= Game1.GetGridHeight() || xIndex < 0 || xIndex >= Game1.GetGridWidth()) return;

                Game1.Grid[yIndex, xIndex].AddParticle(newState, emitterState, CalcNewEnergyLevel(), (byte)(UpdateId + 1));
                break;

            case PropagationState.DownRight:

                yIndex = gridLoc.Item1 + 1;
                xIndex = gridLoc.Item2 + 1;

                if (yIndex < 0 || yIndex >= Game1.GetGridHeight() || xIndex < 0 || xIndex >= Game1.GetGridWidth()) return;

                Game1.Grid[yIndex, xIndex].AddParticle(newState, emitterState, CalcNewEnergyLevel(), (byte)(UpdateId + 1));
                break;

            case PropagationState.DownLeft:

                yIndex = gridLoc.Item1 + 1;
                xIndex = gridLoc.Item2 - 1;

                if (yIndex < 0 || yIndex >= Game1.GetGridHeight() || xIndex < 0 || xIndex >= Game1.GetGridWidth()) return;

                Game1.Grid[yIndex, xIndex].AddParticle(newState, emitterState, CalcNewEnergyLevel(), (byte)(UpdateId + 1));
                break;

            case PropagationState.UpLeft:

                yIndex = gridLoc.Item1 - 1;
                xIndex = gridLoc.Item2 - 1;

                if (yIndex < 0 || yIndex >= Game1.GetGridHeight() || xIndex < 0 || xIndex >= Game1.GetGridWidth()) return;

                Game1.Grid[yIndex, xIndex].AddParticle(newState, emitterState, CalcNewEnergyLevel(), (byte)(UpdateId + 1));
                break;
        }

        if (!(Game1.Grid[yIndex, xIndex] is AbsorbWall)) Game1.Superpositions[yIndex, xIndex] += CalcNewEnergyLevel();
        
    }

    public void Update()
    {
        if (Math.Abs(ELevel) < ENERGY_TOLERANCE)
        {
            Game1.Superpositions[gridLoc.Item1, gridLoc.Item2] -= ELevel;
            return;
        }

        switch(state)
        {
            case PropagationState.Up:
                CopyParticle(PropagationState.UpLeft);
                CopyParticle(PropagationState.Up);
                CopyParticle(PropagationState.UpRight);
                break;

            case PropagationState.Down:
                CopyParticle(PropagationState.DownLeft);
                CopyParticle(PropagationState.Down);
                CopyParticle(PropagationState.DownRight);
                break;

            case PropagationState.Left:
                CopyParticle(PropagationState.UpLeft);
                CopyParticle(PropagationState.Left);
                CopyParticle(PropagationState.DownLeft);
                break;

            case PropagationState.Right:
                CopyParticle(PropagationState.UpRight);
                CopyParticle(PropagationState.Right);
                CopyParticle(PropagationState.DownRight);
                break;

            case PropagationState.UpRight:
                CopyParticle(PropagationState.Up);
                CopyParticle(PropagationState.UpRight);
                CopyParticle(PropagationState.Right);
                break;

            case PropagationState.DownRight:
                CopyParticle(PropagationState.Down);
                CopyParticle(PropagationState.DownRight);
                CopyParticle(PropagationState.Right);
                break;

            case PropagationState.DownLeft:
                CopyParticle(PropagationState.Down);
                CopyParticle(PropagationState.DownLeft);
                CopyParticle(PropagationState.Left);
                break;

            case PropagationState.UpLeft:
                CopyParticle(PropagationState.Up);
                CopyParticle(PropagationState.UpLeft);
                CopyParticle(PropagationState.Left);
                break;
        }

        Game1.Superpositions[gridLoc.Item1, gridLoc.Item2] -= ELevel;
    }
}

