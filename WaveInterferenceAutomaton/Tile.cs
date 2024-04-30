// File Name: Tile.cs
// Creation Date: Apr. 29, 2024
// Description: The generic tile class used to hold partilces in the simulation

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

class Tile
{
    public const byte DIMENSION = 8;

    public Rectangle Rect { get;  protected set; }

    protected byte row;
    protected byte column;

    public Tile(byte row, byte column)
    {
        this.row = row;
        this.column = column;

        Rect = new Rectangle(column * DIMENSION, row * DIMENSION, DIMENSION, DIMENSION);
    }
}

