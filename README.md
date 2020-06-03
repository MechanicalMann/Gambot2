# Gambot 2: Electric Boogaloo

_The orphaned daughter shall have her revenge._

![.NET Core](https://github.com/malorisdead/Gambot2/workflows/.NET%20Core/badge.svg?branch=master)

Gambot 2 is a ground-up rewrite of [Gambot](https://github.com/Milk-Enterprises/Gambot) in .NET Core.

## What's a Gambot?

Gambot is a chat bot for Slack and Discord, (very) loosely based on [Bucket](https://github.com/zigdon/xkcd-Bucket/),
the XKCD IRC chat bot.

Gambot can learn factoids, pick up on interesting band names, remember amusing
quotes, jump in on chains of repeated messages, and occasionally glitch out
and mangle a response.

Gambot is designed to be modular (though it's not _quite_ there yet) so other
features should be relatively easy to add.

Gambot is written in C# and designed to run anywhere .NET Core can be installed.

## Getting Started

To run Gambot, you will need a machine with the .NET Core runtime version 2.1
or higher installed.

```bash
git clone https://github.com/malorisdead/Gambot2.git
cd Gambot2/src
dotnet restore
dotnet build
```

You can then run Gambot using

```bash
dotnet run Gambot.Bot
```

## Documentation

Please see the [wiki](https://github.com/malorisdead/Gambot2/wiki) for all your
documentation needs.

Here are some good places to start.

### Using Gambot
- [Interacting with Gambot](https://github.com/malorisdead/Gambot2/wiki#interacting-with-gambot)
- [Pronoun Preferences](https://github.com/malorisdead/Gambot2/wiki/Pronoun-Preferences)
- [Factoids](https://github.com/malorisdead/Gambot2/wiki/Factoids)
- [Variables](https://github.com/malorisdead/Gambot2/wiki/Variables)

### Running Your Own Gambot

- [Configuration](https://github.com/malorisdead/Gambot2/wiki/Configuration)
- [Discord Setup](https://github.com/malorisdead/Gambot2/wiki/Connecting-to-Discord)
- [Slack Setup](https://github.com/malorisdead/Gambot2/wiki/Connecting-to-Slack)

## Development

Right now, Gambot development is primarily taking place in this repository.
If you're interested in module development, there are plenty of examples to
start from.

Issues and pull requests are being accepted, but please bear in mind that this
is a side project and not my day job (unfortunately?).

---

[...Why?](https://gunshowcomic.com/513)