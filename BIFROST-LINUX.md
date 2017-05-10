#Install Mono Service to run as service at Linux - need .NET 4.5

sudo apt install mono-complete
sudo apt install mono-4.0-service

#Command to initialize service

sudo mono-service share/ubuntu-service/service/PagarMe.Mpos.Bridge.Service.exe

#Command to stop service normally

sudo kill $(sudo cat /tmp/PagarMe.Mpos.Bridge.Service.exe.lock)

#Command to remove lock of service when it ends up with exception

sudo rm /tmp/PagarMe.Mpos.Bridge.Service.exe.lock

#Command to see Bifrost log

sudo cat .config/PagarMe.Mpos.Bridge/{yyyy}-{mm}-{dd}.log