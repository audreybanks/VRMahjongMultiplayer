using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour, IComparable<Tile>, IEquatable<Tile> {
   public int value; // 0-9 Man, 10-19 Pin, 20-29 Sou, 31-37 Honors: E, S, W, N, Haku, Hatsu, Chun
   public string tileName;

   public int CompareTo(Tile otherTile) {
        if (otherTile == null) {
            return 1;
        } else if (value > otherTile.value) {
            return 1;
        } else if (value < otherTile.value) {
            return -1;
        } else {
            return 0;
        }
    }

    public bool Equals(Tile otherTile) {
        if (otherTile) {
            return false;
        }
        return this.CompareTo(otherTile) == 0;
    }

    public override bool Equals(System.Object obj) {
        if (obj == null) {
            return false;
        }

        Tile tile = obj as Tile;
        if (tile == null) {
            return false;
        } else {
            //TODO: check if this tile is a 5 or 0 tile, and check if other tile is 5 or 0 tile
            return this.CompareTo(tile) == 0;
        }
    }

    public static bool operator ==(Tile tile1, Tile tile2) {
        if (((object)tile1) == null || ((object)tile2) == null) {
            return System.Object.Equals(tile1, tile2);
        }
        return tile1.Equals(tile2);
    }

    public static bool operator !=(Tile tile1, Tile tile2) {
        return !tile1.Equals(tile2);
    }

    public override int GetHashCode() {
        return this.value.GetHashCode();
    }


    public static bool operator >(Tile tile1, Tile tile2) {
        return tile1.CompareTo(tile2) > 0;
    }

    public static bool operator <(Tile tile1, Tile tile2) {
        return tile1.CompareTo(tile2) < 0;
    }

    public static bool operator <=(Tile tile1, Tile tile2) {
        return tile1.CompareTo(tile2) <= 0;
    }

    public static bool operator >=(Tile tile1, Tile tile2) {
        return tile1.CompareTo(tile2) >= 0;
    }
}
