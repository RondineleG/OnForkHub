# Use Debian as the base image
FROM debian:bookworm-slim

# Set environment variables
ENV DOTNET_ROOT=/usr/share/dotnet
ENV PATH="$PATH:/usr/share/dotnet"

# Update and install required dependencies
RUN apt update && apt install -y --no-install-recommends \
    curl \
    wget \
    git \
    git-flow \
    ca-certificates && \
    apt clean && \
    rm -rf /var/lib/apt/lists/*

# Install .NET SDK 8 and 9
RUN wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt update && apt install -y dotnet-sdk-9.0 dotnet-sdk-8.0 && \
    apt clean && \
    rm -rf /var/lib/apt/lists/*

# Install Node.js LTS
RUN curl -fsSL https://deb.nodesource.com/setup_lts.x | bash - && \
    apt update && apt install -y --no-install-recommends nodejs && \
    apt clean && \
    rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /app

RUN git config --global --add safe.directory '/app'

# Expose the application port
EXPOSE 8000 8001 9000 9001

# Set the default command
CMD ["bash"]
