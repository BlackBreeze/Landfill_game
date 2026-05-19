using UnityEngine;

public enum TetrominoType { I, O, T, S, Z, J, L }

public static class TetrominoData
{
    // Cell offsets [pieceType][rotation][cellIndex] — (x=col, y=row), y+ = up
    // All offsets are relative to the piece pivot (spawn position)
    public static readonly Vector2Int[][][] Cells = new Vector2Int[][][]
    {
        // I
        new Vector2Int[][] {
            new[]{ V(-1,0), V(0,0), V(1,0), V(2,0) },   // R0 ████
            new[]{ V(0,1),  V(0,0), V(0,-1),V(0,-2) },   // R1 vertical
            new[]{ V(-2,-1),V(-1,-1),V(0,-1),V(1,-1) },  // R2 ████ (shifted)
            new[]{ V(1,1),  V(1,0), V(1,-1),V(1,-2) },   // R3 vertical (shifted)
        },
        // O (no effective rotation)
        new Vector2Int[][] {
            new[]{ V(0,0), V(1,0), V(0,1), V(1,1) },
            new[]{ V(0,0), V(1,0), V(0,1), V(1,1) },
            new[]{ V(0,0), V(1,0), V(0,1), V(1,1) },
            new[]{ V(0,0), V(1,0), V(0,1), V(1,1) },
        },
        // T
        new Vector2Int[][] {
            new[]{ V(0,1), V(-1,0), V(0,0), V(1,0) },   // R0 .T. / TTT
            new[]{ V(1,0), V(0,1),  V(0,0), V(0,-1) },  // R1 T. / TT / T.
            new[]{ V(0,-1),V(1,0),  V(0,0), V(-1,0) },  // R2 TTT / .T.
            new[]{ V(-1,0),V(0,-1), V(0,0), V(0,1) },   // R3 .T / TT / .T
        },
        // S
        new Vector2Int[][] {
            new[]{ V(-1,0), V(0,0), V(0,1), V(1,1) },   // R0 .## / ##.
            new[]{ V(0,1),  V(0,0), V(1,0), V(1,-1) },  // R1 #. / ## / .#
            new[]{ V(-1,-1),V(0,-1),V(0,0), V(1,0) },   // R2 (same as R0, shifted)
            new[]{ V(-1,1), V(-1,0),V(0,0), V(0,-1) },  // R3 (same as R1, shifted)
        },
        // Z
        new Vector2Int[][] {
            new[]{ V(-1,1), V(0,1), V(0,0), V(1,0) },   // R0 ##. / .##
            new[]{ V(1,1),  V(1,0), V(0,0), V(0,-1) },  // R1 .# / ## / #.
            new[]{ V(-1,0), V(0,0), V(0,-1),V(1,-1) },  // R2 (same as R0, shifted)
            new[]{ V(-1,-1),V(-1,0),V(0,0), V(0,1) },   // R3 (same as R1, shifted)
        },
        // J
        new Vector2Int[][] {
            new[]{ V(-1,1), V(-1,0), V(0,0), V(1,0) },  // R0 #.. / ###
            new[]{ V(1,1),  V(0,1),  V(0,0), V(0,-1) }, // R1 ## / #. / #.
            new[]{ V(1,-1), V(1,0),  V(0,0), V(-1,0) }, // R2 ### / ..#
            new[]{ V(-1,-1),V(0,-1), V(0,0), V(0,1) },  // R3 .# / .# / ##
        },
        // L
        new Vector2Int[][] {
            new[]{ V(1,1),  V(-1,0), V(0,0), V(1,0) },  // R0 ..# / ###
            new[]{ V(1,-1), V(0,1),  V(0,0), V(0,-1) }, // R1 #. / #. / ##
            new[]{ V(-1,-1),V(1,0),  V(0,0), V(-1,0) }, // R2 ### / #..
            new[]{ V(-1,1), V(0,-1), V(0,0), V(0,1) },  // R3 ## / .# / .#
        },
    };

