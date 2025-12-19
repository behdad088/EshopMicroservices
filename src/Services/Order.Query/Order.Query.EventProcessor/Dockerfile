FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/Order.Query/Order.Query.EventProcessor/Order.Query.EventProcessor.csproj", "Services/Order.Query/Order.Query.EventProcessor/"]
COPY ["Services/Shared/Shared.csproj", "Services/Shared/"]
COPY ["Services/Order.Query/Order.Query.Data/Order.Query.Data.csproj", "Services/Order.Query/Order.Query.Data/"]
COPY ["Directory.Packages.props", "/"]
RUN dotnet restore "Services/Order.Query/Order.Query.EventProcessor/Order.Query.EventProcessor.csproj"
COPY . .
WORKDIR "/src/Services/Order.Query/Order.Query.EventProcessor"
RUN dotnet build "./Order.Query.EventProcessor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Order.Query.EventProcessor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Order.Query.EventProcessor.dll"]
