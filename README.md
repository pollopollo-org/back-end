# [Api.PolloPollo.org](https://api.pollopollo.org) &middot; ![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg) [![codecov](https://codecov.io/gh/pollopollo-org/back-end/branch/master/graph/badge.svg)](https://codecov.io/gh/pollopollo-org/back-end) ![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)

API code base for the [PolloPollo.org](https://www.pollopollo.org) platform for charity donations

# Installing
This project is made using [.Net Core 3.1](https://dotnet.microsoft.com/download), which must be installed to build the code.

## Windows
The Microsoft described prerequisites can be found [here](https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore2x)

## MacOS
The Microsoft described prerequisites can be found [here](https://docs.microsoft.com/en-us/dotnet/core/macos-prerequisites?tabs=netcore2x)

## Linux
The Microsoft described prerequisites can be found [here](https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites?tabs=netcore2x)


## Building the code
- Using command-line
    1. ```> dotnet restore``` - fetches the dependencies.
    2. ```> dotnet build``` - builds the code
- Using Visual Studio 2017/Visual Studio Community/Visual Studio for Mac
  1. Click *Build solution* in the UI under build in the toolbar

## Setup

Install [MySql](https://dev.mysql.com/doc/mysql-installation-excerpt/5.7/en/).

Create a user secret for the connection string to the database.
```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<ConnectionString>"
```
Create a user secret for the authentication token.
```
dotnet user-secrets set "authentication:Secret" "<HMACSHA256 String>"
```

Update the database, see Database Migrations section.

## Database Migrations (EntityFrameworkCore)
### Using command-line

Documentation for [EntityFramework Commands](http://www.entityframeworktutorial.net/efcore/cli-commands-for-ef-core-migration.aspx)

**NOTE:** Go to /PolloPollo.Web before running ef commands.
```
cd /PolloPollo.Web
```

Add migrations
```
dotnet ef migrations add <MigrationName> --project ../PolloPollo.Entities
```

Update database
```
dotnet ef database update <MigrationName>
```

Generate sql script from migration. Stand in the PolloPollo.Web directory.

```
dotnet ef migrations script -i --project ../PolloPollo.Entities -o <FILE>
```

# Running the tests
- Using command-line
    1. ```> dotnet tests```
- Using Visual Studio 2017/Visual Studio Community/Visual Studio for Mac
    1. Click *Test explorer* in the UI under test in the toolbar
    2. Run all tests

# Deployment
- Using command-line
    1. Create a `appsettings.Production.json` file with the following structure:
    ```
    {
        "Logging": {
            "LogLevel": {
                "Default": "Debug",
                "System": "Information",
                "Microsoft": "Information"
            }
        },
        "ConnectionStrings": {
            "DefaultConnection": "<Live-production-connectionstring>"
        },
        "Authentication": {
            "Secret": "<HMACSHA256 String>"
        },
    }
    ```
    2. ```> dotnet publish``` - This command builds the code and creates the directory with the builded dll's at the path: PolloPollo.Web/bin/Debug/netcoreapp2.2/publish
    3. Create a [Github release](https://help.github.com/en/articles/creating-releases)
    4. Put contents of publish directory onto the server and restart application on server.

# Built With
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-2.2) - The web framework used
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) - The database framework used
- [xUnit.net](https://xunit.github.io/) - The unit test framework used
- [MySQL](https://www.mysql.com/) - The database used

# License
This project is licensed under the MIT license - see the [LICENSE.md](LICENSE.md) file for details
