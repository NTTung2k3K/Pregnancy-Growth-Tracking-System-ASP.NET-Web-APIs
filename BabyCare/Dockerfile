# Use .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy solution and global files
COPY *.sln .
COPY BabyCare.sln .
COPY global.json* .
COPY Directory.Build.props* .

# Copy all project files
COPY BabyCare.API/*.csproj ./BabyCare.API/
COPY BabyCare.Contract.Repositories/*.csproj ./BabyCare.Contract.Repositories/
COPY BabyCare.Contract.Services/*.csproj ./BabyCare.Contract.Services/
COPY BabyCare.Core/*.csproj ./BabyCare.Core/
COPY BabyCare.ModelViews/*.csproj ./BabyCare.ModelViews/
COPY BabyCare.Repositories/*.csproj ./BabyCare.Repositories/
COPY BabyCare.Services/*.csproj ./BabyCare.Services/

# Restore dependencies
RUN dotnet restore BabyCare.sln

# Copy all files
COPY . .

# Publish the API project
RUN dotnet publish BabyCare.API -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "BabyCare.API.dll"]