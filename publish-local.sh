dotnet pack src/Blazor.WebGL;

if [ ! -d "nuget-local" ]; then
    mkdir nuget-local;
fi

dotnet nuget push "src/Blazor.WebGL/bin/Debug/Blazor.WebGL.0.0.1-pre000001.nupkg" -s "$(pwd)/nuget-local";