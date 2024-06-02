// File Name: Emitter.cs
// Creation Date: May. 19, 2024
// Description: A type of open tile that can also emit new particles

using GameUtility;

class Emitter : OpenTile
{
    //NOTE: This timer is only updated every time all tiles are updated. This will result in skewed timings
    private Timer emittTimer;

    private PropagationState[] dirs;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="dirs">List of all the directions to emitt particles.</param>
    /// <param name="emittTime"></param>
    public Emitter(byte row, byte column, PropagationState[] dirs, float emittTime) : base(row, column)
    {
        this.dirs = dirs;
        emittTimer = new Timer(emittTime, true);
    }

    public override void Update(double elapsedTotalMilliseconds)
    {
        base.Update(elapsedTotalMilliseconds);

        emittTimer.Update(elapsedTotalMilliseconds);

        if(emittTimer.IsFinished())
        {

            for (short i = 0; i < dirs.Length; i++)
            {
                AddParticle(dirs[i], dirs[i], Particle.STARTING_ENERGY, (byte)(Game1.GlobalUpdateId + 1), false);
            }

            emittTimer.ResetTimer(true);
        }

    }
}