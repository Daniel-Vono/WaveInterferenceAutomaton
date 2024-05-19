// File Name: OpenTile.cs
// Creation Date: May. 03, 2024
// Description: An open tile that can hold particles

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;


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

    public override void AddParticle(PropagationState state, PropagationState emitterState, float energyLevel, byte updateId)
    {
        particles.AddToTail(new Particle(state, emitterState, energyLevel, row, column, updateId));
    }

    public override float Superposition()
    {
        float superposition = 0;

        particles.IterateGeneric((LList<Particle> List, Node<Particle> node, short index) =>
        {
            superposition += node.Value.ELevel;
        });

        return superposition;
    }
}