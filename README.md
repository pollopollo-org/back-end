# [Api.PolloPollo.org](https://api.pollopollo.org) &middot; ![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg) [![codecov](https://codecov.io/gh/pollopollo-org/back-end/branch/master/graph/badge.svg)](https://codecov.io/gh/pollopollo-org/back-end) ![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)

API code base for the [PolloPollo.org](https://www.pollopollo.org) platform for charity donations

# Installing
This project is made using [.NET Core 3.1](https://dotnet.microsoft.com/download), which must be installed to build the code.

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

# Migrations

In this section we list a few guidelines for PolloPollo migrations, and fixes for sticky situations.

## Guidelines

- Make sure your branches migrations are up to date with the [migrations on master](https://github.com/pollopollo-org/back-end/tree/master/PolloPollo.Entities/Migrations) (see [this fix](#out_of_sync)).
- Before creating a new migration, make sure the following steps are in order:
    1. You're database is up to date with the newest migration (If you cannot update it please see [this fix](#cant_update_migration)).
    2. Double check that no new migrations are up. If you have two migrations on seperate branches that eventually will be merged together, you will need to revert both of them and create it again.
- Follow the naming conventions of (most of the) existing migrations - Migration_V*x* (where *x* is the number of the previous migration + 1).

## Fixes

- <a name="out_of_sync"/> If you find yourself on a branch which have migrations that are not up to date or unaligned with [migrations on master](https://github.com/pollopollo-org/back-end/tree/master/PolloPollo.Entities/Migrations), you will need to pull the migrations into your branch. This will wipe any migrations you might have created, but this can be fixed by creating a new one once your database is up to date. Pull the migrations from master as such:

```bash
cd /PolloPollo.Entities
git checkout master -- Migrations/
```

- <a name="cant_update_migration"/> If you can't update your migration to the newest one added, you might try wiping it and reupdating. This issue often occurs when your database is out of sync with the migrations history.
    1. Remove any migrations that you might have created, see [removing migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing?tabs=dotnet-core-cli#remove-a-migration) for the proper way to do it.

    2. Wipe your database and create a new one from scratch, this will be something along the lines of:

    ```sql
    DROP DATABASE `pollopollo`;
    CREATE SCHEMA `pollopollo`;
    ```

    3. Update your database to the latest known stable migration ```dotnet ef database update <MigrationName>```.


## Updating the production environment (when migrations won't work).

You need to be super carefull when handling the migrations on PolloPollo's production platform, as the database contains data essential for PolloPollo, such as products, applicants and userdata. Accidentally wiping this data, will cause serious effects for PolloPollo's userbase. Please use the test platform to practice, and use this option as a last resort.

1. You might find yourself in a situation where the database on either test or production might be unable to update. There is no good fix for this      issue, but you might consider creating dump files (which is a bit risky). To copy all the data, run the following command in mysql:
```mysqldump --no-create-info -u <user> -p pollopollo > /path/to/dump.sql```.
This command copies all the data in the database, not the individual create statements. Open the newly created *dump.sql* and remove everything under the *Dumping data for table `__efmigrationshistory`* statements. We don't want the old EF Migrations History, but the new one we are going to create in a bit.

2. Now you have copied all the data from the database, so you should be able to follow the steps in [this fix](#cant_update_migration).

3. Now your migrations are up to date, so you can attempt reinserting the data into the database. Use the following command:
```mysql -u <user> -p pollopollo < /path/to/dump.sql```.
If you are experiencing issues at this point, you might have changed existing values in the structure of the Entities, or added required fields to some of the new properties you have added to your data. For the first case, you will need to figure out which property you have changed, and manually change it in the *dump.sql* file. If you have added a new required field, you will need to add a default value for the data already in the database, do this by manually changing the *dump.sql* file aswell.

# Built With
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-2.2) - The web framework used
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) - The database framework used
- [xUnit.net](https://xunit.github.io/) - The unit test framework used
- [MySQL](https://www.mysql.com/) - The database used

# License
This project is licensed under the MIT license - see the [LICENSE.md](LICENSE.md) file for details
