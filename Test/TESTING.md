# Testing

## Testing Setup

1. Download the Azure Cosmos DB Emulator [here](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator).
2. In the Cosmos DB emulator, create a new database and container as follows:
	1. Create a new database with database ID "Shared-Free". Leave throughput at the default (manual 400 RU/s).
	2. Create a new container with database ID "Shared-Free", container ID "V1-pmdboard", and partition key "/id".
3. In the emulator, for each json file in this folder, create an entry in the V1-pmdboard container.

NOTE: The JSON files are accurate as of May 6, 2021. Hopefully the SRC mods don't end up changing the schema yet again...

## Test Cases

1. URLs (SRC API endpoints) should be logged to the console.
2. Each function should execute in less than 1 minute.
3. Runs should be created in the V1-pmdboard container.
4. Runs should only be created if the game-category-platform-language combination has a non-empty record-holder.
5. Runs should have the following format:

Property Name | Expected Value
------------- | --------------
id | "run-<SRC game ID>-<SRC category ID>-<SRC platform ID>-<SRC language ID>"
Game | Title of the game
Category | Category of run
Platform | Platform the run was performed on
Language | ENG or JPN
Version | Only filled for Explorers of Time/Darkness and WiiWare, otherwise empty
Runner | SRC username of the runner (including guests)
RunDate | YYYY-MM-DD date of the run
RunTime | Length of the run in seconds
SRCLink | 8-character ID on speedrun.com