using EW.Graphics;
namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public class PlayerReference
    {
        public string Name;
        public string Bot = null;

        public bool Playable = false;
        public bool AllowBots = true;
        public bool Required = false;
        public bool Spectating = false;
        public bool NonCombatant = false;


        public bool LockFaction = false;
        public string Faction;  //派系

        public bool OwnsWorld = false;

        public bool LockColor = false;
        public HSLColor Color = new HSLColor(0, 0, 238);

        public bool LockSpawn = false;
        public int Spawn = 0;

        public bool LockTeam = false;
        public int Team = 0;



        public string[] Enemies = { };
        public string[] Allies = { };   //盟国

        public PlayerReference() { }
        public PlayerReference(MiniYaml my)
        {
            FieldLoader.Load(this, my);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}