# cloud-pmd-updater

Tracks PMD world records using Azure Functions and Cosmos DB.

## Configuration

Configure the following variables (in local.settings.json or Azure Portal/CLI):
* FUNCTIONS_WORKER_RUNTIME: "dotnet"
* CosmosDBConnection: Your Cosmos DB connection string

## Installation

### Local install

1. Clone this repo onto your machine.
2. Create a local.settings.json file in CloudPMD.Updater using the above variables.
3. Start the CloudPMD.Utilities Function App and POST localhost:7073/api/SeedDatabase to seed the database.

### Azure install

Work-in-progress

## Features

* Automatically tracks PMD world records by polling (daily by default) the speedrun.com API
* As WRs do not change often, stores the records in Cosmos DB

## Credits

Oliver X. (Liversticks)