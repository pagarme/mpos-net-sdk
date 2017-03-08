# Commands:

The exact structure of the commands depends on the used protocol. Here will be listed the commands and parameters available on all protocols:

Error handling depends on the protocol.

## List Devices

Lists all available devices.

### Response

An array with all available devices is returned. Each device may contain the following parameters:

**id:** Device ID.

**name:** Device name.

**manufacturer:** Device manufacturer.

## Initialize

The first message to be sent is the initialize command.

### Request

**encryption_key:** Pagar.me's encryption key

**device_id:** Device to be used. First available will be used if not provided.

## Status

Checks the bridge status.

### Response

**status:** Current bridge status, possible values are: `idle`, `initialized`, `in_use`.

**connected_device_id:** Connect device ID, if any.

**available_devices:** Number of devices available in the host computer.

## Display Message

Displays a message, if the devices supports it.

### Request

**message:** Message to be displayed.

## Process Payment

Captures payment information and returns information about it:

### Request

**amount:** Transaction amount, in cents.

### Response

**status:** Payment status, possible values are: `accepted`, `rejected`.

**card_hash:** Card hash to be used when creating the transaction.

**card_holder_name:** Card holder name.

**payment_method:** Payment method selected, current possible values are `credit_card` and `debit_card`.

## Finish Payment

Finishes the card processing. Calling this command after creating the transaction is **obligatory**, either in case of failure.

### Request

**issuer_script:**

