#!/bin/bash

docker build --build-arg REMOTE_USER=$USER --build-arg REMOTE_UID=$UID --build-arg REMOTE_GID=$UID -t on-fork-hub -f .devcontainer/Dockerfile .
docker run --rm -it -v $(pwd):/app -w /app on-fork-hub bash
