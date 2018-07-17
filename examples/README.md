# TestGame

Not really a game right now, but this project is used to test the Blazor.WebGL library.

## Building

Firstly build the Blazor.WebGL library using VS Code tasks (use `Ctrl+Shift+B` choose `build - Blazor.WebGL` task). At the root of the repository run `publish-local.sh` script.

```sh
# Run the publish script
./publish-local.sh
```

Then using VS Code tasks (use `Ctrl+Shift+P` find `Tasks: Run task` command) execute `run - TestGame` task. 