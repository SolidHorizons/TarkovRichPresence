using System.Text.RegularExpressions;

public static class RegexController
{
    public static Regex RE_MAP = new Regex(@"scene preset path:maps/(?<mapname>[^_]+)_preset\.bundle"); //application log
    public static Regex RE_TALKING_TO_TRADER = new Regex(@"getTraderAssort/(?<traderid>[0-9a-fA-F]{24})"); //backend log
    public static Regex RE_ACCOUNT_ID = new Regex(@"AccountId:(?<accountid>\d+)"); //application log

    public static Dictionary<string, string> TraderIDTranslation = new Dictionary<string, string>{
        {"54cb50c76803fa8b248b4571", "Prapor"},
        {"58330581ace78e27b8b10cee", "Therapist"},
        {"5935c25fb3acc3127c3d8cd9", "Fence"},
        {"54cb57776803fa99248b456e", "Skier"},
        {"579dc571d53a0658a154fbec", "Peacekeeper"},
        {"5a7c2eca46aef81a7ca2145d", "Mechanic"},
        {"5ac3b934156ae10c4430e83c", "Ragman"},
        {"5c0647fdd443bc2504c2d371", "Jeager"},
        {"6617beeaa9cfa777ca915b7c", "Ref"}
    };

    public static string CheckRegexOnLine(Regex re, string line)
    {
        if(line is not null)
        {
            return re.Match(line).ToString();   
        }
        return null!;
    }

    public static string DecodeTrader(string traderid)
    {
        return TraderIDTranslation[traderid];
    }

    public static void HandleMatch(KeyValuePair<string,string> match)
    {
        
    }
    
}