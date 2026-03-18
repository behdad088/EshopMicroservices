FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/ApiGateways/ApiGateways.csproj", "Services/ApiGateways/"]
COPY ["Services/Shared/Shared.csproj", "Services/Shared/"]
COPY ["Directory.Packages.props", "/"]
RUN dotnet restore "Services/ApiGateways/ApiGateways.csproj"
COPY . .
WORKDIR "/src/Services/ApiGateways"
RUN dotnet build "./ApiGateways.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ApiGateways.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiGateways.dll"]
