# WebSocket

A websocket endpoint is provided on `/mpos`.

# Protocol

All messages are encoded in JSON. All request messages have the following parameter:

- **context_id:** The id of [context](architecture.md#context).

- **request_type:** Request type. A request can be a step of processing the transaction, initialize or close mpos connection, display message on mpos, or a request for current mpos status.

All responses contains the following parameters:

- **context_id:** The id of [context](architecture.md#context) which this response belongs.

- **request_type:** Type of the response, which can be success response, error on process or unknown command, in case of request type unknown.

- **error:** The error message if any ocurred. `request_type` will be 9.

In this documentation only request/response examples will be provided. For information on actual parameters, check [this](command.md).

## List Devices

**request_type:** `1` or `listDevices`

### Request

```json
{
	"request_type": 1,
	"context_id": "42"
}
```

### Response

```json
{
	"request_type": 1,
	"context_id": "42",
	"device_list": [{
		"port": "COM3",
		"id": "01234567-89AB-CDEF-FEDC-BA9876543210",
		"name": "Serial Device (COM3)",
	}]
}
```

## Initialize

**request_type:** `2` or `initialize`

### Request

```json
{
	"request_type": 2,
	"context_id": "42",
	"initialize": {
		"device_id": "01234567-89AB-CDEF-FEDC-BA9876543210",
		"encryption_key": "ek_test_f9cws0bU9700VqWE4UDuBlKLbvX4IO",
		"baud_rate": "115200"
	}
}
```

### Response

Initialized:
```json
{
	"request_type": 2,
	"context_id": "42"
}
```

or

Already Initialized:
```json
{
	"request_type": 3,
	"context_id": "42"
}
```

## Process Payment

**request_type:** `4` or `process`

### Request

```json
{
	"request_type": 4,
	"context_id": "42",
	"process": {
		"amount": 100
	}
}
```

### Response

```json
{
	"request_type": 4,
	"context_id": "42",
	"process": {
		"card_hash": "sdljfh38o4o(¨&(*$@YR*(&YU",
		"status": 0,
		"payment_method": 1,
		"card_holder_name": "Douglas",
		"is_online_pin": false
	}
}
```

## Finish Payment

**request_type:** `5` or `finish`

### Request

```json
{
	"request_type": 5,
	"context_id": "42",
	"finish": {
		"success": true,
		"response_code": "0000",
		"emv_data": "000000000.0000"
	}
}
```

### Response

```json
{
	"request_type": 5,
	"context_id": "42"
}
```

## Display Message

**request_type:** `6` or `displayMessage`

### Request

```json
{
	"request_type": 6,
	"context_id": "42",
	"display_message": {
		"message": "PagarMe"
	}
}
```

### Response

```json
{
	"request_type": 6,
	"context_id": "42"
}
```

## Status

**request_type:** `7` or `status`

### Request

```json
{
	"request_type": 7,
	"context_id": "42"
}
```

### Response

```json
{
	"request_type": 7,
	"context_id": "42",
	"status": {
		"status": 1,
		"connected_device_id": "01234567-89AB-CDEF-FEDC-BA9876543210",
		"available_devices": 2
	}
}
```

## Close

**request_type:** `8` or `close`

### Request

```json
{
	"request_type": 8,
	"context_id": "42"
}
```

### Response

```json
{
	"request_type": 8,
	"context_id": "42"
}
```



# Client Libraries

You can find reference implementation of a client library for usage in the browser [here](example-site).

---

(mpos-bridge-js): **https:**//github.com/pagarme/mpos-bridge-js

