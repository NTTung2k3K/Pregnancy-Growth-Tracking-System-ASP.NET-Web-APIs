# Use the .NET 8 base image for the runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the .NET 8 SDK image for build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Define the build configuration argument (default is 'Release')
ARG BUILD_CONFIGURATION=Release

# Set the working directory
WORKDIR /src

# Copy the solution file and project files
COPY ["BabyCare/BabyCare.sln", "BabyCare/"]
COPY ["BabyCare/BabyCare.API/BabyCare.API.csproj", "BabyCare/BabyCare.API/"]
COPY ["BabyCare/BabyCare.Core/BabyCare.Core.csproj", "BabyCare/BabyCare.Core/"]
COPY ["BabyCare/BabyCare.Services/BabyCare.Services.csproj", "BabyCare/BabyCare.Services/"]
COPY ["BabyCare/BabyCare.Contract.Services/BabyCare.Contract.Services.csproj", "BabyCare/BabyCare.Contract.Services/"]
COPY ["BabyCare/BabyCare.Repositories/BabyCare.Repositories.csproj", "BabyCare/BabyCare.Repositories/"]
COPY ["BabyCare/BabyCare.Contract.Repositories/BabyCare.Contract.Repositories.csproj", "BabyCare/BabyCare.Contract.Repositories/"]
COPY ["BabyCare/BabyCare.ModelViews/BabyCare.ModelViews.csproj", "BabyCare/BabyCare.ModelViews/"]

# Restore dependencies
RUN dotnet restore "BabyCare/BabyCare.sln"

# Copy the rest of the application files
COPY . .

# Set the working directory for the build process
WORKDIR "/src/BabyCare/BabyCare.API"

# Build the project
RUN dotnet build "BabyCare.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application to prepare for deployment
FROM build AS publish
RUN dotnet publish "BabyCare.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Set up the final image
FROM base AS final
WORKDIR /app

# Copy the published files from the publish stage
COPY --from=publish /app/publish .

# Set the entry point for the container
ENTRYPOINT ["dotnet", "BabyCare.API.dll"]