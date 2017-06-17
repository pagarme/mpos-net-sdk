@echo off

certutil.exe -d %1 -D -n Bifrost
certutil.exe -d %1 -A -t "TCu,Cuw,Tuw" -n Bifrost -i %2\%3.crt