    // SRS wall kick offsets for JLSTZ pieces [fromRotation * 2 + direction] (0=CW, 1=CCW per pair)
    // Index: 0=0→1, 1=1→0, 2=1→2, 3=2→1, 4=2→3, 5=3→2, 6=3→0, 7=0→3
    public static readonly Vector2Int[][] WallKicksJLSTZ = new Vector2Int[][]
    {
        new[]{ V(0,0), V(-1,0), V(-1, 1), V(0,-2), V(-1,-2) }, // 0→1
        new[]{ V(0,0), V( 1,0), V( 1,-1), V(0, 2), V( 1, 2) }, // 1→0
        new[]{ V(0,0), V( 1,0), V( 1,-1), V(0, 2), V( 1, 2) }, // 1→2
        new[]{ V(0,0), V(-1,0), V(-1, 1), V(0,-2), V(-1,-2) }, // 2→1
        new[]{ V(0,0), V( 1,0), V( 1, 1), V(0,-2), V( 1,-2) }, // 2→3
        new[]{ V(0,0), V(-1,0), V(-1,-1), V(0, 2), V(-1, 2) }, // 3→2
        new[]{ V(0,0), V(-1,0), V(-1,-1), V(0, 2), V(-1, 2) }, // 3→0
        new[]{ V(0,0), V( 1,0), V( 1, 1), V(0,-2), V( 1,-2) }, // 0→3
    };

    public static readonly Vector2Int[][] WallKicksI = new Vector2Int[][]
    {
        new[]{ V(0,0), V(-2,0), V( 1,0), V(-2,-1), V( 1, 2) }, // 0→1
        new[]{ V(0,0), V( 2,0), V(-1,0), V( 2, 1), V(-1,-2) }, // 1→0
        new[]{ V(0,0), V(-1,0), V( 2,0), V(-1, 2), V( 2,-1) }, // 1→2
        new[]{ V(0,0), V( 1,0), V(-2,0), V( 1,-2), V(-2, 1) }, // 2→1
        new[]{ V(0,0), V( 2,0), V(-1,0), V( 2, 1), V(-1,-2) }, // 2→3
        new[]{ V(0,0), V(-2,0), V( 1,0), V(-2,-1), V( 1, 2) }, // 3→2
        new[]{ V(0,0), V( 1,0), V(-2,0), V( 1,-2), V(-2, 1) }, // 3→0
        new[]{ V(0,0), V(-1,0), V( 2,0), V(-1, 2), V( 2,-1) }, // 0→3
    };

    // Maps rotation transition to kick table index
    public static int KickIndex(int fromRot, bool clockwise)
    {
        // Pairs: (0CW=0), (1CCW=1), (1CW=2), (2CCW=3), (2CW=4), (3CCW=5), (3CW=6), (0CCW=7)
        if (clockwise)
            return fromRot switch { 0 => 0, 1 => 2, 2 => 4, 3 => 6, _ => 0 };
        else
            return fromRot switch { 1 => 1, 0 => 7, 3 => 5, 2 => 3, _ => 0 };
    }

    public static readonly Color[] PieceColors = new Color[]
    {
        new Color(0.00f, 0.90f, 0.90f), // I — cyan
        new Color(0.95f, 0.85f, 0.00f), // O — yellow
        new Color(0.65f, 0.00f, 0.85f), // T — purple
        new Color(0.00f, 0.80f, 0.10f), // S — green
        new Color(0.95f, 0.10f, 0.10f), // Z — red
        new Color(0.10f, 0.20f, 0.90f), // J — blue
        new Color(0.95f, 0.50f, 0.00f), // L — orange
    };

    static Vector2Int V(int x, int y) => new Vector2Int(x, y);

    public static Vector2Int[] GetCells(TetrominoType type, int rotation)
        => Cells[(int)type][rotation & 3];

    public static Vector2Int[] GetKickOffsets(TetrominoType type, int fromRot, bool clockwise)
    {
        int idx = KickIndex(fromRot, clockwise);
        return type == TetrominoType.I ? WallKicksI[idx] : WallKicksJLSTZ[idx];
    }
}
