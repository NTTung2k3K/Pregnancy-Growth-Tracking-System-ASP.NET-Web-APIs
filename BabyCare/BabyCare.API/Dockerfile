# Use the .NET 8 base image for the runtime environment 
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base 
USER $APP_UID 
WORKDIR /app 
EXPOSE 8080 
EXPOSE 8081 

# Use the .NET 8 SDK image for build environment 
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build 
ARG BUILD_CONFIGURATION=Release 
WORKDIR /src 

# Copy project files
COPY ["BabyCare.API/BabyCare.API.csproj", "BabyCare.API/"]
COPY ["BabyCare.Core/BabyCare.Core.csproj", "BabyCare.Core/"]
COPY ["BabyCare.Services/BabyCare.Services.csproj", "BabyCare.Services/"]
COPY ["BabyCare.Contract.Services/BabyCare.Contract.Services.csproj", "BabyCare.Contract.Services/"]
COPY ["BabyCare.Repositories/BabyCare.Repositories.csproj", "BabyCare.Repositories/"]
COPY ["BabyCare.Contract.Repositories/BabyCare.Contract.Repositories.csproj", "BabyCare.Contract.Repositories/"]
COPY ["BabyCare.ModelViews/BabyCare.ModelViews.csproj", "BabyCare.ModelViews/"]

# Restore dependencies
RUN dotnet restore "/src/BabyCare.API/BabyCare.API.csproj"

# Copy the entire solution
COPY . .

# Set working directory to API project
WORKDIR "/src/BabyCare.API"

# Build the project
RUN dotnet build "BabyCare.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish 
RUN dotnet publish "BabyCare.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false 

# Final stage 
FROM base AS final 
WORKDIR /app 
COPY --from=publish /app/publish . 
ENTRYPOINT ["dotnet", "BabyCare.API.dll"]