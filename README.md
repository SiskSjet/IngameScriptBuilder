![logo](IngameScriptBuilder/icon.ico)
# IngameScriptBuilder
A little tool that helps create ingame script for Space Engineers

## Note
Minification is in an experimental state. This can cause an not working script.

## Usage
```
Usage: ISB.exe [arguments] [options]

Arguments:
	Project  Directory which include all your source files or a .csproj project file
	Output   If empty script is generated in clipboard else to specified file

Options:
	-m | --minify                Minify the ingame script
	-r | --removeComments        Generate script without comments
	-rd | --removeDocumentation  Generate script without documentations
	-xd | --excludeDirectory     Exclude an directory
	-xf | --excludefile          Exclude a file
	-v | --version               Show version information
	-h | --help                  Show help information
```
#### Generate from C# project
ISB copies the generated script file to clipboard if no `Output` argument is set.

	ISB.exe ".\path\to\projectFile.csproj"
#### Generate from Directory
You can also specify a directory which host your .cs files

	ISB.exe ".\path\to\projectDirectory"

#### Export
By default ISB copy the generated script to the clipboard.
If you want export this to a file use the `Output` argument.

	ISB.exe ".\path\to\projectFile.csproj" ".\path\to\file.txt"

#### Exclude files
By default the `.\Properties\AssemblyInfo.cs` in the project root is excluded.

To exclude a file use the `-xf` option.

	ISB.exe ".\path\to\projectDirectory" -xf ".\path\to\unwantedFile.cs"
You can use multiple `-xf` to exclude multiple files.

	ISB.exe ".\path\to\projectDirectory" -xf ".\path\to\unwantedFile.cs" -xf ".\path\to\anotherUnwantedFile.cs"

#### Exclude directories
To exclude a directory use the `-xd` option.

	ISB.exe ".\path\to\projectDirectory" -xd ".\path\to\unwantedDirectory"

Like files you can exclude multiple directories by using multiple `-xd`

	ISB.exe ".\path\to\projectDirectory" -xd ".\path\to\unwantedDirectory" -xd ".\path\to\anotherUnwantedDirectory"
