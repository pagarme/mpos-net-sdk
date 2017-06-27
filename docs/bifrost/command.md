# Commands:

The exact structure of the commands depends on the used protocol. Here will be listed the commands and parameters available on all protocols:

**contextId:** The id of [context](architecture.md#context). Need to be the same on a transaction processing.

**requestType:** Step of the transaction processing.

### Request Types

- **1** or **listDevices**
- **2** or **initialize**
- **4** or **process**
- **5** or **finish**
- **6** or **displayMessage**
- **7** or **status**
- **8** or **close** 

### Response Types

- **1**: *devices listed*
- **2**: *initialized*
- **3**: *already initialized*
- **4**: *processed*
- **5**: *finished*
- **6**: *message displayed*
- **7**: *status*
- **8**: *closed*
- **9**: *error* 

Error handling depends on the protocol.

## List Devices

Lists all available devices.

### Response

An array with all available devices is returned. Each device may contain the following parameters:

**id:** Device ID. Different in each time the service runs.

**name:** Device name.

## Initialize

The first message to be sent is the initialize command.

### Request

**initialize.encryptionKey:** Pagar.me's encryption key

**initialize.deviceId:** Device to be used. First available will be used if not provided.

**initialize.baudRate:** The Baud Rate of the device to be user.

### Response

**responseType:** 2 for initialized, 3 if it has been initialized before.

## Status

Checks the bridge status.

### Response

**status.status:** Current bridge status, possible values are:
- **0**: *uninitialized*
- **1**: *ready*
- **2**: *in use*
- **3**: *closed*

**status.connectedDeviceId:** Connect device ID, if any.

**status.availableDevices:** Number of devices available in the host computer.

## Display Message

Displays a message, if the devices supports it.

### Request

**displayMessage.message:** Message to be displayed.

## Process Payment

Captures payment information and returns information about it:

### Request

**process.amount:** Transaction amount, in cents.

### Response

**process.status:** Payment status, possible values are:
- **0**: *accepted*
- **1**: *rejected*
- **2**: *errored*

**process.cardHash:** Card hash to be used when creating the transaction.

**process.cardHolderName:** Card holder name.

**process.paymentMethod:** Payment method selected, current possible values are:
- **1**: *credit card*
- **2**: *debit card*

## Finish Payment

Finishes the card processing. Calling this command after creating the transaction is **obligatory**, either in case of failure.

### Request

**finish.emvData:** card emv response from acquirer

**finish.success:** acquirer success

**finish.responseCode:** acquirer response code

## Close

Close the connection to the device.