echo off

IF [%1]==[] goto noparam

echo "Build image '%1' and 'latest'..."
docker build -f ./Dockerfile --build-arg PROJ=MyLab.FileStorage -t ghcr.io/mylab-tools/fs-api:%1 -t ghcr.io/mylab-tools/fs-api:latest ../src
docker build -f ./Dockerfile --build-arg PROJ=MyLab.FileStorage.Cleaner -t ghcr.io/mylab-tools/fs-api:%1 -t ghcr.io/mylab-tools/fs-api:latest ../src

echo "Publish image '%1' ..."
docker push ghcr.io/mylab-tools/fs-api:%1
docker push ghcr.io/mylab-tools/fs-cleaner:%1

echo "Publish image 'latest' ..."
docker push ghcr.io/mylab-tools/fs-api:latest
docker push ghcr.io/mylab-tools/fs-cleaner:latest

goto done

:noparam
echo "Please specify image version"
goto done

:done
echo "Done!"