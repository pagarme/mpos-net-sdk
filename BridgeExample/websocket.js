var callWS = function() {
	var instance = new webSocket();
	instance.call();
}

var webSocket = function () {

	this.response = {
		unknownCommand: 0,
		processed: 1,
		finished: 2,
		error: 3
	};
	
	this.request = {
		process: 1,
		finish: 2
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
		this.parent.callProcess();
	};
	
	this.handleResponse = function (response) {
		var info = JSON.parse(response.data);
		
		if (info.ResponseType == this.parent.response.processed)
		{
			this.parent.callFinish();
		}
		else
		{
			this.close();
			alert(this.parent.getEndingMessage(info));
		}
	};
	
	this.getEndingMessage = function (info, that)
	{
		switch(info.ResponseType)
		{
			case this.response.finished:
				return "Payment Succeded";
				
			case this.response.error:
				return info.Error;
				
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
	
	this.callProcess = function() {

		var requestInit = {
			RequestType: this.request.process,
			Process: {
				Amount: this.amount * 100,
				MagstripePaymentMethod: this.method
			}
		};
	
		this.ws.send(JSON.stringify(requestInit));
	};
	
	this.callFinish = function() {

		var requestFinish = {
			RequestType: this.request.finish,
			Finish: {
				Success: true,
				ResponseCode: "0000",
				EmvData: "000000000.0000"
			}
		};
	
		this.ws.send(JSON.stringify(requestFinish));
	};

};