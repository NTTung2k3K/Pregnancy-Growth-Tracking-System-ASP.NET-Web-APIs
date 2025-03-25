# Use the .NET 8 base image for the runtime environment 
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base 
USER $APP_UID 
WORKDIR /app 
EXPOSE 8080 
EXPOSE 8081 

# Use the .NET 8 SDK image for build environment 
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build 

# Define the build configuration argument (default is 'Release') 
ARG BUILD_CONFIGURATION=Release 

# Set the working directory 
WORKDIR /src 

# Copy the solution file first (if you have a .sln file)
COPY *.sln ./

# Copy project files and restore dependencies
COPY BabyCare.API/*.csproj BabyCare.API/
COPY BabyCare.Core/*.csproj BabyCare.Core/
COPY BabyCare.Services/*.csproj BabyCare.Services/
COPY BabyCare.Contract.Services/*.csproj BabyCare.Contract.Services/
COPY BabyCare.Repositories/*.csproj BabyCare.Repositories/
COPY BabyCare.Contract.Repositories/*.csproj BabyCare.Contract.Repositories/
COPY BabyCare.ModelViews/*.csproj BabyCare.ModelViews/
COPY BabyCare.WorkerService/*.csproj BabyCare.WorkerService/

# Restore dependencies
RUN dotnet restore "BabyCare.API/BabyCare.API.csproj"

# Copy the entire project
COPY . .

# Set the working directory for the build process 
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