var wsWrap = null
var devices = null

function init () {
  setIfNull('baud-rate')

  const contextId = getById('context-id').value
  wsWrap = new WebSocketWrap(contextId)

  getDeviceList()
}

function setIfNull (name) {
  const currentValue = getById(name).value

  if (!currentValue) {
    getById(name).value = getLocal(name)
  }
}

function getDeviceList () {
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
  const deviceNameSelect = getById('device-name')

  if (devices.length === 0) {
    deviceNameSelect.innerHTML = '<option value="">---</option>'
    showMessage('Não foram encontrados dispositivos')
    toggleButton("test", false)
    toggleButton("save", false)
    return
  }

  deviceNameSelect.innerHTML = '<option value="">-- Selecione --</option>'
  const chosenDevice = getLocal('device-name')

  for(let d = 0; d < devices.length; d++) {
    const selected = chosenDevice === devices[d].name ? 'selected' : ''

    deviceNameSelect.innerHTML +=
      '<option value="' + devices[d].name + '"'
        + selected +
      '>'
        + devices[d].name +
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

  const deviceName = getSelected(getById('device-name'))

  if (!deviceName) {
    showMessage('Dispositivo inválido')
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
    if (devices[d].name === deviceName) {
      wsWrap.initialize('', devices[d].id, baudRate, true, timeout)
    }
  }

  if (!devices) {
    showMessage('Dispositivo ' + deviceName + ' não encontrado')
  }
}

function saveAndFinish () {
  wsWrap.closeContext()
  setLocal('device-name', getSelected(getById('device-name')))
  setLocal('baud-rate', getById('baud-rate').value)
  toggleButton("test", false)
  toggleButton("save", false)
  showMessage('Configurações salvas no navegador')
}

init()
