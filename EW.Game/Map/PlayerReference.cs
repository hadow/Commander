
namespace EW
{
    public class PlayerReference
    {
        public string Name;

        public bool Playable = false;
        public PlayerReference(MiniYaml my)
        {
            FieldLoader.Load(this, my);
        }
    }
}