# Tarkov Rich Presence
Tarkov Rich Presence is a lightweight background tool that runs in the application tray which updates your Discord Rich Presence to whatever you are doing in Tarkov, so rather than it saying "Playing Escape from Tarkov" it now gives information regarding your current status! The application was designed to be lightweight and sit idle in the background since we all know Tarkov is a difficult game to run that requires a lot of resources. 

## Current features
### 1. Process detection
The application currently tracks whether you are playing Escape from Tarkov or not, making sure not to display any Rich Presence when you are not actively playing Tarkov!
### 2. Tarkov.dev integration
Some metrics we cannot get out of the logs Tarkov provides, so this information we get from the Tarkov.dev API. This allows us to show things like your player level, your PMC (Bear, USEC) and possibly your game edition!
### 3. Non-invasive log tracking
We do not bind to any processes spun up by tarkov or the anti-cheat, we only look at log files to determine what is currently happening. 

## Future features - WIP
### - Trader rich presence tracking
We plan on adding functionalities to be able to track what trader you are interacting with at that moment
### - Raid status tracking
We currently only show whether you are in raid or in the main menu, we plan on expanding this functionality to provide more in-depth information such as (Insuring gear, queueing for raid, etc.)

## Known quirks
### Unset player id
We have not implemented a way to fetch the playerid yet, we are working on this. If you would like to use the tarkov.dev integration you could set the player id manually in the settings file. Please note that the player id has to be a string and not a number/integer. (just wrap your id like so: "<playerid>")