Catalogue Raisonne

This was the project I learned C# with.  The original goal was to create a clone of the Reiner Knizia board game, "Lost Cities."  The intended genre shift was from the archaelogical expeditions of the original game to collecting famous paintings within the rough groupings of Nature (green), Wealth (yellow), Religion (white), Violence (red), and Portraiture (blue).

I stopped working on the project in July, 2011.  The game works (entirely through the command line) but I lost motivation for adding a polished GUI to the client using a dead-end technology (WPF).  I also realized that, for employment demo purposes, I should be concentrating on demonstrable C++ code in the short term.  I might return to this some day.

Ultimately, I would love to branch out of the C/C++/C#/Java school of programming, but for now, that's what dominates the traditional gaming industry, so that's what I need to focus on.

Some "highlights" of the project:

Custom network message serialization solution that is built dynamically at init time (by emitting CIL) that leads to performance that is both an order of magnitude faster and more compact than any of the built-in .NET solutions.

Heavy reflection use (at init-time only) to connect "command" objects with handler delegates, while retaining type safety.

Primitive concurrency with a handful of fixed threads per process, using message-passing with no shared state.  This setup was the precursor to my more general approach to concurrency that can be seen in the CCGOnline project.

Slash command system from which everything is controllable; supported by a full help system

Server services: chat, lobbies, primitive matchmaking

Logging and log management services with exception handling, a precursor to what exists in CCGOnline

Localization ready (all text in a string table)


