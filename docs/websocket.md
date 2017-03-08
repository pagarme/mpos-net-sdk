# WebSocket

A websocket endpoint is provided on `/ws`.

# Protocol

All messages are encoded in JSON. All request messages have the following parameter:**

**context:** The [context](architecture.md#context).

**command:** The command to be executed.

**command_id:** Request ID, optional. Used to track the response.

All responses contains the following parameters:**

**command:** The command executed.

**context:** The context where the command was executed

**command_id:** Echoes the value provided in the request.

**success:** Wheter the command was executed successfuly.

**response:** The actual response data, if the command has any.

**error:** Error message if any ocurred.

In this documentation only request/response examples will be provided. For information on actual parameters, check [this](command.md).

## List Devices

**Command:** list_devices

### Request

```json
{
    "command": "list_devices"
}
```

### Response

```json
{
    "command": "list_devices",
    "success": true,
    "response": [{
        "id": 1,
        "name": "MOBI PIN 10",
        "manufacturer": "Gertec"
    }]
}
```

## Initialize

**Command:** initialize

### Request

```json
{
    "command": "initialize",
    "encryption_key": "ek_test_msSNokas032mAsdkmnklsspa4AdjzmcALl23",
    "device_id": 1
}
```

### Response

```json
{
    "command": "initialize",
    "success": true
}
```

## Status

**Command:** status

### Request

```json
{
    "command": "status"
}
```

### Response

```json
{
    "command": "status",
    "success": true,
    "response": {
        "status": "initialized",
        "connected_device_id": 1,
        "devices_available": 1
    }
}
```

## Display Message

**Command:** display_message

### Request

```json
{
    "command": "display_message",
    "message": "Hello World!"
}
```

### Response

```json
{
    "command": "display_message",
    "success": true
}
```

## Process Payment

**Command:** process_payment

### Request

```json
{
    "command": "process_payment",
    "amount": 1000
}
```

### Response

```json
{
    "command": "process_payment",
    "success": true,
    "response": {
        "card_hash": "3248_sndifsijdsfsdfdha9pfdsjfsdhgsdioufpojh480r9p8sodhfgof9p283oihblsdOFa8ygoasidufhgasdfhkb",
        "card_holder_name": "JONATHAN MARQUES",
        "payment_method": "debit_card"
    }
}
```

# Client Libraries

You can find reference implementation of a client library for usage in the browser [here](mpos-bridge-js).

---

(mpos-bridge-js):** https:**//github.com/pagarme/mpos-bridge-js

