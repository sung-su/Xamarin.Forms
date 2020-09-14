#!/bin/bash -e

SCRIPT_FILE=$(readlink -f $0)
SCRIPT_DIR=$(dirname $SCRIPT_FILE)

NUSPEC_DIR="$SCRIPT_DIR/.nuspec"

BUILD_CONF=Debug

get_version() {
  PRENAME=$1
  if [ -z "$PRENAME" ]; then
    PRENAME="local"
  fi
  LOCAL_VERSION=9.9.9
  VERSION=$LOCAL_VERSION-$PRENAME-$((10000 + $(git rev-list --count HEAD)))
  echo $VERSION
}

cmd_clean() {
  dotnet clean Xamarin.Forms.Tizen.sln
  if [ "$1" == "all" ]; then
    rm -fr $NUSPEC_DIR/netstandard2.0 $NUSPEC_DIR/net46
    rm -f $SCRIPT_DIR/Xamarin.Forms.*.nupkg
  fi
  rm -fr $NUSPEC_DIR/bin $NUSPEC_DIR/obj
}

# Creates a solution file and build using dotnet cli of .NETCore.
cmd_build() {
  dotnet new sln -n Xamarin.Forms.Tizen
  dotnet sln Xamarin.Forms.Tizen.sln add Xamarin.Forms.Platform/Xamarin.Forms.Platform.csproj Xamarin.Forms.Core/Xamarin.Forms.Core.csproj Xamarin.Forms.Maps/Xamarin.Forms.Maps.csproj Xamarin.Forms.Platform.Tizen/Xamarin.Forms.Platform.Tizen.csproj Xamarin.Forms.Xaml/Xamarin.Forms.Xaml.csproj Xamarin.Forms.Build.Tasks/Xamarin.Forms.Build.Tasks.Tizen.csproj Xamarin.Forms.Maps.Tizen/Xamarin.Forms.Maps.Tizen.csproj Stubs/Xamarin.Forms.Platform.Tizen/Xamarin.Forms.Platform.Tizen\ \(Forwarders\).csproj
  dotnet build -c $BUILD_CONF Xamarin.Forms.Tizen.sln
}

cmd_build_coverage() {
  git checkout -t origin/coverage
  dotnet new sln -n Xamarin.Forms.Tizen
  dotnet sln Xamarin.Forms.Tizen.sln add Xamarin.Forms.Platform/Xamarin.Forms.Platform.csproj Xamarin.Forms.Core/Xamarin.Forms.Core.csproj Xamarin.Forms.Maps/Xamarin.Forms.Maps.csproj Xamarin.Forms.Platform.Tizen/Xamarin.Forms.Platform.Tizen.csproj Xamarin.Forms.Xaml/Xamarin.Forms.Xaml.csproj Xamarin.Forms.Build.Tasks/Xamarin.Forms.Build.Tasks.Tizen.csproj Xamarin.Forms.Maps.Tizen/Xamarin.Forms.Maps.Tizen.csproj Stubs/Xamarin.Forms.Platform.Tizen/Xamarin.Forms.Platform.Tizen\ \(Forwarders\).csproj
  dotnet build -c $BUILD_CONF Weavers.sln
  dotnet build -c $BUILD_CONF Xamarin.Forms.Tizen.sln
}

cmd_pack() {
  VERSION=$1; shift
  if [ -z "$VERSION" ]; then
    pushd $SCRIPT_DIR > /dev/null
    VERSION=$(get_version)
    popd > /dev/null
  fi
    dotnet msbuild -nologo /t:Restore $NUSPEC_DIR/pack.csproj
    dotnet msbuild -nologo /t:Pack $NUSPEC_DIR/pack.csproj /p:Version=$VERSION /p:Configuration=$BUILD_CONF $@
    dotnet msbuild -nologo /t:Restore $NUSPEC_DIR/pack_maps.csproj
    dotnet msbuild -nologo /t:Pack $NUSPEC_DIR/pack_maps.csproj /p:Version=$VERSION /p:Configuration=$BUILD_CONF $@
}

cmd=$1; [ $# -gt 0 ] && shift;
case "$cmd" in
  clean) cmd_clean $@ && exit 0 ;;
  build | "") cmd_build $@ && exit 0 ;;
  build-coverage | "") cmd_build_coverage $@ && exit 0 ;;
  pack) cmd_pack $@ && exit 0 ;;
  version) get_version $@ && exit 0 ;;
  *) echo "Invalid command" && exit 1 ;;
esac
