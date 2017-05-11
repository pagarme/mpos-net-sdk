# WebSocket

A websocket endpoint is provided on `/mpos`.

# Protocol

All messages are encoded in JSON. All request messages have the following parameter:**

**contextId:** The id of [context](architecture.md#context). Need to be the same on a transaction processing.

**requestType:** Step of the transaction processing.

All responses contains the following parameters:**

**responseType:** Type of the response, which can be success response, error on process or unknown command, in case of request type unknown.

**error:** Error message if any ocurred. Response Type, in this caso, will be 9.

In this documentation only request/response examples will be provided. For information on actual parameters, check [this](command.md).

## List Devices

**RequestType:** "1" or "listDevices"

### Request

```json
{
	"RequestType": 1,
	"ContextId": "42"
}
```

### Response

```json
{
	"ResponseType": 1,
	"DeviceList": [{
		"Port": "COM3",
		"Id": "01234567-89AB-CDEF-FEDC-BA9876543210",
		"Name": "Serial Device (COM3)",
	}]
}
```

## Initialize

**RequestType:** "2" or "initialize"

### Request

```json
{
	"RequestType": 2,
	"ContextId": "42",
	"Initialize": {
		"DeviceId": "01234567-89AB-CDEF-FEDC-BA9876543210",
		"EncryptionKey": "ek_test_f9cws0bU9700VqWE4UDuBlKLbvX4IO",
		"BaudRate": "115200"
	}
}
```

### Response

Initialized:
```json
{
	"ResponseType": 2
}
```

or

Already Initialized:
```json
{
	"ResponseType": 3
}
```

## Process Payment

**RequestType:** "4" or "process"

### Request

```json
{
	"RequestType": 4,
	"ContextId": "42",
	"Process": {
		"Amount": 100,
		"MagstripePaymentMethod": "Debit"
	}
}
```

### Response

```json
{
	"ResponseType":4,
	"Process": {
		"CardHash": "sdljfh38o4o(¨&(*$@YR*(&YU",
		"Status": 0,
		"PaymentMethod": 1,
		"CardHolderName": "Douglas",
		"IsOnlinePin": false
	}
}
```

## Finish Payment

**RequestType:** "5" or "finish"

### Request

```json
{
	"RequestType": 5,
	"ContextId": "42",
	"Finish": {
		"Success": true,
		"ResponseCode": "0000",
		"EmvData": "000000000.0000"
	}
}
```

### Response

```json
{
	"ResponseType": 5
}
```

## Display Message

**RequestType:** "6" or "displayMessage"

### Request

```json
{
	"RequestType": 6,
	"ContextId": "42",
	"DisplayMessage": {
		"Message": "PagarMe"
	}
}
```

### Response

```json
{
	"ResponseType": 6
}
```

## Status

**RequestType:** "7" or "status"

### Request

```json
{
	"RequestType": 7,
	"ContextId": "42"
}
```

### Response

```json
{
	"ResponseType": 7,
	"Status": {
		"Status": 1,
		"ConnectedDeviceId": "01234567-89AB-CDEF-FEDC-BA9876543210",
		"AvailableDevices": 2
	}
}
```

## Close

**RequestType:** "8" or "close"

### Request

```json
{
	"RequestType": 8,
	"ContextId": "42"
}
```

### Response

```json
{
	"ResponseType": 8
}
```



# Client Libraries

You can find reference implementation of a client library for usage in the browser [here](example-site).

---

(mpos-bridge-js): **https:**//github.com/pagarme/mpos-bridge-js

