# 🛡️ Moderator Bot

A lightweight, modern Discord moderation bot built with **C# (.NET 10)** using **Discord.Net** and Discord Slash Commands (`InteractionService`).

---

## ⚡ Features & Slash Commands

All moderation actions respond with ephemeral (hidden) messages to keep your staff actions discrete.

| Command | Description | Permissions Required |
| :--- | :--- | :--- |
| `/kick <user> [reason]` | Kicks a specified member from the server. | `Kick Members` |
| `/ban <user> [reason]` | Bans a specified member from the server. | `Ban Members` |
| `/unban <user> [reason]` | Unbans a user. Features **live autocomplete** from the server's ban list. | `Ban Members` |
| `/timeout <user> <minutes> [reason]` | Placed a member in timeout for a set number of minutes. | `Moderate Members` |
| `/untimeout <user> [reason]` | Removes an active timeout from a member. | `Moderate Members` |

---

## 🛠️ Tech Stack & Requirements

- **Framework:** .NET 10.0
- **Library:** [Discord.Net](https://github.com/discord-net/Discord.Net) (v3.20.1)
- **Features:** Gateway Intents, Interaction Framework (Slash Commands & Autocomplete Handlers)

---

## ⚙️ Configuration Setup

1. Rename or create a `config.json` file in the root directory alongside your `.csproj` file:

```json
{
  "Token": "YOUR_DISCORD_BOT_TOKEN_HERE"
}
