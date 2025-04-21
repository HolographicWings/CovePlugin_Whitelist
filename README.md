# CoveWhitelist
 Whitelist plugin for [WebFishingCove](https://github.com/DrMeepso/WebFishingCove) dedicated server.

## Command Usage

Use the `!whitelist` command to manage the server whitelist.

### Enable Whitelist
```bash
!whitelist on
```
Enables the whitelist. Players not on the whitelist will be kicked.

### Disable Whitelist
```bash
!whitelist off
```
Disables the whitelist. Anyone can join the server.

### Add Player to Whitelist
```bash
!whitelist add <SteamID or Username>
```

### Remove Player from Whitelist
```bash
!whitelist remove <SteamID or Username>
```

## Notes
- Only server admins can use these commands.
- Usernames only work if the players are connected (so when the whitelist is off as well).
