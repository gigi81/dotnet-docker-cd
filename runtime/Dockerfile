ARG NET_VERSION=10.0
ARG SOPS_VERSION=3.11.0

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:${NET_VERSION} AS runtime

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

# Re-declare the ARG inside the stage so it's available to RUN instructions
ARG SOPS_VERSION
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
