#!/bin/sh

if [ $# -eq 0 ]; then
    >&2 echo "No arguments provided"
    exit 1
fi

echo "Build images '$1'-dev"
docker build --progress plain -f ./Dockerfile --build-arg PROJ=MyLab.FileStorage -t ghcr.io/mylab-tools/fs-api:$1-dev ../src
docker build --progress plain -f ./Dockerfile --build-arg PROJ=MyLab.FileStorage.Cleaner -t ghcr.io/mylab-tools/fs-cleaner:$1-dev ../src

echo "Publish images '$1-dev' ..."
docker push ghcr.io/mylab-tools/fs-api:%1-dev
docker push ghcr.io/mylab-tools/fs-cleaner:%1-dev

echo "Done!"