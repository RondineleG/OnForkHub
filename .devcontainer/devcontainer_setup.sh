#!/bin/bash

docker build -t on-fork-hub -f .devcontainer/Dockerfile .
docker run --rm -it -v ${PWD}:/app -w /app on-fork-hub bash
