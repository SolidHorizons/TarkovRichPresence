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
        matching, //used to check when we start loading into a raid
        raidStarted, //used to check when a raid has actually started
        raidStartedTransit, //alternate check for raid start, via the [Transit] log line
        raidEnded, //used to check when a raid has ended

    }
    public static Regex RE_MAP = new Regex(@"scene preset path:maps/(?<mapname>.+?)_preset\.bundle"); //application log
    public static Regex RE_TALKING_TO_TRADER = new Regex(@"getTraderAssort/(?<traderid>[0-9a-fA-F]{24})"); //backend log
    public static Regex RE_ACCOUNT_ID = new Regex(@"AccountId:(?<accountid>\d+)"); //application log
    public static Regex RE_INSURANCE_SCREEN = new Regex(@"client/insurance/items/list/cost"); //backend log
    public static Regex RE_MATCHMAKING = new Regex(@"TRACE-NetworkGameMatching"); //application log, matchmaking started, raid is loading
    public static Regex RE_RAID_STARTED = new Regex(@"GameStarted:"); //application log, raid has actually started
    public static Regex RE_RAID_STARTED_TRANSIT = new Regex(@"\[Transit\] Flag:\w+, RaidId:[0-9a-fA-F]+, Count:\d+, Locations:"); //application log, also seen firing when a raid starts
    public static Regex RE_RAID_ENDED = new Regex(@"CompleteSelectedProfile"); //application log, raid has ended, back on the way to menu

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
                // The map name is only logged while we're still loading into the raid, so stash it
                // in the RaidStateManager for use once the raid actually starts.
                string mapKey = match.Value.Split("/")[1].Split("_")[0];
                FileLogger.Log("[RegexController] " + match.Key + ": " + mapKey);
                Location? pendingLocation = TarkovRPStates.GetLocation(mapKey);

                if (pendingLocation == null)
                {
                    Console.WriteLine($"Location '{mapKey}' not found in TarkovRPStates.");
                    break;
                }

                RaidStateManager.getInstance.SetPendingLocation(pendingLocation);
                break;
            case REGEX_FLAG.matching:
                FileLogger.Log("[RegexController] " + match.Key + ": matchmaking started, loading into raid");
                RaidStateManager.getInstance.EnterLoading();
                break;
            case REGEX_FLAG.raidStarted:
                FileLogger.Log("[RegexController] " + match.Key + ": raid started");
                RaidStateManager.getInstance.EnterRaid();
                break;
            case REGEX_FLAG.raidStartedTransit:
                // Fires earlier than GameStarted with imprecise timing, so update the phase but
                // let GameStarted be the one that actually pushes the Discord presence/timer.
                FileLogger.Log("[RegexController] " + match.Key + ": raid started (early signal)");
                RaidStateManager.getInstance.EnterRaid(updatePresence: false);
                break;
            case REGEX_FLAG.raidEnded:
                FileLogger.Log("[RegexController] " + match.Key + ": raid ended");
                RaidStateManager.getInstance.EnterRaidEnd();
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