# BobDono
Little bot for discord server.

## About

Well it's just a bot written in .NET Core 2.0 for single server use. If for some wicked reason's you'd want to run it on your's you will have to read through source and change some stuffs.
Postgresql is used as DBMS with EntityFrameworkCore being ORM library and NPGSQL is EF driver for postgre.

## Framework

Communication with discord itself is provided by DSharpPlus library. As for framework that processess all commands and whatnot... I wrote it myself. It's pretty simple yet allows to add multiple commands without spaghetti code.
It's based on attributes, `ModuleAttribute` describes class with commands, `CommandHandlerAttribute` defines specific command.
From more interesting things, it's able to create modules per channel so each channel has different context.

## Functionality

As out little discord server is anime oriented this bot allows to track favourite characters, search them etc.
It's main functionality is to organize animebracket-like contests in discord channel.
