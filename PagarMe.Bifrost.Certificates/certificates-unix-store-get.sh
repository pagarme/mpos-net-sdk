#!/bin/bash

if [ -f "/etc/redhat-release" ]; then
    yum install -y nss-tools
elif [ -f "/etc/debian_version" ]; then
    apt-get update
    apt-get install libnss3-tools -y
else
    distro=$(lsb_release -s | cut -d: -f2 | sed s'/^\s*//g')
    if [ "$distro" == "Arch Linux" ]; then
        pacman -S --noconfirm --force nss
    fi
fi

certutil -L -d sql:$HOME/.pki/nssdb -n "Bifrost" -a > $1
