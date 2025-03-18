FROM debian:bookworm-slim

ENV DOTNET_ROOT=/usr/share/dotnet
ENV PATH="$PATH:/usr/share/dotnet"

RUN apt update && apt install -y --no-install-recommends \
    curl \
    wget \
    git \
    git-flow \
    ca-certificates && \
    apt clean && \
    rm -rf /var/lib/apt/lists/*

RUN wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt update && apt install -y dotnet-sdk-9.0 dotnet-sdk-8.0 && \
    apt clean && \
    rm -rf /var/lib/apt/lists/*

RUN curl -fsSL https://deb.nodesource.com/setup_lts.x | bash - && \
    apt update && apt install -y --no-install-recommends nodejs && \
    apt clean && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
RUN git config --global --add safe.directory '/app'
EXPOSE 8000 8001 9000 9001
CMD ["bash"]
