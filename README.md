Last Updated: 19th March 2025

# Project: Kuroko (.NET 9 Edition)
Kuroko is a Discord Bot, built upon Microsoft's .NET 9 runtime & Entity Framework 9. The bot is currently under "Early Access" release and is actively being worked on.
If you wish to acquire the bot for your server to help test it's feature-sets, feel free to join our Discord server via https://discord.gg/fFbMNGbXVv to provide feedback.

Link to Kuroko: https://discord.com/oauth2/authorize?client_id=1115368540124500018

## Current Feature Sets
### True BanSync
BanSync allows 2+ servers to communicate with each other to provide notifications of bans occurring. The relationship established is in the form of "Host-Client" relationship.
Every relationship made is considered unique. Therefore, 1 server can act as both host & client. The system has no centralized banlist database and solely relies on Discord server banlists instead.
Hence the term, "True BanSync". Plus as a result of this, we can stay within Discord's ToS as well as offer different levels of "Opt-In" via the usage of Sync Modes.

Sync Modes are the following:
* Disabled - No Sync
* Default - Client obeys what mode is set by default on Host
* Simplex - Client can read Host banlist
* Half Duplex
  * Client can read Host banlist & Client can send ban warnings to Host.
  * Under Client mode, the client will accept warnings from all hosts at this time. May be changed in future to allow per-server perms
* Full Duplex
  * Client can read Host banlist & Client can send ban commands to Host.
  * Under client mode, the Host can run ban commands on Client.
  * This mode should **NEVER** be used unless networked with trusted sources
### Patreon Integration
You can now support us via Patreon if you like using our bot. By subscribing, you get access to redeem premium keys which you can use on any server as long as the bot is present ofcourse. 
You do not need to be an admin or mod for that server either. Our Patreon page: https://www.patreon.com/nekoputer

## Currently Worked-On
* Monitoring backend & logs for now while bugfixing

## Expected Future Re-Works
* Blackbox Recorder
* Tickets
* Mod Logs

## Currently Under Consideration
* Music Playback
* Websocket-Based API

## Not Yet Ready
* Terms of Service
* Privacy Policy
* Website
