# How to create Docker Hyper-V VM for QA

**Requirements**: Windows 10 Pro / Enterprise with Hyper-V support

&nbsp;

## Enable Hyper-V Platform and Hyper-V Management Tools

---

https://docs.microsoft.com/en-us/virtualization/hyper-v-on-windows/quick-start/enable-hyper-v

&nbsp;

## Create Ubuntu VM in Hyper-V

---

1. Start Hyper-V Manager  
   Action -> Quick Create

2. Select "Ubuntu 18.04.1 LTS" or later version if available

3. Expand "More Options" at the bottom right corner  
   Enter VM name, e.g. "Foundry QA Docker"  
   Select network, "Default Switch"
4. Click "Create Virtual Machine"  
   Hyper-V Manager will download image from internet (if necessary) and will automatically create VM

&nbsp;

## Configure VM if necessary

---

1. Click "Edit settings..."

&nbsp;

## Connect to VM, configure and start Ubuntu installation

---

1. Click "Connect" to connect to VM and click "Start" button  
   Ubuntu installation will be started automatically

2. Select language -> Continue

3. Select Keyboard Layout -> Continue

4. Select / enter Location (for time zone) -> Continue

5. Enter user / computer information = > Continue

   > For example:  
   > User Name: QA Tester  
   > Your Computer's Name: Foundry-QA-VM  
   > Username: qatester  
   > Password: qa@test#123

6. Select "Login Automatically", so will not need to enter password every time  
   it will start Ubuntu installation and system configuration

   > It might pop a dialog "Connect to Foundry QA Docker", just close it after installation / configuration is finished, it will show dialog to do some configurations just click Next -> Next -> Next -> done

&nbsp;

## Remove unnecessary software

---

1. Ubuntu Desktop -> Show Applications (button at the bottom left) -> Ubuntu Software (usually on the second page)

   > as alternative can click "Ubuntu Software" button at the vertical panel at the left
   > Switch to "Installed" tab and uninstall all unnecessary software (games / editors / etc)

&nbsp;

## Update system

---

1. Ubuntu Desktop -> Show Applications (button at the bottom left) -> Software Updater  
   it will automatically check for updates

2. Click "Install Now"

3. Click "Restart Now" after done

&nbsp;

## Add External Hyper-V switch, so can access VM remotely

---

1. In Hyper-V manager  
   Action -> Virtual Switch Manager

2. At the right side select "External"  
   click "Create Virtual Switch"

3. Assign a name, for example "External Switch"

4. Make sure that "External Network" is selected  
   Select network adapter from drop-down (preferably WiFi if on notebook)  
   Ok - Yes
5. Select "Foundry QA VM"  
   Settings...
   - select "Add Hardware" at the left side and "Network Adapter" at the right side
     Add -> select "External Switch" from drop-down -> Ok
   - restart VM  
     Shutdown -> Start

&nbsp;

## Install OpenSSH Server

---

It is used to access Linux terminal remotely through SSH client.

1. Ubuntu Desktop -> Show Applications (button at the bottom left) -> Terminal  
   can right-click on Terminal -> Add to Favorites, so it will add it to favorites bar

2. Update packages info

```bash
sudo apt-get update
```

3. Install open ssh server

```bash
sudo apt-get install openssh-server
```

4. Check status

```bash
sudo service ssh status
```

5. Modify ssh configuration if necessary

```bash
sudo nano /etc/ssh/sshd_config
```

6. If added / modified ssh config, restart ssh service

```bash
sudo service ssh restart
```

&nbsp;

## Test ssh

---

1. In Hyper-V Manager select "Foundry QA Docker" VM and at the bottom switch to "Networking" tab  
   there you will see VM IP address, need to remember one for the "External Switch", for example: 10.0.1.23

2. on host Windows system, start either cmd or powershell

```bash
ssh [VM IP address] -l [user name]
```

for example:

```bash
ssh 10.0.1.23 -l qatester
```

After entering password it should open linux ssh remote terminal

> Highly recommend to install Cmder (http://cmder.net) console emulator for Windows
> it is absolutely free and greatly improve powershell and cmd productivity

> Sometimes somehow copy-paste from host Windows into Hyper-V Ubuntu VM does not work (clipboard is not shared)
> but using SSH instead of Ubuntu Desktop terminal solves the problem since SSH terminal is just a regular program in Windows

&nbsp;

## Install curl

---

Curl is used to download files from internet from terminal

1. Update packages info

```bash
sudo apt-get update
```

2. Install open ssh server

```bash
sudo apt-get install curl
```

&nbsp;

## Install Docker CE

---

1. Either start terminal in Ubuntu desktop or remote ssh from host Windows

2. Download Docker installation script

```bash
curl -fsSL https://get.docker.com -o get-docker.sh
```

3. Execute Docker installation script

```bash
sudo sh get-docker.sh
```

4. Add current user to the Docker group, so will not need to "sudo" when executing Docker commands

```bash
sudo usermod -aG docker [user name]
```

- for example:

```bash
sudo usermod -aG docker qatester
```

5. Might need to logout/login Ubuntu Desktop or ssh in order to take effect

6. Test installation

```bash
docker version
```

&nbsp;

## Install Docker-Compose

---

Docker-compose is a tool that allows to deploy and configure a bunch of docker containers using yaml configuration file

1. Either start terminal in Ubuntu desktop or remote ssh from host Windows

2. Download latest docker-compose  
   can check the latest version at https://github.com/docker/compose/releases

```bash
sudo curl -L "https://github.com/docker/compose/releases/download/1.23.2/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
```

3. Apply executable permissions to the binary

```bash
sudo chmod +x /usr/local/bin/docker-compose
```

4. If need create symlink (if cannot run docker-compose ... not found)

```bash
sudo ln -s /usr/local/bin/docker-compose /usr/bin/docker-compose
```

5. Test installation

```bash
docker-compose version
```

&nbsp;

## Test Docker containers

---

1. Either start terminal in Ubuntu desktop or remote ssh from host Windows

2. Start hello-world container  
   if it downloaded and started successfully, should see something like:
   > This message shows that your installation appears to be working correctly.

```bash
docker run hello-world:linux
```

3. List all containers.  
   Should see hello:world:linux container exited sometime ago

```bash
docker container ls -a
```

4. Remove test container
   > no need for full id, can just use first couple of characters from id

```bash
docker container rm -f [container id]
```

5. List all images
   > should see hello-world image with "linux" tag

```bash
docker image ls -a
```

6. Delete test image
   > no need for full id, can just use first couple of characters from id

```bash
docker image rm -f [image id or name:tag]
```
