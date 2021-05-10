# Testing

## Testing Setup

1. Download the Azure Cosmos DB Emulator [here](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator).
2. Start the Utilities Function App.
3. Make a POST request to localhost:7073/api/SeedDatabase.

NOTE: The DB seed script creates a new Cosmos DB database with 400 RU/s provisioned throughput.
For a "production" environment, it is recommended to fine-tune DB creation via the Azure portal.

## Test Cases

1. URLs (SRC API endpoints) should be logged to the console.
2. Each function should execute in less than 1 minute.
3. Runs should be created in the V1-pmdboard container.
4. Runs should only be created if the game-category-platform-language combination has a non-empty record-holder.
5. Runs should have the following format:

Property Name | Expected Value
------------- | --------------
id | run-[SRC game ID]-[SRC category ID]-[SRC platform ID]-[SRC language ID]
Game | Title of the game
Category | Category of run
Platform | Platform the run was performed on
Language | ENG or JPN
Version | Only filled for Explorers of Time/Darkness and WiiWare, otherwise empty
Runner | SRC username of the runner (including guests)
RunDate | YYYY-MM-DD date of the run
RunTime | Length of the run in seconds
SRCLink | 8-character ID on speedrun.com