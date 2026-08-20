from ghcr.io/flavourous/build:avalonia-android as build
arg config=release
arg version=0.0.0
arg versionCode=1

run mkdir /build
workdir /build
copy . /build

run dotnet publish -f:net10.0-android -c:${config} -p:ApplicationDisplayVersion=${version} -p:ApplicationVersion=${versionCode} TagTag.Android/TagTag.Android.csproj

from scratch as out
arg config=release
copy --from=build /build/TagTag.Android/bin/${config}/net10.0-android/publish/*-Signed.apk /
