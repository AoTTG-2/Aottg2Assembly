using UI;
using Settings;
using Characters;

namespace GameManagers
{
    class PlayerProperty
    {
        public static string DisplayName = "DisplayName";
        public static string DisplayGuild = "DisplayGuild";
        public static string Status = "Status";
        public static string CharacterViewId = "CharacterViewId";
        public static string Character = "Character";
        public static string CustomMapHash = "CustomMapHash";
        public static string CustomLogicHash = "CustomLogicHash";
        public static string Team = "Team";
        public static string Loadout = "Loadout";
        public static string Kills = "Kills";
        public static string Deaths = "Deaths";
        public static string HighestDamage = "HighestDamage";
        public static string TotalDamage = "TotalDamage";
    }

    class PlayerStatus
    {
        public static string Alive = "Alive";
        public static string Dead = "Dead";
        public static string Spectating = "Spectating";
    }

    class PlayerCharacter
    {
        public static string Human = "Human";
        public static string Titan = "Titan";
        public static string Shifter = "Shifter";
    }

    class HumanLoadout
    {
        public static string Blades = "Blades";
        public static string Guns = "Guns";
        public static string ThunderSpears = "ThunderSpears";
    }

    class TeamInfo
    {
        public static string Blue = "Blue";
        public static string Red = "Red";
        public static string None = "None";
        public static string Titan = "Titan";
        public static string Human = "Human";

        public static bool SameTeam(string a, string b)
        {
            return a == b && a != None && b != None;
        }

        public static bool SameTeam(BaseCharacter a, BaseCharacter b)
        {
            return SameTeam(a.Team, b.Team);
        }
    }
}
