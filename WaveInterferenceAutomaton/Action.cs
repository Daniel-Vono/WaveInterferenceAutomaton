using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct Action
{
    public enum ActionType 
    {
        Waves,
        AbsorbWall,
        Emitter
    }

    public Tuple<byte, byte> Coord;
    public ActionType Type;

    public Action(Tuple<byte, byte> coord, ActionType type)
    {
        Coord = coord;
        Type = type;
    }
}
