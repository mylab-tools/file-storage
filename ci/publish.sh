#!/bin/sh

if [ $# -eq 0 ]; then
    >&2 echo "No arguments provided"
    exit 1
fi

echo "Build images '$1' and 'latest'..."
docker build --progress plain -f ./Dockerfile --build-arg PROJ=MyLab.FileStorage -t ghcr.io/mylab-tools/fs-api:$1 -t ghcr.io/mylab-tools/fs-api:latest ../src
docker build --progress plain -f ./Dockerfile --build-arg PROJ=MyLab.FileStorage.Cleaner -t ghcr.io/mylab-tools/fs-cleaner:$1 -t ghcr.io/mylab-tools/fs-cleaner:latest ../src

echo "Publish images '$1' ..."
docker push ghcr.io/mylab-tools/fs-api:$1
docker push ghcr.io/mylab-tools/fs-cleaner:$1

echo "Publish images 'latest' ..."
docker push ghcr.io/mylab-tools/fs-api:latest
docker push ghcr.io/mylab-tools/fs-cleaner:latest

echo "Done!"