dotnet pack src/Blazor.WebGL;

if [ ! -d "nuget-local" ]; then
    mkdir nuget-local;
fi

dotnet nuget push "src/Blazor.WebGL/bin/Debug/Blazor.WebGL.1.0.0.nupkg" -s "$(pwd)/nuget-local";