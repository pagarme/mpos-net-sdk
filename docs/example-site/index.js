var devicePort = getLocal("device-port");
var baudRate = getLocal("baud-rate");

function init () {
  getById("device-port").value = devicePort;
  getById("baud-rate").value = baudRate;
}

init();

function callWS () {
  const contextId = getById("context-id").value;
  const wsWrap = new webSocketWrap(contextId);

  setValues(wsWrap);

  const valid = validate(wsWrap);

  if (!valid)
	return;

  wsWrap.call(wsWrap.listDevices, handleResponse);
}

function setValues (wsWrap) {
  wsWrap.amount = getById("amount").value;

  wsWrap.method =
    getById("credit").checked ? "Credit" :
    getById("debit").checked ? "Debit" :
    null;
};

function validate (wsWrap) {
  let message = "";
  let valid = true;

  if (isNaN(wsWrap.amount) || wsWrap.amount <= 0) {
    message += "\n- Invalid amount";
    valid = false;
  }

  if (wsWrap.method == null) {
    message += "\n- No method chosen";
    valid = false;
  }

  if (!valid) {
    showMessage("Errors:" + message);
  }

  return valid;
};

function handleResponse (response) {

  const ws = this;
  const wsWrap = ws.parent;

  const responseJson = JSON.parse(response.data);

  switch (responseJson.response_type) {
    case wsWrap.response.devicesListed:
	  initialize(wsWrap, responseJson);
  	  break;

    case wsWrap.response.initialized:
    case wsWrap.response.alreadyInitialized:
  	  wsWrap.process();
  	  break;

    case wsWrap.response.processed:
  	  wsWrap.finish(responseJson);
	  break;

    case wsWrap.response.closed:
  	  return;

    default:
  	  ws.close();

  	  const message = getEndingMessage(wsWrap, responseJson);
  	  if (message) showMessage(message);

  	  break;
  }
};

function initialize (wsWrap, responseJson) {
  const encryptionKey = getById("encryption-key").value;

  const deviceId = getDevice(wsWrap, responseJson);

  if (deviceId != null)
	wsWrap.initialize(encryptionKey, deviceId, baudRate);
}

function getDevice (wsWrap, responseJson) {
  const devices = responseJson.device_list;

  for(let d = 0; d < devices.length; d++) {
    if (devices[d].port == devicePort) {
      return devices[d].id;
    }
  }

  showMessage("Port " + devicePort + " not found");

  wsWrap.ws.close();
  wsWrap.close();

  return null;
}

function getEndingMessage (wsWrap, responseJson) {
  switch (responseJson.response_type) {
    case wsWrap.response.finished:
      return "Payment Succeded";

    case wsWrap.response.error:
      return responseJson.error;

    case wsWrap.response.unknownCommand:
      return "Unknown Request";

    default:
      return "Unknown Response";
  }
};