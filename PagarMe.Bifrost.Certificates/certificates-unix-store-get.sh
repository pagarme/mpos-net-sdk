#!/bin/bash
apt-get update
apt-get install libnss3-tools

certutil -L -d sql:$HOME/.pki/nssdb -n "Bifrost" -a > $1
