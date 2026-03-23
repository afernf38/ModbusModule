# Modbus Module

Windows Forms Modbus TCP tester built on .NET 8 and NModbus4.

## What this project does

This app provides a simple desktop UI to:

- Connect and disconnect from one or more Modbus TCP devices
- Read Modbus registers on demand
- Write coils and holding registers on demand
- Run manual polling cycles from the UI
- Receive internal polling events through a reusable connection class

The repository also includes a reusable `clsConexionModbusTCP` implementation you can use outside the form.

## Tech stack

- .NET 8 (`net8.0-windows`)
- Windows Forms
- [NModbus4](https://www.nuget.org/packages/NModbus4)
- Newtonsoft.Json

## Project layout

- `Program.cs` - application entry point
- `frmModbusTester.cs` - UI behavior (connect/read/write/polling actions)
- `frmModbusTester.Designer.cs` - UI controls and layout
- `clsConexion.cs` - abstract base connection contract (`UNE135411` namespace)
- `clsConexionModbusTCP.cs` - Modbus TCP implementation

## Requirements

- Windows OS (WinForms app)
- .NET 8 SDK
- Network access to target Modbus TCP device(s)
- Default Modbus TCP port is usually `502`

## Build and run

From repository root:

```bash
dotnet restore
dotnet build "Modbus Module.csproj"
dotnet run --project "Modbus Module.csproj"
```

You can also open `Modbus Module.sln` in Visual Studio and run it directly.

## UI quick start

1. Open the `Conexion` tab and set `IP`, `Puerto`, `Unit ID`, and `Timeout`.
2. Click `Conectar`.
3. Use:
   - `Lectura` tab for on-demand reads
   - `Escritura` tab for on-demand writes
   - `Polling` tab for repeated reads with a fixed interval
4. Watch per-tab logs for responses and errors.

## Supported Modbus operations

Read:

- `Coil`
- `DiscreteInput`
- `HoldingRegister`
- `InputRegister`

Write:

- `Coil`
- `HoldingRegister`

Trying to write `DiscreteInput` or `InputRegister` returns a validation error.

## Write value formats

For `Coil`:

- `1,0,1`
- `true,false,true`

For `HoldingRegister`:

- `100,200,300`

## Connection JSON format (for `clsConexionModbusTCP`)

The connection class expects a JSON string in `Conectar(...)`.

### Single device example

```json
{
  "ip": "127.0.0.1",
  "puerto": 502,
  "unitId": 1,
  "timeoutMs": 2000,
  "modo": "BajoDemanda",
  "intervaloPollingMs": 1000
}
```

### Multiple devices example

```json
{
  "modo": "Ambos",
  "intervaloPollingMs": 1000,
  "dispositivos": [
    {
      "clave": "PLC_A",
      "ip": "192.168.1.10",
      "puerto": 502,
      "unitId": 1,
      "timeoutMs": 2000
    },
    {
      "clave": "PLC_B",
      "ip": "192.168.1.11",
      "puerto": 502,
      "unitId": 2,
      "timeoutMs": 2000
    }
  ]
}
```

## Synchronization modes

- `BajoDemanda` - on-demand reads and writes only
- `PollingPeriodico` - internal periodic polling only
- `Ambos` - both polling and on-demand operations

When mode is `PollingPeriodico`, direct `Leer(...)` and `Escribir(...)` are blocked by design.

## Error handling

Operations return a dictionary of error codes/messages (for example `ERR_MODBUS_001`, `ERR_MODBUS_004`, etc.) so callers can log or map failures.

## Notes

- This is a practical test tool; it does not include authentication, encryption, or production hardening.
- Use isolated networks and safe devices when testing write operations.
