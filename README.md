Arcmage Online
==============

Arcmage Online is a card database, a deckbuilder and an online browser-based implementation of a CCG being developed as part 
of the [Arcmage project](https://arcmage.org). 

It offers a virtual one-to-one table top for playing the game without rule enforcing, and without the need for any plugins or player registration.

The game has no artificial intelligence nor are there any plans to implement game rules.

Furthermore, Arcmage Online offers a card generation webiste to create new cards 
and decks.

Repository Structure
--------------------

The respository is used for both the card db/generation as for the online game

Common projects (Client and Server)
* Arcmage.Model : The public model for cards, decks, games, etc ...

Client projects
* Arcmage.Client : A custom strong typed c# http client for easy access to the Arcmage API.

Api folders
* Arcmage.Configuration : A small settings library to load some settings from json
* Arcmage.DAL : The database orm layer for use with Entity Framework Core
* Arcmage.Seed : A database seeder to populate an empty database with some basic data (Sets, Factions, ...)
* Arcmage.InputConvertor : A library to convert our card layout xml field to markdown or vise versa
* Arcmage.Server.Api : The Rest API for Authentication, Card Creation, Deck Creation, Card Searching, ... (also hosts the Matrix Matchbot)
* Arcmage.Web : Holds the angular webapp for card and deck creation, and the vue app for the online game

Game folders
* Arcmage.Game.Api : The singalR hub backend for the browser to browser communication

Matchbot (matrix.org complient bot)
* Matrix.Sdk.Api : A fork from https://github.com/VRocker/MatrixAPI under Apache License, Version 2.0, January 2004, http://www.apache.org/licenses/
* Arcmage.Matrix.Matchbot : a simple matrix bot to facilitate player matchups and to create an arcmage game from within a matrix room
* Arcmage.Matrix.Matchbot.Console : a console app to host the matchbox locally  

tools
* ApiExamples : A small demo on how to use the Arcmage.Client to interact with the Arcmage API
* DeckAutoCompleter : A tool that extracs and updates the card's 'RuleText' and 'FlavourText' fields from the 'LayoutText' markup field for a given deck.
* DeckTranslator : A tool that copies cards from one language to a new language for a given deck. It marks the cards as work in progress and copies the artwork, fills in the cards' translated Subtype, Info, ...
* ProductGenerator : A tool that exports cards/decks in a woocommerce compliant file so that it can be imported in our webshop.

Build and hosting on windows
============================

Build requirements
------------------

- .NET 5 sdk or above, see https://dotnet.microsoft.com/en-us/download/dotnet/6.0
- .ASP.NET Core Runtime 5 or above, see https://dotnet.microsoft.com/en-us/download/dotnet/6.0
- ef core tools, see https://docs.microsoft.com/en-us/ef/core/cli/dotnet
- Visual Studio 2019 Community Edition (or above),see https://visualstudio.microsoft.com/vs/community/
- Visual Studio Code, see https://code.visualstudio.com/
- nodejs, see https://nodejs.org/en/download/
- inkscape 0.91, 1.0.2 or above, see https://inkscape.org/
- SQL Server Express 2019 or above (or any database with support for entity framework core, e.g. MySQL), see https://www.microsoft.com/nl-be/sql-server/sql-server-downloads
- git, see https://git-scm.com/downloads

Check inkscape install
----------------------

* open a command prompt and navigate to C:\Program Files\Inkscape
* run 'inkscape.exe -V' to see the inkscape version en remember the version

Getting the code
----------------

* open a command prompt and navigate to a desired checkout location
* use 'git clone https://github.com/wtactics/arcmage.git' to get the code

Configuring the applications settings
-------------------------------------

A sample application settings file looks like

```
{
  "App": {
    "ArcmageConnectionString": "Server=localhost;Database=Arcmage;User Id=<db-user>;Password=<db-password>",
    "RepositoryRootPah": "wwwroot",
    "TokenEncryptionKey": "<your 20 char token>",
    "PortalUrl": "http://localhost:5000",
    "ApiUrl": "http://localhost:5000",
    "ApiListenUrls": "http://*:5000",
    "InkscapeExe": "<path-to-inkscape-executable>",
    "InkscapeVersion": "1.0.2",
    "GameApiUrl": "http://localhost:5090",
    "GameApiListenUrls": "http://*:5090",
    "SendGridApiKey": ""
  }
}
```

Edit ***both*** the Acrmage.Game.Api\appsettings.json and Arcmage.Server.Api\appsettings.json files
   * Update the Arcmage connection strings to point to your database provider
   * Update the TokenEncryptionKey with your own custom generated 20 char key, to encrypt login tokens
   * Update the InkscapeExe path to 'C:\Program Files\Inkscape\inkscape.exe' 
   * Update the InkscapeVersion to '1.0.2' (or leave empty to use the 0.9.x command line syntax)
   * Update the SendGridApiKey to use sendgrid to validate signup registrations, or leave empty

In case you need to run the inkscape export process with elevated permissions or under a certain system user (windows only)
   * Update the ForceInkscapeUserImpersonate
   * Fill in InkscapeUser 
   * Fill in InkscapePassword

Advanced settings  (see Arcmage.Configuration/Settings.cs for all options)

   * The game runtime can be run stand alone or the api can start/stop the game runtime on request
   * The api can host the matrix matchmaking bot (arcbot)

Building and run the api
------------------------

1. Build the solution
   * The first time build will fetch all the required nuget packages
2. Open a command line at the Arcmage.DAL folder and run 'dotnet ef database update'
   * This will create the an empty database with the correct tables
3. Set Arcmage.Server.Api as startup project and run (use the project launch, not IIS Express)

Seeding the database
--------------------

1. Edit Acrmage.Seed\appsettings.cs and update the ServiceUser fields with your user name, email and password.
   * This will be the default admin user
2. Run the Arcmage.Seed 
   * In Visual Studio, right click the Arcmage.Seed > Debug > Start New Instance
   * Or use the alternative 'dotnet run' command to start the Arcmage.Seed program
3. Remove the  ServiceUser fields (no longer needed)

Settings tip for contributers
-----------------------------

If you'd like to contribute the code base, but keep your settings from being committed, use alternative 'appsettings_development.json' files instead.
(You can still leave the appsettings.json files as is)

Building the angular webapp
---------------------------

1. open the Arcmage.Web folder in Visual Studio Code 
2. open a new terminal
3. run 'npm install'
    * This will fetch all required npm packages
4. run 'npm run build'
    * This will build the angular app (and game vue js app) and output it in the './dist' folder
	* A post build script will copy the './dist' contents to Arcmage.Server.Api/wwwroot (this is done so we can host the web apps and the server api in a single Kestrel web server)
	
While the Arcmage.Server.API is actice browse to http://localhost:5000

Running the game api
--------------------

The game api is a standalone dotnet core app, using signalR hubs to allow browser to browser communication for the online game.

While the Arcmage.Server.Api is running, start the Arcmage.Game.Api as well 
* In Visual Studio, right click the Arcmage.Game.Api > Debug > Start New Instance
* Or use the alternative 'dotnet run' command to start the Arcmage.Game.Api program

Build and hosting on linux
==========================

Build requirements for ubuntu 21.10
-----------------------------------

* .NET 5 sdk or above, see https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu
* .ASP.NET Core Runtime 5 or above, see https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu
* ef core tools, see https://docs.microsoft.com/en-us/ef/core/cli/dotnet
* Visual Studio Code, see https://code.visualstudio.com/docs/setup/linux#_debian-and-ubuntu-based-distributions
* nodejs, see https://github.com/nodesource/distributions/blob/master/README.md#debinstall
* npm, see https://www.digitalocean.com/community/tutorials/how-to-install-node-js-on-ubuntu-20-04
* inkscape 0.91, 1.0.2 or above, see https://inkscape.org/release/inkscape-1.0/gnulinux/ubuntu/ppa/dl/
* SQL Server Express 2019 or above (or any database with support for entity framework core, e.g. MySQL), see https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-ubuntu?view=sql-server-ver15
* git, see https://git-scm.com/download/linux

optionally install sqlpackage, 
 * see https://docs.microsoft.com/en-us/sql/tools/sqlpackage/sqlpackage-download?view=sql-server-ver15#get-sqlpackage-net-core-for-linux
 * allows you to import/export to/from .bacpac database backup files


Build requirements for debian 11
--------------------------------

* .NET 5 sdk or above, see https://docs.microsoft.com/en-us/dotnet/core/install/linux-debian
* .ASP.NET Core Runtime 5 or above, see https://docs.microsoft.com/en-us/dotnet/core/install/linux-debian
* ef core tools, see https://docs.microsoft.com/en-us/ef/core/cli/dotnet
* Visual Studio Code, see https://code.visualstudio.com/docs/setup/linux#_debian-and-ubuntu-based-distributions
* nodejs, see https://github.com/nodesource/distributions/blob/master/README.md#debinstall
* npm, see https://www.digitalocean.com/community/tutorials/how-to-install-node-js-on-debian-10
* inkscape 0.91, 1.0.2 or above, see https://wiki.inkscape.org/wiki/Installing_Inkscape
* SQL Server Express 2019 or above (or any database with support for entity framework core, e.g. MySQL), see https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-ubuntu?view=sql-server-ver15
* git, see https://git-scm.com/download/linux

optionally install sqlpackage, 
 * see https://docs.microsoft.com/en-us/sql/tools/sqlpackage/sqlpackage-download?view=sql-server-ver15#get-sqlpackage-net-core-for-linux
 * allows you to import/export to/from .bacpac database backup files



Check inkscape install
----------------------

* open a shell
* run '/bin/inkscape -V' to see the inkscape version en remember the version

Getting the code
----------------

* open a shell and navigate to a desired checkout location
* use 'git clone https://github.com/wtactics/arcmage.git' to get the code

Configuring the applications settings
-------------------------------------

A sample application settings file looks like

```
{
  "App": {
    "ArcmageConnectionString": "Server=localhost;Database=Arcmage;User Id=<db-user>;Password=<db-password>",
    "RepositoryRootPah": "wwwroot",
    "TokenEncryptionKey": "<your 20 char token>",
    "PortalUrl": "http://localhost:5000",
    "ApiUrl": "http://localhost:5000",
    "ApiListenUrls": "http://*:5000",
    "InkscapeExe": "<path-to-inkscape-executable>",
    "InkscapeVersion": "1.0.2",
    "GameApiUrl": "http://localhost:5090",
    "GameApiListenUrls": "http://*:5090",
    "SendGridApiKey": ""
  }
}
```

Edit ***both*** the Acrmage.Game.Api\appsettings.json and Arcmage.Server.Api\appsettings.json files
   * Update the Arcmage connection strings to point to your database provider
   * Update the TokenEncryptionKey with your own custom generated 20 char key, to encrypt login tokens
   * Update the InkscapeExe path to '/bin/inkscape' 
   * Update the InkscapeVersion to '1.0.2' (or leave empty to use the 0.9.x command line syntax)
   * Update the SendGridApiKey to use sendgrid to validate signup registrations, or leave empty

Advanced settings  (see Arcmage.Configuration/Settings.cs for all options)

   * The game runtime can be run stand alone or the api can start/stop the game runtime on request
   * The api can host the matrix matchmaking bot (arcbot)

Building and run the api
------------------------

0. open a shell
1. navigate to the checkout location and run 'dotnet build Arcmage.sln'
   * The first time build will fetch all the required nuget packages
2. navigate to the Arcmage.DAL folder and run 'dotnet ef database update'
   * This will create the an empty database with the correct tables
3. navigate to the Arcmage.Server.Api folder and run 'dotnet ./bin/Debug/net5.0/Arcmage.Server.Api.dll'

Seeding the database
--------------------

While the api is running

0. Edit Acrmage.Seed\appsettings.cs and update the ServiceUser fields with your user name, email and password.
   * This will be the default admin user
1. open a shell
2. navigate the Arcmage.Seed folder and run 'dotnet ./bin/Debug/net5.0/Arcmage.Seed.dll'
3. Remove the  ServiceUser fields (no longer needed)

Settings tip for contributers
-----------------------------

If you'd like to contribute the code base, but keep your settings from being committed, use alternative 'appsettings_development.json' files instead.
(You can still leave the appsettings.json files as is)

Building the angular webapp
---------------------------

1. open the Arcmage.Web folder in Visual Studio Code 
2. open a new terminal
3. run 'npm install'
    * This will fetch all required npm packages
4. run 'npm run buildlinux'
    * This will build the angular app (and game vue js app) and output it in the './dist' folder
	* A post build script will copy the './dist' contents to Arcmage.Server.Api/wwwroot (this is done so we can host the web apps and the server api in a single Kestrel web server)
	
While the Arcmage.Server.API is actice browse to http://localhost:5000

Running the game api
--------------------

The game api is a standalone dotnet core app, using signalR hubs to allow browser to browser communication for the online game.

While the Arcmage.Server.Api is running, start the Arcmage.Game.Api as well 
1. open a shell
2. navigate the Arcmage.Game.Api folder and run 'dotnet ./bin/Debug/net5.0/Arcmage.Game.Api.dll'


Where are the cards/decks?
==========================

You'll notice that http://localhost:5000 doesn't show any cards/decks.
This software stack comes with no cards or artwork. However you can create your own cards/decks once logged in.
Generated cards will be at Server.Arcmage.Api\wwwwroot\arcmage\Cards
Feel free to use our great CC-BY-SA4 artwork repository!. 

Card Templates
==============

Our card svg templates are at Server.Arcmage.Api\wwwroot\arcmage\CardTemplates

They are per Faction and per Type. Each template combination has the several files
1. (template).svg : the real full blown template, used to generate the cards in high resolution png and pdf files
2. (template).png : used as background while editing the card on the website
3. (template) overlay plain.svg : used as overlay while editing the card on the website  
4. (template) overlay.svg : used for changing the template in inkscape (before saving it as overlay plain)

Furthermore, the border.svg and border.png files are used to create a 2mm bleed around the cards when generating the png and pdfs print ready files.

Resources
=========

If you want to know more about the project please visit the following links or 
read the suggested documents:
- https://aminduna.arcmage.org/ [Arcmage]
- https://aminduna.arcmage.org/#/cards [Arcmage cards]
- https://aminduna.arcmage.org/#/games [Arcmage online game]
