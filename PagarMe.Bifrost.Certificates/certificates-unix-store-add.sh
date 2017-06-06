#!/bin/bash
apt-get update
apt-get install libnss3-tools

#Export for future use
openssl pkcs12 -export -out $1/$2.pfx -inkey $1/$2.key -in $1/$2.crt -passin pass:"" -passout pass:""

# Chrome
certutil -d sql:$HOME/.pki/nssdb -D -n Bifrost 2> /dev/null
certutil -d sql:$HOME/.pki/nssdb -A -t "P,," -n Bifrost -i $1/$2.crt

# Firefox
certutil -d $(find ~/.mozilla/ -name cert*.db | sed s/cert[0-9].db//g | sort | uniq) -D -n Bifrost 2> /dev/null
certutil -d $(find ~/.mozilla/ -name cert*.db | sed s/cert[0-9].db//g | sort | uniq) -A -t "TCu,Cuw,Tuw" -n Bifrost -i $1/$2.crt
