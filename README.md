# PinguApps.AppwriteMigrator

PinguApps.AppwriteMigrator is a .NET CLI tool that enables a code-first approach to managing your database schema with Appwrite. This tool simplifies the process of syncing and migrating your database schema between local and remote environments, ensuring consistency across development, staging, and production setups.

[![NuGet Version](https://img.shields.io/nuget/v/PinguApps.AppwriteMigrator?logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/PinguApps.AppwriteMigrator) [![NuGet Downloads](https://img.shields.io/nuget/dt/PinguApps.AppwriteMigrator?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/PinguApps.AppwriteMigrator) ![GitHub License](https://img.shields.io/github/license/PinguApps/AppwriteMigrator?style=for-the-badge) [![GitHub Repo stars](https://img.shields.io/github/stars/PinguApps/AppwriteMigrator?style=for-the-badge&logo=github)](https://github.com/PinguApps/AppwriteMigrator) [![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/PinguApps/AppwriteMigrator/main.yml?style=for-the-badge&logo=github)](https://github.com/PinguApps/AppwriteMigrator)

# 🔧 Installation
Install the PinguApps.AppwriteMigrator package via NuGet:

```sh
dotnet tool install -g PinguApps.AppwriteMigrator --prerelease
```

# 🚀 Usage
The `appwrite-migrator` tool provides commands to sync your local schema with a remote Appwrite project and to migrate changes from a local JSON file to a remote Appwrite project.

## 📃 Commands
### `sync`
Syncs the settings from your provided project to a local JSON file.

#### Usage
```sh
appwrite-migrator sync [--endpoint <String>] [--id <String>] [--key <String>] [--file-name <String>] [--help]
```
#### Options
- `-e, --endpoint <string>`: The API endpoint (Required)
- `-i, --id <string>`: The Project ID for the target project(Required)
- `-k, --key <string>`: The API Key for the project (Required)
- `-f, --file-name <string>`: The filename to store the schema within (Default: `appwrite-schema.json`)
- `-h, --help`: Show help message


### `migrate`
Migrates the local settings JSON up to the specified project.

#### Usage
```sh
appwrite-migrator sync [--endpoint <String>] [--id <String>] [--key <String>] [--file-name <String>] [--help]
```
#### Options
- `-e, --endpoint <string>`: The API endpoint (Required)
- `-i, --id <string>`: The Project ID for the target project(Required)
- `-k, --key <string>`: The API Key for the project (Required)
- `-f, --file-name <string>`: The filename containing the schema definition (Default: `appwrite-schema.json`)
- `-h, --help`: Show help message

# 🗃️ Examples
## Syncing Schema
To sync the schema from an appwrite project running on your localhost to the default `.json` file:
```sh
appwrite-migrator sync -e http://localhost/v1 -i 98c71317207156ca9d3a -k 6e2793615c46c756ce4edd77dd9f4b00af225a4b3ef57704986012c71a9c3c677b4fab497229c71b411678eed4e0bc692de9d354b30983f33963926656d7945127e2448ee888b454d80150901b6f381efd9591ece755b2e6aea718c29bc6d1c78b6edcfad56474444b04058b6d4baaea188f8c097f623de8b362740d591c7314
```
This command will create or update the `appwrite-schema.json` file with the current schema from the specified project.


## Migrating Schema
To migrate the local JSON file to a remote Appwrite project:
```sh
appwrite-migrator migrate -e https://myproject.com/api/v1 -i 98c71317207156ca9d3a -k 6e2793615c46c756ce4edd77dd9f4b00af225a4b3ef57704986012c71a9c3c677b4fab497229c71b411678eed4e0bc692de9d354b30983f33963926656d7945127e2448ee888b454d80150901b6f381efd9591ece755b2e6aea718c29bc6d1c78b6edcfad56474444b04058b6d4baaea188f8c097f623de8b362740d591c7314
```
This command will read the `appwrite-schema.json` file and apply the schema to the specified project.