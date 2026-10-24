# SecretEnv

Converts a .NET user secrets JSON file into systemd `Environment=` lines.

## Usage

```
SecretEnv "%APPDATA%\Microsoft\UserSecrets\<id>\secrets.json"
```

Or run with no arguments to be prompted for the path. Pipe to `Set-Clipboard` to copy the output.

## Publishing

```
cd SecretEnv
dotnet publish -p:PublishProfile=SelfContainedWinX64
```

Publishes to `%LocalAppData%\Programs\SecretEnv\`. Add that folder to your PATH once:

```powershell
[Environment]::SetEnvironmentVariable("Path", $env:Path + ";$env:LocalAppData\Programs\SecretEnv", "User")
```
