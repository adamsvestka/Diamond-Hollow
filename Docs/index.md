# Diamond Hollow

*by Adam Švestka*

An infinite 2D vertical scrolling game. The player navigates upward over platforms, avoids or kills enemies and collects diamonds.

Based on the Diamond Hollow series of Flash games. Coded in C# using the [MonoGame](https://www.monogame.net) framework.

![Screenshot](Images/screenshot.png)


## Installation

Tested with `.NET 6` and `.NET Core 3.1`

1. Download a version of the source code from [MFF GitLab](https://gitlab.mff.cuni.cz/teaching/nprg031/2022-summer/student-svestka1.git)

    - You can either visit the link above and click on <kbd>
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" width="10" height="10"><path fill="currentColor" fill-rule="evenodd" clip-rule="evenodd" d="M11.78 7.159a.75.75 0 0 0-1.06 0l-1.97 1.97V1.75a.75.75 0 0 0-1.5 0v7.379l-1.97-1.97a.75.75 0 0 0-1.06 1.06l3.25 3.25L8 12l.53-.53 3.25-3.25a.75.75 0 0 0 0-1.061zM2.5 9.75a.75.75 0 0 0-1.5 0V13a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V9.75a.75.75 0 0 0-1.5 0V13a.5.5 0 0 1-.5.5H3a.5.5 0 0 1-.5-.5V9.75z"></path></svg> <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" width="10" height="10"><path fill="currentColor" fill-rule="evenodd" clip-rule="evenodd" d="M4.22 6.22a.75.75 0 0 1 1.06 0L8 8.94l2.72-2.72a.75.75 0 1 1 1.06 1.06l-3.25 3.25a.75.75 0 0 1-1.06 0L4.22 7.28a.75.75 0 0 1 0-1.06z"></path></svg></kbd> > <kbd>zip</kbd> to download the source code
    - Or you can clone the repository using a command: `git clone git@gitlab.mff.cuni.cz:teaching/nprg031/2022-summer/student-svestka1.git`

2. Install .NET

    Make sure you have `.NET 6` and `.NET Core 3.1` installed.

    You can download them here: [https://dotnet.microsoft.com/en-us/download](https://dotnet.microsoft.com/en-us/download)

3. Run the application
    
    ```sh
    cd student-svestka1
    dotnet run
    ```

    *You might have to run this twice; .NET has to build the game resources along with the project and seems to not do this in the correct order sometimes.*


## How to play

You can move the player left/right using the <kbd>A</kbd>/<kbd>D</kbd> keys and jump with <kbd>Space</kbd>.
The player aims in the direction of your mouse and you can fire by left-clicking. Holding down the left mouse button will make the player shoot continuously.

Anything else that moves is an enemy, so shoot them or avoid them.
Your objective is to collect as many diamonds as possible, there is no end to the game.
For more information on enemies and special items, you can collect to aid you, see the [HOW TO PLAY](how-to-play.md).


## Building the docs

Requires [DocFX](https://dotnet.github.io/docfx/index.html). You can [install](https://dotnet.github.io/docfx/tutorial/docfx_getting_started.html#2-use-docfx-as-a-command-line-tool) it with NuGet:

```sh
nuget install docfx.console
```

The executable will be under `docfx.console/tools/` (requires mono).

Then run the following commands to build and serve to docs:

```sh
cd Docs
docfx build
docfx serve
```