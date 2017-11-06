# ERNI-Photo-Database
![badge](https://mar3ek.visualstudio.com/_apis/public/build/definitions/1f269315-a206-4cf0-b019-922c68fb0593/7/badge)

# Development
After cloning the solution, you need to configure the SQL server connection string to use for development and enable the Azure Storage Emulator.

## SQL Server connection string
The application takes its connection string from a [user secret](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?tabs=visual-studio). For development, you need to add you own user secret for the application pick it up.

The setting name is `ConnectionString`.

### Setting the connection string from Visual Studio:
1. Open Solution explorer and right-click the ERNI.PhotoDatabase.Server project
2. In the context menu, select Manage user secrets
3. Put the following as the content of the file that opens:
```
{
  "ConnectionStrings": {
    "Database": "your-sql-server-connection-string"
  }
}
```

### Setting the connection string from VS code (or other editor)
1. Open a command prompt and change your wowking directory to the folder `server\ERNI.PhotoDatabase.Server`
2. Run `dotnet restore` (this will install the user secrets CLI tool)
3. Run `dotnet user-secrets set ConnectionString your-sql-server-connection-string`

## Azure Storage Emulator
The application uses Azure blob storage to store images. For development, we use Azure Storage Emulator to develop locally.

Install the emulator from [here](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator). There is also a Linux variand linked in that article.

Follow the instructions in the linked article to see how to initialize and start the emulator. Once the emulator is successfully started, the application will be ready to run locally.
