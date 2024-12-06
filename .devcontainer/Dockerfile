# Use Debian as the base image
FROM debian:bookworm-slim

# Set a non-root user
ARG REMOTE_USER
ARG REMOTE_UID
ARG REMOTE_GID
RUN groupadd --gid $REMOTE_GID $REMOTE_USER && \
    useradd -s /bin/bash --uid $REMOTE_UID --gid $REMOTE_GID -m $REMOTE_USER && \
    apt-get update && apt-get install -y sudo && \
    echo $REMOTE_USER ALL=\(ALL\) NOPASSWD:ALL > /etc/sudoers.d/$REMOTE_USER && \
    chmod 0440 /etc/sudoers.d/$REMOTE_USER

ENV HOME /home/${REMOTE_USER}

USER ${REMOTE_USER}

# Set environment variables
ENV DOTNET_ROOT=/usr/share/dotnet
ENV PATH="$PATH:/usr/share/dotnet"

# Update and install required dependencies
RUN sudo apt update && sudo apt install -y --no-install-recommends \
    curl \
    wget \
    git \
    git-flow \
    ca-certificates && \
    sudo apt clean && \
    sudo rm -rf /var/lib/apt/lists/*

# Install .NET SDK 8 and 9
RUN sudo wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    sudo dpkg -i packages-microsoft-prod.deb && \
    sudo apt update && sudo apt install -y dotnet-sdk-9.0 dotnet-sdk-8.0 && \
    sudo apt clean && \
    sudo rm -rf /var/lib/apt/lists/*

# Install Node.js LTS
RUN sudo curl -fsSL https://deb.nodesource.com/setup_lts.x | bash - && \
    sudo apt update && sudo apt install -y --no-install-recommends nodejs && \
    sudo apt clean && \
    sudo rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /app

# Expose the application port
EXPOSE 8000 8001 9000 9001

# Set the default command
CMD ["bash"]
