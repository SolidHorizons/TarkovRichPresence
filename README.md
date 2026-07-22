# Tarkov Rich Presence
Tarkov Rich Presence is a lightweight background tool that runs in the application tray which updates your Discord Rich Presence to whatever you are doing in Tarkov, so rather than it saying "Playing Escape from Tarkov" it now gives information regarding your current status! The application was designed to be lightweight and sit idle in the background since we all know Tarkov is a difficult game to run that requires a lot of resources. 

## Current features
### 1. Process detection
The application currently tracks whether you are playing Escape from Tarkov or not, making sure not to display any Rich Presence when you are not actively playing Tarkov!
### 2. Tarkov.dev integration
Some metrics we cannot get out of the logs Tarkov provides, so this information we get from the Tarkov.dev API. This allows us to show things like your player level, your PMC (Bear, USEC) and possibly your game edition!
### 3. Non-invasive log tracking
We do not bind to any processes spun up by tarkov or the anti-cheat, we only look at log files to determine what is currently happening. 
### 4. Trader rich presence tracking
The application tracks which trader you are talking to and displays this.
### 5. Raid status tracking
We accuratly display what map you are currently playing and display how long you have left in the raid on your profile  

## Known quirks
### Flea market tracking
Currently we are working on fixing the tracking of flea market, due to how tarkov does logging of it