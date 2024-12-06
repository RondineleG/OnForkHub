#!/bin/bash

docker build -t on-fork-hub -f .devcontainer/Dockerfile .
docker run --rm -it -v $(pwd):/app -w /app on-fork-hub bash
