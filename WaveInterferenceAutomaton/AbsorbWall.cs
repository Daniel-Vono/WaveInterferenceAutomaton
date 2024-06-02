// File Name: AbsorbWall.cs
// Creation Date: May. 19, 2024
// Description: A tile that stops all incoming particles

class AbsorbWall : Tile
{
    public const float SUPERPOSITION = -1;
    public AbsorbWall(byte row, byte column) : base(row, column)
    {
        Game1.Superpositions[row, column] = SUPERPOSITION;
    }

    public override void Update(double elapsedTotalMilliseconds)
    {
        
    }

    public override void AddParticle(PropagationState state, PropagationState emitterState, float energyLevel, byte updateId, bool superpositionAccountedFor = true)
    {
        
    }

    public override float Superposition()
    {
        return SUPERPOSITION;
    }
}