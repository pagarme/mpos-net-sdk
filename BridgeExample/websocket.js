var callWS = function(contextId, devicePort, encryptionKey, baudRate) {
	var instance = new webSocket(contextId, devicePort, encryptionKey, baudRate);
	instance.call();
}

var webSocket = function (contextId, devicePort, encryptionKey, baudRate) {
	
	this.contextId = contextId;
	this.devicePort = devicePort;
	this.encryptionKey = encryptionKey;
	this.baudRate = baudRate;

	this.response = {
		unknownCommand: 0,
		devicesListed: 1,
		initialized: 2,
		alreadyInitialized: 3,
		processed: 4,
		finished: 5,
		messageDisplayed: 6,
		closed: 7,
		error: 8,
	};
	
	this.request = {
		listDevices: 1,
		initialize: 2,
		process: 3,
		finish: 4,
		displayMessage: 5,
		close: 6,
	};
	
	this.amount = 0;
	this.method = null;
	this.ws = null;

	this.call = function() {
		if ("WebSocket" in window)
		{			
			this.setValues();
			
			var valid = this.validate();
			
			if (!valid)
				return;
			
			this.ws = new WebSocket("ws://localhost:2000/mpos");

			this.ws.parent = this;
			this.ws.onopen = this.open;		
			this.ws.onmessage = this.handleResponse;
			this.ws.onclose = this.close;
			this.ws.onerror = this.error;
		}

		else
		{
			alert("WebSocket NOT supported by your Browser!");
		}
	};
	
	this.setValues = function() {
		this.amount = document.getElementById("amount").value;
		
		this.method = 
			document.getElementById("Credit").checked ? "Credit" :
			document.getElementById("Debit").checked ? "Debit" :
			null;
	};
	
	this.validate = function() {
		var message = "";
		var valid = true;
	
		if (isNaN(this.amount) || this.amount <= 0)
		{
			message += "\n- Invalid amount";
			valid = false;
		}
		if (this.method == null)
		{
			message += "\n- No method chosen";
			valid = false;
		}
		
		if (!valid)
		{
			alert("Errors:" + message);
		}
		
		return valid;
	};
	
	this.open = function(a,b,c,d) {
		//document.getElementById("sender").disabled = true;
		this.parent.listDevices();
	};
	
	this.handleResponse = function (response) {
		var responseContent = JSON.parse(response.data);
		
		switch(responseContent.ResponseType)
		{
			case (this.parent.response.devicesListed):
				this.parent.initialize(responseContent);
				break;

			case (this.parent.response.initialized):
			case (this.parent.response.alreadyInitialized):
				
				this.parent.process();
				break;

			case (this.parent.response.processed):
				this.parent.finish(responseContent);
				break;

			default:
				this.close();

				var message = this.parent.getEndingMessage(responseContent);
				if (message) alert(message);

				break;
		}
		
	};
	
	this.getEndingMessage = function (response)
	{
		switch(response.ResponseType)
		{
			case this.response.finished:
				return "Payment Succeded";
				
			case this.response.error:
				return response.Error;
				
			case this.response.unknownCommand:
				return "Unknown Request";
				
			default:
				return "Unknown Response";
		}
	};
	
	this.close = function() {
		document.getElementById("sender").disabled = false;
	};
	
	this.error = function(){
		alert("Url '" + this.url + "' not found or disconnected");
		this.close();
	};
	
	this.listDevices = function() {

		var request = {
			RequestType: this.request.listDevices,
			ContextId: this.contextId,
		};
	
		this.sendMessage(request);
	};
	
	this.initialize = function(response) {

		var devices = response.DeviceList;
		var deviceId = null;
	
		for(var d = 0; d < devices.length; d++)
		{
			if (devices[d].Port == this.devicePort)
			{
				deviceId = devices[d].Id;
			}
		}
	
		var request = {
			RequestType: this.request.initialize,
			ContextId: this.contextId,
			Initialize: {
				DeviceId: deviceId,
				EncryptionKey: this.encryptionKey,
				BaudRate: this.baudRate
			}
		};
	
		this.sendMessage(request);
	};
	
	this.process = function() {

		var request = {
			RequestType: this.request.process,
			ContextId: this.contextId,
			Process: {
				Amount: this.amount * 100,
				MagstripePaymentMethod: this.method
			}
		};
	
		this.sendMessage(request);
	};
	
	this.finish = function(response) {

		var request = {
			RequestType: this.request.finish,
			ContextId: this.contextId,
			Finish: {
				Success: true,
				ResponseCode: "0000",
				EmvData: "000000000.0000"
			}
		};
	
		this.sendMessage(request);
	};

	this.sendMessage = function(request) {
		this.ws.send(JSON.stringify(request));
	}

	
};