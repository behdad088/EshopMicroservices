FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/Order.Query/Order.Query.API/Order.Query.API.csproj", "Services/Order.Query/Order.Query.API/"]
COPY ["Services/Shared/Shared.csproj", "Services/Shared/"]
COPY ["Services/Order.Query/Order.Query/Order.Query.csproj", "Services/Order.Query/Order.Query/"]
COPY ["Directory.Packages.props", "/"]
RUN dotnet restore "Services/Order.Query/Order.Query.API/Order.Query.API.csproj"
COPY . .
WORKDIR "/src/Services/Order.Query/Order.Query.API"
RUN dotnet build "./Order.Query.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Order.Query.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Order.Query.API.dll"]
