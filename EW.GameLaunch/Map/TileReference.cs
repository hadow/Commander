
namespace EW
{

    public struct TerrainTile
    {
        public readonly ushort Type;
        public readonly byte Index;

        public TerrainTile(ushort type,byte index)
        {
            Type = type;
            Index = index;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Index.GetHashCode();
        }
    }

    public struct ResourceTile
    {
        public readonly byte Type;
        public readonly byte Index;

        public ResourceTile(byte type,byte index)
        {
            Type = type;
            Index = index;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Index.GetHashCode();
        }
    }

}