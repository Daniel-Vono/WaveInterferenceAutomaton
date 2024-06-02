// File Name: OpenTile.cs
// Creation Date: May. 03, 2024
// Description: An open tile that can hold particles

class OpenTile : Tile
{
    public const float REASONABLE_MAX_ENERGY = Particle.STARTING_ENERGY * 2;

    protected LList<Particle> particles = new LList<Particle>();

    public OpenTile(byte row, byte column) : base(row, column)
    {

    }

    public override void Update(double elapsedTotalMilliseconds)
    {
        Node<Particle> currPart = particles.Head;
        Node<Particle> prevPart = null;

        short idx = 0;

        while (currPart != null)
        {
            if (currPart.Value.UpdateId == Game1.GlobalUpdateId)
            {
                currPart.Value.Update();

                currPart = currPart.Next;
                particles.RemoveAt(idx);
            }
            else
            {
                prevPart = currPart;
                currPart = currPart.Next;

                idx++;
            }
        }
    }

    public override void AddParticle(PropagationState state, PropagationState emitterState, float energyLevel, byte updateId, bool superpositionAccountedFor = true)
    {
        particles.AddToTail(new Particle(state, emitterState, energyLevel, row, column, updateId, superpositionAccountedFor));
    }

    public override float Superposition()
    {
        return float.NaN;
    }
}