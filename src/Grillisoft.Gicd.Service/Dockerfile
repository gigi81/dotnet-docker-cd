# Multi-stage build for Grillisoft.Gicd.Service
# Build context should be the repository root when running docker build -f src/Grillisoft.Gicd.Service/Dockerfile .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

# Copy solution and common MSBuild props to leverage layer caching
COPY *.slnx ./
COPY *.props ./

# Copy project files
COPY src ./src
COPY tests ./tests

# Restore packages
RUN dotnet restore --use-current-runtime

# Copy everything and publish
WORKDIR /build/src/Grillisoft.Gicd.Service
RUN dotnet publish -c Release -o /app/publish --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl & ca-certificates to download sops in runtime image, then clean up
RUN apt-get update && apt-get install -y git curl ca-certificates \
    && rm -rf /var/lib/apt/lists/*

ARG SOPS_VERSION=3.11.0
RUN set -eux; \
    ARCH="$(uname -m)"; \
    case "$ARCH" in \
      x86_64) SOPS_ARCH=amd64 ;; \
      aarch64|armv8*) SOPS_ARCH=arm64 ;; \
      armv7l) SOPS_ARCH=armv7 ;; \
      *) echo "Unsupported arch $ARCH"; exit 1 ;; \
    esac; \
    curl -Lo /usr/local/bin/sops "https://github.com/getsops/sops/releases/download/v${SOPS_VERSION}/sops-v${SOPS_VERSION}.linux.${SOPS_ARCH}"; \
    chmod +x /usr/local/bin/sops

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 80
ENTRYPOINT ["dotnet", "Grillisoft.Gicd.Service.dll"]
