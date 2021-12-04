# Arcmage Online
Arcmage Online is a browser-based Collectible Card Game (CCG) that includes a deck-builder and card database. It is part of the [Arcmage project](https://arcmage.org). 

## Features
- Play Arcmage on a virtual one-versus-one tabletop without rule enforcement, plugins, or player registration.
- Create new cards and decks on a card generation website.

The game has no artificial intelligence, nor are there any plans to implement game rules.

## Repository Structure
This respository contains the card database, card generation website, and online game.

### Common projects (client and server)
* Arcmage.Model : The public model for cards, decks, games, etc.

### Client projects
* Arcmage.Client : A custom strongly-typed C# http client for easy access to the Arcmage API.

### API folders
* Arcmage.Configuration : A small settings library to load some settings from json
* Arcmage.DAL : The database ORM layer for use with Entity Framework Core
* Arcmage.Seed : A database seeder to populate an empty database with some basic data (Sets, Factions, etc.)
* Arcmage.InputConvertor : A library to convert our card layout XML field to markdown or vice versa
* Arcmage.Server.Api : The REST API for Authentication, Card Creation, Deck Creation, Card Searching, etc.
* Arcmage.Web : Holds the Angular web app for card and deck creation, and the Vue app for the online game

### Game folders
* Arcmage.Game.Api : The SignalR hub backend for the browser-to-browser communication

### Tools
* ApiExamples : A small demo on how to use the Arcmage.Client to interact with the Arcmage API
* DeckAutoCompleter : A tool that extracts and updates the card's 'RuleText' and 'FlavourText' fields from the 'LayoutText' markup field for a given deck.
* DeckTranslator : A tool that copies cards from one language to a new language for a given deck. It marks the cards as work in progress and copies the artwork, fills in the cards' translated Subtype, Info, etc.
* ProductGenerator : A tool that exports cards/decks in a woocommerce-compliant file so that it can be imported in our webshop

## Building and Installing
### Requirements
- .NET 5
- ef core tools 3.1
- Visual Studio 2019 Community Edition (or above)
- Visual Studio Code
- nodejs
- inkscape 0.91 or above, see https://inkscape.org/
- SQL Server Express (or any database with support for entity framework core, e.g. MySQL)

Although the development stack is Windows-based, it should be possible to build on Linux as well by running `dotnet build`. Also it should be possible to configure the Arcmage.DAL project and ef core to use the MySQL provider or any other provider listed here: https://docs.microsoft.com/en-us/ef/core/providers/?tabs=dotnet-core-cli.

### Building and running the API
#### Before you start:
1. Install inkscape.
2. Add it to your system variables!

**Tip:** Check if you can run 'inkscape' in a command prompt.

**Tip:** If you need to run the inkscape export process with elevated permissions or as a certain system user:
* Edit Arcmage.Server.Api\appsettings.json and enable ForceInkscapeUserImpersonate and fill in InkscapeUser and InkscapePassword

#### To build and run the API:
1. Clone the repository and open the Arcmage.sln solution in Visual Studio.
2. Edit ***both*** the Acrmage.Game.Api\appsettings.json and Arcmage.Server.Api\appsettings.json files.
   * Update the HangFire and Arcmage connection strings to point to your database provider.
   * Update the TokenEncryptionKey with your own custom generated 20 char key.
3. Build the solution.
   * The first-time build fetches all the required nuget packages.
4. Open a command line at the Arcmage.DAL folder and run `dotnet ef database update`
   * This creates an empty database with the correct tables.
5. Set Arcmage.Server.Api as the startup project and run (use the project launch, not IIS Express).

### Seeding the database
1. Edit Acrmage.Seed\appsettings.cs and update the ServiceUser fields with your user name, email and password.
   * This will be the default admin user.
2. Start Arcmage.Seed using one of the following methods:
   * In Visual Studio, right-click **Arcmage.Seed** > **Debug** > **Start New Instance**.
   * Run `dotnet run`

### Settings tip for contributors
If you'd like to contribute the code base, but keep your settings from being committed, use alternative 'appsettings_development.json' files instead.
(You can still leave the appsettings.json files as is)

### Building the Angular web app
1. Open the Arcmage.Web folder in Visual Studio Code.
2. Open a new terminal.
3. Run `npm install`
    * This will fetch all required npm packages
4. Run `npm run build`
    * This will build the Angular app (and game Vue js app) and output it in the './dist' folder
	* A post-build script will copy the './dist' contents to **Arcmage.Server.Api/wwwroot** (this is done so we can host the web apps and the server API in a single Kestrel web server)
5. While the Arcmage.Server.API is active, browse to http://localhost:5000.

### Running the game API
The game API is a standalone dotnet core app using signalR hubs for the browser-to-browser communication.

While the Arcmage.Server.Api is running, start the Arcmage.Game.Api using either of the following methods:
* In Visual Studio, right-click **Arcmage.Game.Api** > **Debug** > **Start New Instance**.
* Run `dotnet run`

## Where are the cards/decks?
You'll notice that http://localhost:5000 doesn't show any cards/decks.
This software stack comes with no cards or artwork. However you can create your own cards/decks once logged in.
Cards generate into the following path: **Server.Arcmage.Api\wwwwroot\arcmage\Cards**

Feel free to use our great CC-BY-SA4 artwork repository!

## Card templates
Find card templates in the following path: **Server.Arcmage.Api\wwwroot\arcmage\CardTemplates**

Templates are organized by Faction and Type. Each template contains several files:
1. (template).svg : the real full blown template, used to generate the cards in high resolution png and pdf files
2. (template).png : used as background while editing the card on the website
3. (template) overlay plain.svg : used as overlay while editing the card on the website  
4. (template) overlay.svg : used for changing the template in inkscape (before saving it as overlay plain)

The border.svg and border.png files are used to create a 2mm bleed around the cards when generating the png and pdf print-ready files.

## Resources
If you want to know more about the project please visit the following links:
- [Play Arcmage online](https://aminduna.arcmage.org/#/games)
- [View Arcmage cards](https://aminduna.arcmage.org/#/cards)
