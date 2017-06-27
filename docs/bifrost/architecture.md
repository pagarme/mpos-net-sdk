# Architecture

## Device

Represents a single device connected to the bridge host computer.

## Context

Multiple contexts can exist inside the same bridge instance. Each context can perform one action a time. One device can only be used by one context.

Contexts usage is optional and a default one will be used if none is provided in the request.

