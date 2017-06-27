# Commands to execute Bifrost on Linux

## Install Mono Service to run as service at Linux - need .NET 4.5

* sudo apt install mono-complete
* sudo apt install mono-4.0-service

## Initialize service

* sudo mono-service [path-to-the-service]/PagarMe.Bifrost.Service.exe

## Stop service normally

* sudo kill $(sudo cat /tmp/PagarMe.Bifrost.Service.exe.lock)

## Remove lock of service when it ends up with exception

* sudo rm /tmp/PagarMe.Bifrost.Service.exe.lock

## Bifrost log

* sudo cat .config/PagarMe.Bifrost/{yyyy}-{mm}-{dd}.log