# TestGame

Not really a game right now, but this project is used to test the Blazor.WebGL library.

## Building

**Building an example assumes that you already have [built Blazor.WebGL library](../../#building).**

At the root of the repository run `publish-local.sh` script.

```sh
# Run the publish script
./publish-local.sh
```

Then using VS Code tasks (use `Ctrl+Shift+P` find `Tasks: Run task` command) execute `build - TestGame` task. 

## Running

Using VS Code tasks execute `run - TestGame` task. Open your browser and go to the http://localhost:11510/ page.
