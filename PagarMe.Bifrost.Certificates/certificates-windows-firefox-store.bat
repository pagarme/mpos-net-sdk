@echo off

certutil.exe -d %1 -D -n %3
certutil.exe -d %1 -A -n %3 -i %2\%3.crt -t "%4"
