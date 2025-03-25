# Use official .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy solution and project files
COPY *.sln ./
COPY BabyCare.API/*.csproj ./BabyCare.API/
COPY BabyCare.Contract.Repositories/*.csproj ./BabyCare.Contract.Repositories/
COPY BabyCare.Contract.Services/*.csproj ./BabyCare.Contract.Services/
COPY BabyCare.Core/*.csproj ./BabyCare.Core/
COPY BabyCare.ModelViews/*.csproj ./BabyCare.ModelViews/
COPY BabyCare.Repositories/*.csproj ./BabyCare.Repositories/
COPY BabyCare.Services/*.csproj ./BabyCare.Services/
COPY BabyCare.WorkerService/*.csproj ./BabyCare.WorkerService/

# Restore dependencies
RUN dotnet restore BabyCare.sln

# Copy all files and build
COPY . ./
RUN dotnet publish BabyCare.API -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "BabyCare.API.dll"]