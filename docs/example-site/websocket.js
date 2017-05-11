var callWS = function() {
  var contextId = document.getElementById("contextId").value;
  var devicePort = document.getElementById("devicePort").value;
  var encryptionKey = document.getElementById("encryptionKey").value;
  var baudRate = document.getElementById("baudRate").value;

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
      this.showMessage("Errors:" + message);
    }

    return valid;
  };

  this.open = function() {
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
        if (message) this.parent.showMessage(message);

        break;
    }

  };

  this.getEndingMessage = function (response) {
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

  this.showMessage = function(message) {
    var messages = document.getElementById("messages").innerHTML;
    messages = "<div><pre>" + message + "</pre></div>" + messages;

    document.getElementById("messages").innerHTML = messages;
  };

  this.close = function() {
  };

  this.error = function(){
    this.parent.showMessage("Url '" + this.url + "' not found or disconnected");
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

    if (deviceId == null)
    {
      this.showMessage("Port " + this.devicePort + " not found");

      this.ws.close();
      this.close();

      return;
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
    var message = JSON.stringify(request);
    this.ws.send(message);
  }


};