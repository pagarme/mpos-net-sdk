# Commands:

The exact structure of the commands depends on the used protocol.

The parameters available on all protocols:

- **context_id:** The id of [context](architecture.md#context). Need to be the same on the entire transaction processing.

- **request_type:** Step of the transaction processing.

The parameters returned by protocols:

- **context_id:** The id of [context](architecture.md#context). Returns the value sent on request.

- **request_type:** Step of the transaction processing. Will be the same of request or ´Error´.

### Request Types

- **1** or **list_devices**
- **2** or **initialize**
- **4** or **process**
- **5** or **finish**
- **6** or **display_message**
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

## List Devices

Lists all available devices.

### Response

An array with all available devices is returned. Each device may contain the following parameters:

- **id:** Device ID. Different in each time the service runs.

- **name:** Device name.

## Initialize

Should be sent before any command, except List Devices.

### Request

- **initialize.encryption_key:** Pagar.me's encryption key

- **initialize.device_id:** Device to be used. First available will be used if not provided.

- **initialize.baudRate:** The Baud Rate of the device to be user.

- **initialize.simple_initialize:** Initialize mpos without updating tables. Usefull for browser configuration. Should not be used for transacting.

- **initialize.timeout_milliseconds:** Timeout to mpos initialize. Usefull for browser configuration, to detect if the right port is being used - wrong ports simply do not respond.

### Response

- **response_type:** 2 for initialized, 3 if it has been initialized before.

## Process Payment

Captures payment information and returns information about it:

### Request

- **process.amount:** Transaction amount, in cents.

- **process.applications:** list of applications that can be used to process transaction. If empty, the saved one will be used.

- **process.magstripe_payment_method:** when passing magstripe transactions, payment method can be passed. If empty, Credit will be called.

### Response

- **process.status:** Payment status, possible values are:
    - **0**: *accepted*
    - **1**: *rejected*
    - **2**: *errored*
    - **3**: *canceled*

- **process.card_hash:** Card hash to be used when creating the transaction.

- **process.card_holder_name:** Card holder name.

- **process.payment_method:** Payment method selected, current possible values are:
    - **1**: *credit card*
    - **2**: *debit card*

- **process.is_online_pin:** Password check online or saved on the card.

- **process.error_code:** Code of error reported by mpos.

## Finish Payment

Finishes the card processing. Calling this command after creating the transaction is **obligatory**, either in case of failure.

### Request

- **finish.emv_data:** card emv response from acquirer.

- **finish.success:** acquirer success.

- **finish.response_code:** acquirer response code.

## Display Message

Displays a message, if the devices supports it.

### Request

- **displayMessage.message:** Message to be displayed.

## Status

Checks the bridge status.

### Response

- **status.code:** Current bridge status code, possible values are:
    - **0**: *uninitialized*
    - **1**: *ready*
    - **2**: *in use*
    - **3**: *closed*

- **status.connected_device_id:** Connect device ID, if any.

- **status.available_devices:** Number of devices available in the host computer.

## Close

Close the connection to the device.