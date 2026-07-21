namespace TarkovRichPresence;

class Location
{
    public string Name { get; init; } = string.Empty; // Location like "Customs", "Shoreline" but could also refer to "Main Menu" or "Loading Screen"
    public string LocationImage { get; init; } = string.Empty; // Image key for the location, used for the large image in Discord Rich Presence
    public string State { get; init; } = string.Empty; // State like "In Raid", "In Lobby", "In Menu" or "Loading"
    public int MaxRaidTimeInSeconds { get; init; } = 0; // Max raid time in seconds, used for the progress bar, when 0 assume location like stash which does not have a time limit
}

class TraderConversation
{
    public string Name { get; init; } = string.Empty;
    public string TraderImage { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
}

class TarkovRPStates
{
    private static readonly Dictionary<string, Location> _locations = new()
    {
        ["mainmenu"] = new Location { Name = "Main Menu", State = "In Menu", LocationImage = "banner_hideout", MaxRaidTimeInSeconds = 0 },
        ["lobby"] = new Location { Name = "Lobby", State = "In Lobby", LocationImage = "banner_hideout", MaxRaidTimeInSeconds = 0 },
        ["loading"] = new Location { Name = "Loading Into Raid", State = "Doomscrolling Time", LocationImage = "banner_hideout", MaxRaidTimeInSeconds = 0 },
        ["customs"] = new Location { Name = "Customs", State = "In Raid", LocationImage = "banner_customs", MaxRaidTimeInSeconds = 2400 },
        ["shoreline"] = new Location { Name = "Shoreline", State = "In Raid", LocationImage = "banner_shoreline", MaxRaidTimeInSeconds = 2700 },
        ["woods"] = new Location { Name = "Woods", State = "In Raid", LocationImage = "banner_woods", MaxRaidTimeInSeconds = 2400 },
        ["interchange"] = new Location { Name = "Interchange", State = "In Raid", LocationImage = "banner_interchange", MaxRaidTimeInSeconds = 2400 },
        ["factory"] = new Location { Name = "Factory (Day)", State = "In Raid", LocationImage = "banner_factory_day", MaxRaidTimeInSeconds = 1200 }, // Fix for future (or never idc)
        // ["factory4day"] = new Location { Name = "Factory (Day)", State = "In Raid", LocationImage = "banner_factory_day", MaxRaidTimeInSeconds = 1200 },
        // ["factory4night"] = new Location { Name = "Factory (Night)", State = "In Raid", LocationImage = "banner_factory_night", MaxRaidTimeInSeconds = 1500 },
        ["reserve"] = new Location { Name = "Reserve", State = "In Raid", LocationImage = "banner_reserve", MaxRaidTimeInSeconds = 2400 },
        ["labsday"] = new Location { Name = "Labs", State = "In Raid", LocationImage = "banner_the_lab", MaxRaidTimeInSeconds = 1800 },
        ["labsnight"] = new Location { Name = "Labs", State = "In Raid", LocationImage = "banner_the_lab", MaxRaidTimeInSeconds = 1800 },
        ["lighthouse"] = new Location { Name = "Lighthouse", State = "In Raid", LocationImage = "banner_lighthouse", MaxRaidTimeInSeconds = 1800 },
        ["streets"] = new Location { Name = "Streets", State = "In Raid", LocationImage = "banner_streets", MaxRaidTimeInSeconds = 2400 },
        ["groundzero"] = new Location { Name = "Ground Zero", State = "In Raid", LocationImage = "banner_ground_zero", MaxRaidTimeInSeconds = 3000 },
        ["labyrinth"] = new Location { Name = "Labyrinth", State = "In Raid", LocationImage = "banner_the_labyrinth", MaxRaidTimeInSeconds = 1800 }
    };

    private static readonly Dictionary<string, TraderConversation> _traders = new()
    {
        ["prapor"] = new TraderConversation { Name = "Prapor", State = "Making deals", TraderImage = "Placeholder"},
        ["therapist"] = new TraderConversation { Name = "Therapist", State = "Exchanging medicin", TraderImage = "Placeholder"},
        ["fence"] = new TraderConversation { Name = "Fence", State = "Shady bussiness", TraderImage = "Placeholder"},
        ["skier"] = new TraderConversation { Name = "Skier", State = "Hauling cargo", TraderImage = "Placeholder"},
        ["peacekeeper"] = new TraderConversation { Name = "Peacekeeper", State = "Discussing treatys", TraderImage = "Placeholder"},
        ["mechanic"] = new TraderConversation { Name = "Mechanic", State = "Making arms deals", TraderImage = "Placeholder"},
        ["ragman"] = new TraderConversation { Name = "Ragman", State = "Getting drip", TraderImage = "Placeholder"},
        ["jaeger"] = new TraderConversation { Name = "Jaeger", State = "Getting camping gear", TraderImage = "Placeholder"},
        ["ref"] = new TraderConversation { Name = "Ref", State = "Signing arena contract", TraderImage = "Placeholder"}
    };

    public static Location? GetLocation(string locationKey)
    {
        if (string.IsNullOrWhiteSpace(locationKey))
        {
            return null;
        }

        return _locations.GetValueOrDefault(locationKey.Trim().ToLowerInvariant());
    }

    public static TraderConversation? GetTraderConversation(string convoKey)
    {
        if (string.IsNullOrWhiteSpace(convoKey))
        {
            return null;
        }

        return _traders.GetValueOrDefault(convoKey.Trim().ToLowerInvariant());
    }
}