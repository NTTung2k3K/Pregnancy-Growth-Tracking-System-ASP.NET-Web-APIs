# ==========================
# 1️⃣ Base image (Runtime)
# ==========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ==========================
# 2️⃣ Build & Restore
# ==========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# ⚠️ Copy toàn bộ source trước khi restore
COPY . .

# ⚠️ Chạy restore
RUN dotnet restore "BabyCare.API/BabyCare.API.csproj"

# Build ứng dụng
WORKDIR "/src/BabyCare.API"
RUN dotnet build "BabyCare.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ==========================
# 3️⃣ Publish app
# ==========================
FROM build AS publish
RUN dotnet publish "BabyCare.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ==========================
# 4️⃣ Final image (Production)
# ==========================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BabyCare.API.dll"]
