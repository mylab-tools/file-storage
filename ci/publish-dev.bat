echo off

IF [%1]==[] goto noparam

echo "Build images '%1'-dev"
docker build --progress plain -f ./Dockerfile --build-arg PROJ=MyLab.FileStorage -t ghcr.io/mylab-tools/fs-api:%1-dev ../src
docker build --progress plain -f ./Dockerfile --build-arg PROJ=MyLab.FileStorage.Cleaner -t ghcr.io/mylab-tools/fs-cleaner:%1-dev ../src

echo "Publish images '%1-dev' ..."
docker push ghcr.io/mylab-tools/fs-api:%1-dev
docker push ghcr.io/mylab-tools/fs-cleaner:%1-dev

goto done

:noparam
echo "Please specify image version"
goto done

:done
echo "Done!"