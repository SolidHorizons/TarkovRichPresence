using System.Text.RegularExpressions;
using TarkovRichPresence;

public static class RegexController
{
    public enum REGEX_FLAG
    {
        acc, //used for account id
        map, //used to check what map is currently on
        trader, //used to check which trader is talked to
        menu, //used to check which menu screen a user is in.

    }
    public static Regex RE_MAP = new Regex(@"scene preset path:maps/(?<mapname>.+?)_preset\.bundle"); //application log
    public static Regex RE_TALKING_TO_TRADER = new Regex(@"getTraderAssort/(?<traderid>[0-9a-fA-F]{24})"); //backend log
    public static Regex RE_ACCOUNT_ID = new Regex(@"AccountId:(?<accountid>\d+)"); //application log
    public static Regex RE_INSURANCE_SCREEN = new Regex(@"client/insurance/items/list/cost"); //backend log

    public static Dictionary<string, string> TraderIDTranslation = new Dictionary<string, string>{
        {"54cb50c76803fa8b248b4571", "Prapor"},
        {"54cb57776803fa99248b456e", "Therapist"},
        {"579dc571d53a0658a154fbec", "Fence"},
        {"58330581ace78e27b8b10cee", "Skier"},
        {"5935c25fb3acc3127c3d8cd9", "Peacekeeper"},
        {"5a7c2eca46aef81a7ca2145d", "Mechanic"},
        {"5ac3b934156ae10c4430e83c", "Ragman"},
        {"5c0647fdd443bc2504c2d371", "Jaeger"},
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

    public static void HandleMatch(KeyValuePair<REGEX_FLAG,string> match)
    {
        if (string.IsNullOrEmpty(match.Value))
            return;

        switch (match.Key)
        {
            case REGEX_FLAG.trader:
                string traderName = DecodeTrader(match.Value.Split('/')[1]);
                FileLogger.Log("[RegexController] " + match.Key + ": " + traderName);
                RPCManager.getInstance.setDiscordRpcStatus(
                    traderName,
                    trader =>
                    {
                        TraderConversation? conversation = TarkovRPStates.GetTraderConversation(trader);

                        if (conversation == null)
                        {
                            Console.WriteLine($"Trader conversation '{trader}' not found in TarkovRPStates.");
                            return null;
                        }

                        return RPCManager.getInstance.CreateTraderConversationPresence(conversation);
                    });
                break;
            case REGEX_FLAG.acc:
                FileLogger.Log("[RegexController] " + match.Key + ": " + match.Value); // It eees wat it eeesss
                break;
            case REGEX_FLAG.map:
                FileLogger.Log("[RegexController] " + match.Key + ": " + match.Value.Split("/")[1].Split("_")[0]);
                RPCManager.getInstance.setDiscordRpcStatus(
                    match.Value.Split("/")[1].Split("_")[0],
                    location =>
                    {
                        Location? loc = TarkovRPStates.GetLocation(location);

                        if (loc == null)
                        {
                            Console.WriteLine($"Location '{location}' not found in TarkovRPStates.");
                            return null;
                        }

                        return RPCManager.getInstance.CreateLocationPresence(loc);
                    });
                break;
            case REGEX_FLAG.menu:
                FileLogger.Log("[RegexController] " + match.Key + ": " + match.Value);
                    RPCManager.getInstance.setDiscordRpcStatus(
                    match.Value.Split("/")[1].Split("_")[0],
                    menuScreen =>
                    {
                        MenuScreen? screen = TarkovRPStates.GetMenuScreen(menuScreen);

                        if (screen == null)
                        {
                            Console.WriteLine($"Location '{menuScreen}' not found in TarkovRPStates.");
                            return null;
                        }

                        return RPCManager.getInstance.CreateMenuScreenPresence(screen);
                    });
                break;
            default:   
                FileLogger.Log("[RegexController] unknown key: " + match.Value);
                return;
        }

        

        // RPCManager.setDiscordRpcStatus();
    }
    
}