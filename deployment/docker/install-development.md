# Docker Install for Development

**Requirements**: Windows 10 Pro / Enterprise

\
**Install Docker for Windows following instructions**

https://docs.docker.com/docker-for-windows/install/

\
**Install DockerCompletion powershell extension for docker**

1. Start powershell (as admin), need to start real powershell

2. Set the script execution policy to allow downloaded scripts signed by trusted publishers to run on your computer

```bash
Set-ExecutionPolicy RemoteSigned
```

3. Install the DockerCompletion PowerShell module for auto-completion of Docker commands (answer Y if it asks to install NuGet manager)

```bash
Install-Module DockerCompletion
Import-Module DockerCompletion
```

4. Start powershell for regular user  
   To make tab completion persistent across all PowerShell sessions, add the command to a $PROFILE by typing these commands at the PowerShell prompt.  
   Create / edit $PROFILE in Notepad

```bash
Notepad $PROFILE
```

5. Add the following to profile:

```bash
Import-Module DockerCompletion
```

\
**Install console emulator**

it's absolutely free and greatly improves work with cmd and powershell  
http://cmder.net

\
**Please, read, getting started guide**

https://docs.docker.com/get-started/

> if you play with some test / demo images you might want to clear them After, so they will not take disk space

1. List all images

```bash
docker image ls -a
```

2. Force remove image

```bash
docker image rm [image id ... you do not need to copy the whole id, just the 1st couple of chars]
```
