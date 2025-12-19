FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER root
WORKDIR /app

# Install ICU libraries and set the necessary environment variable
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
USER $APP_UID

EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
RUN apk add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /src
COPY ["Services/Order.Command/Order.Command.API/Order.Command.API.csproj", "Services/Order.Command/Order.Command.API/"]
COPY ["Services/Shared/Shared.csproj", "Services/Shared/"]
COPY ["Services/Order.Command/Order.Command.Application/Order.Command.Application.csproj", "Services/Order.Command/Order.Command.Application/"]
COPY ["Services/Order.Command/Order.Command.Domain/Order.Command.Domain.csproj", "Services/Order.Command/Order.Command.Domain/"]
COPY ["Services/Order.Command/Order.Command.Infrastructure/Order.Command.Infrastructure.csproj", "Services/Order.Command/Order.Command.Infrastructure/"]
COPY ["Directory.Packages.props", "/"]
RUN dotnet restore "Services/Order.Command/Order.Command.API/Order.Command.API.csproj"
COPY . .
WORKDIR "/src/Services/Order.Command/Order.Command.API"
RUN dotnet build "Order.Command.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Order.Command.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Order.Command.API.dll"]
