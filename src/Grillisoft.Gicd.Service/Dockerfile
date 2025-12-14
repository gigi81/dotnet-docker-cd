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

# Speed up dotnet by skipping first-run work and using a known NuGet packages folder
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    NUGET_PACKAGES=/root/.nuget/packages

# Restore packages using BuildKit cache mounts for NuGet. Requires BuildKit (docker buildx) to be effective.
RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/root/.nuget/NuGetFallbackFolder \
    dotnet restore --use-current-runtime

# Copy everything and publish
WORKDIR /build/src/Grillisoft.Gicd.Service
RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/root/.nuget/NuGetFallbackFolder \
    dotnet publish -c Release -o /app/publish --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install git, curl & ca-certificates to download sops in runtime image, then clean up
RUN apt update && apt install -y git curl ca-certificates

RUN install -m 0755 -d /etc/apt/keyrings
RUN curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
RUN chmod a+r /etc/apt/keyrings/docker.asc

# Create the Docker apt sources list in a POSIX-friendly way (avoid heredoc parsing issues)
RUN tee /etc/apt/sources.list.d/docker.sources <<EOF
Types: deb
URIs: https://download.docker.com/linux/ubuntu
Suites: $(. /etc/os-release && echo "${UBUNTU_CODENAME:-$VERSION_CODENAME}")
Components: stable
Signed-By: /etc/apt/keyrings/docker.asc
EOF

RUN apt update && apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin \
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
ENV DOTNET_ENVIRONMENT=Production

EXPOSE 80
ENTRYPOINT ["dotnet", "gicd.dll"]


