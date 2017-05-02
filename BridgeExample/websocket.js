var webSocket = {

	amount: 0,
	method: null,
	ws: null,

	response: {
		unknownCommand: 0,
		processed: 1,
		finished: 2,
		error: 3
	},
	
	request: {
		process: 1,
		finish: 2
	},

	call: function() {
		if ("WebSocket" in window)
		{
			this.setValues();
			
			var valid = this.validate();
			
			if (!valid)
				return;
			
			this.ws = new WebSocket("ws://localhost:2000/mpos");
				
			this.ws.onopen = this.open;		
			this.ws.onmessage = this.handleResponse;
			this.ws.onclose = this.close;
			this.ws.onerror = this.error;
		}

		else
		{
			alert("WebSocket NOT supported by your Browser!");
		}
	},
	
	setValues: function() {
		this.amount = document.getElementById("amount").value;
		
		this.method = 
			document.getElementById("Credit").checked ? "Credit" :
			document.getElementById("Debit").checked ? "Debit" :
			null;
	},
	
	validate: function() {
		var message = [];
		var valid = true;
	
		if (isNaN(this.amount) || this.amount <= 0)
		{
			message.add("Invalid amount");
			valid = false;
		}
		if (this.method == null)
		{
			message.add("No method chosen");
			valid = false;
		}
		
		if (!valid)
		{
			alert(message);
		}
		
		return valid;
	},
	
	open: function() {
		document.getElementById("sender").disabled = true;
		webSocket.callProcess();
	},
	
	handleResponse: function (response) {
		var info = JSON.parse(response.data);
		var that = webSocket;
		
		switch(info.ResponseType)
		{
			case that.response.processed:
				that.callFinish();
				break;
				
			case that.response.finished:
				that.ws.close();
				break;
				
			case that.response.error:
				alert(info.Message);
				break;
				
			case that.response.unknownCommand:
				alert("Unknown Request");
				break;
				
			default:
				alert("Unknown Response");
				break;
		}
	},
	
	close: function() {
		alert("Payment Succeded");
		document.getElementById("sender").disabled = false;
	},
	
	error: function(){
		alert("Url '" + webSocket.ws.url + "' not found or disconnected");
	},
	
	callProcess: function() {
		var that = webSocket;

		var requestInit = {
			RequestType: that.request.process,
			Process: {
				Amount: that.amount * 100,
				MagstripePaymentMethod: that.method
			}
		};
	
		that.ws.send(JSON.stringify(requestInit));
	},
	
	callFinish: function() {
		var that = webSocket;

		var requestFinish = {
			RequestType: that.request.finish,
			Finish: {
				Success: true,
				ResponseCode: "0000",
				EmvData: "000000000.0000"
			}
		};
	
		that.ws.send(JSON.stringify(requestFinish));
	}

}