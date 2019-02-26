# [Api.PolloPollo.org](https://api.pollopollo.org)
API code base for the [PolloPollo.org](https://www.pollopollo.org) platform for charity donations

# Installing
This project is made using [.Net Core 2.2](https://dotnet.microsoft.com/download), which must be installed to build the code.

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


# Running the tests
- Using command-line
    1. ```> dotnet tests```
- Using Visual Studio 2017/Visual Studio Community/Visual Studio for Mac
    1. Click *Test explorer* in the UI under test in the toolbar
    2. Run all tests

# Deployment
- Using command-line
    1. ```> dotnet publish``` - This command builds the code and creates the folder with the builded dll's at the path: PolloPollo.Web/bin/Debug/netcoreapp2.2/publish
    2. Create [Github release](https://help.github.com/en/articles/creating-releases) from the publish folder

# Built With
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-2.2) - The web framework used
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) - The database framework used
- [xUnit.net](https://xunit.github.io/) - The unit test framework used

# License
This project is licensed under the MIT license - see the [LICENSE.md](LICENSE.md) file for details
