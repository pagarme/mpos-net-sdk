var wsWrap = null
var devices = null

function init () {
  setIfNull('baud-rate')

  const contextId = getById('context-id').value
  wsWrap = new WebSocketWrap(contextId)

  getPortList()
}

function setIfNull (name) {
  const currentValue = getById(name).value

  if (!currentValue) {
    getById(name).value = getLocal(name)
  }
}

function getPortList () {
  wsWrap.call(wsWrap.listDevices, handleResponse)
}

function handleResponse (response) {
  const responseJson = JSON.parse(response.data)

  switch (responseJson.response_type) {
    case wsWrap.response.devicesListed:
      populateDeviceList(responseJson)
      break

    case wsWrap.response.alreadyInitialized:
    case wsWrap.response.initialized:
      wsWrap.displayMessage("Configurado")
      break

    case wsWrap.response.messageDisplayed:
      showMessage("Verifique o visor da mpos")
      toggleButton("save", true)
      break

    case wsWrap.response.contextClosed:
      break

    default:
      wsWrap.closeContext()

      const message = getEndingMessage(wsWrap, responseJson)
      if (message) showMessage(message)

      break
  }
}

function populateDeviceList (responseJson) {
  devices = responseJson.device_list
  const devicePortSelect = getById('device-port')

  if (devices.length === 0) {
    devicePortSelect.innerHTML = '<option value="">---</option>'
    showMessage('Não foram encontradas portas')
    toggleButton("test", false)
    toggleButton("save", false)
    return
  }

  devicePortSelect.innerHTML = '<option value="">-- Selecione --</option>'
  const chosenPort = getLocal('device-port')

  for(let d = 0; d < devices.length; d++) {
    const selected = chosenPort === devices[d].port ? 'selected' : ''

    devicePortSelect.innerHTML +=
      '<option value="' + devices[d].port + '"'
        + selected +
      '>'
        + devices[d].port +
      '</option>'
  }

  toggleButton("test", true)
}

function toggleButton (id, enabled) {
  const button = getById(id)
  button.className = 'btn btn-' + (enabled ? 'primary' : 'mute')
  button.disabled = !enabled
}

function getEndingMessage (wsWrap, responseJson) {
  switch (responseJson.response_type) {
    case wsWrap.response.error:
      return responseJson.error

    case wsWrap.response.unknownCommand:
      return 'Comando desconhecido'

    default:
      return 'Resposta desconhecida'
  }
}

function testConfig () {
  const encryptionKey = ''

  if (!devices) {
    showMessage('Carregue as opções primeiro')
    return
  }

  const devicePort = getSelected(getById('device-port'))

  if (!devicePort) {
    showMessage('Porta inválida')
    return
  }

  const baudRate = getById('baud-rate').value
  
  if (!baudRate) {
    showMessage('Taxa de transmissão inválida')
    return
  }
  
  const timeout = getById('timeout-milliseconds').value

  if (isNaN(timeout)) {
    showMessage('Tempo limite inválido')
    return
  }

  for(let d = 0; d < devices.length; d++) {
    if (devices[d].port === devicePort) {
      wsWrap.initialize('', devices[d].id, baudRate, true, timeout)
    }
  }

  if (!devices) {
    showMessage('Porta ' + devicePort + ' não encontrada')
  }
}

function saveAndFinish () {
  wsWrap.closeContext()
  setLocal('device-port', getSelected(getById('device-port')))
  setLocal('baud-rate', getById('baud-rate').value)
  toggleButton("test", false)
  toggleButton("save", false)
  showMessage('Configurações salvas no navegador')
}

init()
