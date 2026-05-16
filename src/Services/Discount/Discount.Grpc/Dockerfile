FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base

# Install grpc_health_probe so Docker can probe the gRPC health service (grpc.health.v1.Health).
USER root
ARG TARGETARCH
ARG GRPC_HEALTH_PROBE_VERSION=v0.4.25
RUN apk add --no-cache wget \
    && case "${TARGETARCH:-amd64}" in \
        amd64) PROBE_ARCH=amd64 ;; \
        arm64) PROBE_ARCH=arm64 ;; \
        *)     PROBE_ARCH=amd64 ;; \
    esac \
    && wget -qO /usr/local/bin/grpc_health_probe \
        "https://github.com/grpc-ecosystem/grpc-health-probe/releases/download/${GRPC_HEALTH_PROBE_VERSION}/grpc_health_probe-linux-${PROBE_ARCH}" \
    && chmod +x /usr/local/bin/grpc_health_probe

USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release

# Install necessary dependencies for protoc
RUN apk update && apk add --no-cache libc6-compat gcompat

WORKDIR /src
COPY ["Services/Discount/Discount.Grpc/Discount.Grpc.csproj", "Services/Discount/Discount.Grpc/"]
COPY ["Directory.Packages.props", "/"]
RUN dotnet restore "Services/Discount/Discount.Grpc/Discount.Grpc.csproj"
COPY . .
WORKDIR "/src/Services/Discount/Discount.Grpc"
RUN dotnet build "Discount.Grpc.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Discount.Grpc.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Discount.Grpc.dll"]
