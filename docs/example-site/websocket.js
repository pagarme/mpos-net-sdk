var callWS = function() {
  var contextId = getById("context-id").value;
  var devicePort = getById("device-port").value;
  var encryptionKey = getById("encryption-key").value;
  var baudRate = getById("baud-rate").value;

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
    status: 7,
    closed: 8,
    error: 9,
  };

  this.request = {
    listDevices: 1,
    initialize: 2,
    process: 4,
    finish: 5,
    displayMessage: 6,
    status: 7,
    close: 8,
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

      this.ws = new WebSocket("wss://localhost:2000/mpos");

      this.ws.parent = this;
      this.ws.onopen = this.open;		
      this.ws.onmessage = this.handleResponse;
      this.ws.onclose = this.close;
      this.ws.onerror = this.error;
    }

    else
    {
      this.showMessage("WebSocket NOT supported by your Browser!");
    }
  };

  this.setValues = function() {
    this.amount = getById("amount").value;

    this.method = 
      getById("credit").checked ? "Credit" :
      getById("debit").checked ? "Debit" :
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
      this.showMessage("Errors:" + message);
    }

    return valid;
  };

  this.open = function() {
    this.parent.listDevices();
  };

  this.handleResponse = function (response) {

    var responseContent = JSON.parse(response.data);

    switch(responseContent.response_type)
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
      case (this.parent.response.closed):
        return;

      default:
        this.close();

        var message = this.parent.getEndingMessage(responseContent);
        if (message) this.parent.showMessage(message);

        break;
    }

  };

  this.getEndingMessage = function (responseContent) {
    switch(responseContent.response_type)
    {
      case this.response.finished:
        return "Payment Succeded";

      case this.response.error:
        return responseContent.error;

      case this.response.unknownCommand:
        return "Unknown Request";

      default:
        return "Unknown Response";
    }
  };

  this.showMessage = function(message) {
    var messages = getById("messages").innerHTML;
    messages = "<div><pre>" + message + "</pre></div>" + messages;

    getById("messages").innerHTML = messages;
  };

  this.close = function() {
  };

  this.error = function(){
    this.parent.showMessage("Url '" + this.url + "' not found or disconnected");
    this.close();
  };

  this.listDevices = function() {

    var request = {
      request_type: this.request.listDevices,
      context_id: this.contextId,
    };

    this.sendMessage(request);
  };

  this.initialize = function(response) {

    var devices = response.device_list;
    var deviceId = null;

    for(var d = 0; d < devices.length; d++)
    {
      if (devices[d].port == this.devicePort)
      {
        deviceId = devices[d].id;
      }
    }

    if (deviceId == null)
    {
      this.showMessage("Port " + this.devicePort + " not found");

      this.ws.close();
      this.close();

      return;
    }

    var request = {
      request_type: this.request.initialize,
      context_id: this.contextId,
      initialize: {
        device_id: deviceId,
        encryption_key: this.encryptionKey,
        baud_rate: this.baudRate
      }
    };

    this.sendMessage(request);
  };

  this.process = function() {

    var request = {
      request_type: this.request.process,
      context_id: this.contextId,
      process: {
        amount: this.amount * 100,
        magstripe_payment_method: this.method
      }
    };

    this.sendMessage(request);
  };

  this.finish = function(response) {

    var request = {
      request_type: this.request.finish,
      context_id: this.contextId,
      finish: {
        success: true,
        response_code: "0000",
        emv_data: "000000000.0000"
      }
    };

    this.sendMessage(request);
  };

  this.sendMessage = function(request) {
    var message = JSON.stringify(request);
    this.ws.send(message);
  }

};

var getById = function(id) {
	return document.getElementById(id);
}