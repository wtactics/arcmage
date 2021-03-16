Arcmage Online
==============

Arcmage Online is a card database, a deckbuilder and an online browser-based implementation of a CCG being developed as part 
of the [Arcmage project](https://arcmage.org). 

It offers a virtual one-to-one table top for playing the game without rule enforcing, and without the need for any plugins or player registration.

The game has no artificial intelligence nor are there any plans to implement game rules.

Furthermore, Arcmage Online offers an card generation webiste to create new cards 
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
* Arcmage.Server.Api : The Rest API for Authentication, Card Creation, Deck Creation, Card Searching, ...
* Arcmage.Web : Holds the angular webapp for card and deck creation, and the vue app for the online game

Game folders
* Arcmage.Game.Api : The singalR hub backend for the browser to browser communication


tools
* ApiExamples : A small demo on how to use the Arcmage.Client to interact with the Arcmage API
* DeckAutoCompleter : A tool that extracs and updates the card's 'RuleText' and 'FlavourText' fields from the 'LayoutText' markup field for a given deck.
* DeckTranslator : A tool that copies cards from one language to a new language for a given deck. It marks the cards as work in progress and copies the artwork, fills in the cards' translated Subtype, Info, ...
* ProductGenerator : A tool that exports cards/decks in a woocommerce compliant file so that it can be imported in our webshop.

Build and Installation
======================

Build requiremenst
------------------

- .NET 5
- ef core tools 3.1
- Visual Studio 2019 Community Edition (or above)
- Visual Studio Code
- nodejs
- inkscape 0.91 or above, see https://inkscape.org/
- SQL Server Express (or any database with support for entity framework core, e.g. MySQL)

Althoug the development stack is windows based, it should be possible to build on linux as well using the 'dotnet build' command,
also it should be possible to configure the Arcmage.DAL project and ef core to use the MySQL provider or any other provider listed here https://docs.microsoft.com/en-us/ef/core/providers/?tabs=dotnet-core-cli

Building and run the api
------------------------

Before you starts, make sure you've installed inkscape, and add it to your system variables! 
* Check if you can run 'inkscape' in a command prompt
* In case you need to run the inkscape export process with elevated permissions or under a certain system user 
   * Edit Arcmage.Server.Api\appsettings.json and enable ForceInkscapeUserImpersonate and fill in InkscapeUser and InkscapePassword

1. Clone the repository and open the Arcmage.sln solution in Visual Studio
2. Edit ***both*** the Acrmage.Game.Api\appsettings.json and Arcmage.Server.Api\appsettings.json files
   * Update the HangFire and Arcmage connection strings to point to your database provider
   * Update the TokenEncryptionKey with your own custom generated 20 char key
3. Build the solution
   * The first time build will fetch all the required nuget packages
4. Open a command line at the Arcmage.DAL folder and run 'dotnet ef database update'
   * This will create the an empty database with the correct tables
5. Set Arcmage.Server.Api as startup project and run (use the project launch, not IIS Express)

Seeding the database
--------------------

1. Edit Acrmage.Seed\appsettings.cs and update the ServiceUser fields with your user name, email and password.
   * This will be the default admin user

2. Run the Arcmage.Seed 
   * In Visual Studio, right click the Arcmage.Seed > Debug > Start New Instance
   * Or use the alternative 'dotnet run' command to start the Arcmage.Seed program

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

While the Arcmage.Server.Api is running, start the Arcmage.Seed 
* In Visual Studio, right click the Arcmage.Game.Api > Debug > Start New Instance
* Or use the alternative 'dotnet run' command to start the Arcmage.Game.Api program

Where are the cards/decks?
==========================

You'll notice that http://localhost:5000 doesn't show any cards/decks.
This software stack comes with no cards or artwork. However you can create your own cards/decks once logged in.
Generated cards will be at Server.Arcmage.Api\wwwwroot\arcmage\Cards
Feel free to use our great CC-BY-SA4 artwork repository!. 

Card Templates
==============

Our card svg templates are at Server.Arcmage.Api\wwwroot\arcmage\CardTemplates

The are per Faction and per Type. Each template combination has the several files
1. (template).svg : the real full blown template, used to generate the cards in high resolution png and pdf files
2. (template)png : used as background while editing the card on the website
3. (template) overlay plain.svg : used as overlay while editing the card on the website  
4. (template) overlay.svg : used for changing the template in inkscape (before saving it as overlay plain)

Furthermore, the border.svg and border.png files are used to create a 2mm bleed around the cards when generating the png and pdfs print ready files.

Resources
=========

If you want to know more about the project please visit the following links or 
read the suggested documents:
- https://aminduna.arcmage.org/#/cards [Arcmage cards]
- https://aminduna.arcmage.org/#/games [Arcmage online game]
