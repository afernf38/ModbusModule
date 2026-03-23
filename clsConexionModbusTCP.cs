using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using UNE135411;
using Newtonsoft.Json.Linq;
using Modbus.Device;
using ThreadingTimer = System.Threading.Timer;

namespace ModbusTester
{
    /// <summary>
    /// Implementacion de conexion Modbus TCP con soporte de lectura, escritura y polling periodico.
    /// </summary>
    public class clsConexionModbusTCP : clsConexion
    {
        #region Constantes de error

        /// <summary>
        /// Error cuando no existe conexion activa.
        /// </summary>
        private const string ERR_MODBUS_001 = "ERR_MODBUS_001";

        /// <summary>
        /// Error cuando falla la conexion con el dispositivo.
        /// </summary>
        private const string ERR_MODBUS_002 = "ERR_MODBUS_002";

        /// <summary>
        /// Error cuando el formato de la cadena de conexion es invalido.
        /// </summary>
        private const string ERR_MODBUS_003 = "ERR_MODBUS_003";

        /// <summary>
        /// Error cuando falla una lectura Modbus.
        /// </summary>
        private const string ERR_MODBUS_004 = "ERR_MODBUS_004";

        /// <summary>
        /// Error cuando falla una escritura Modbus.
        /// </summary>
        private const string ERR_MODBUS_005 = "ERR_MODBUS_005";

        /// <summary>
        /// Error cuando el tipo de registro no es valido.
        /// </summary>
        private const string ERR_MODBUS_006 = "ERR_MODBUS_006";

        /// <summary>
        /// Error cuando el objeto comodin no es valido.
        /// </summary>
        private const string ERR_MODBUS_007 = "ERR_MODBUS_007";

        /// <summary>
        /// Error cuando la direccion solicitada no es valida.
        /// </summary>
        private const string ERR_MODBUS_008 = "ERR_MODBUS_008";

        /// <summary>
        /// Error cuando la cantidad solicitada no es valida.
        /// </summary>
        private const string ERR_MODBUS_009 = "ERR_MODBUS_009";

        /// <summary>
        /// Error cuando no se encuentran dispositivos conectados.
        /// </summary>
        private const string ERR_MODBUS_010 = "ERR_MODBUS_010";

        /// <summary>
        /// Error cuando no se puede resolver el dispositivo destino.
        /// </summary>
        private const string ERR_MODBUS_011 = "ERR_MODBUS_011";

        /// <summary>
        /// Error cuando el tipo de registro no permite escritura.
        /// </summary>
        private const string ERR_MODBUS_012 = "ERR_MODBUS_012";

        /// <summary>
        /// Error cuando la operacion no esta disponible para el modo actual.
        /// </summary>
        private const string ERR_MODBUS_013 = "ERR_MODBUS_013";

        /// <summary>
        /// Error cuando los valores de escritura son invalidos.
        /// </summary>
        private const string ERR_MODBUS_014 = "ERR_MODBUS_014";

        #endregion

        #region Campos privados

        /// <summary>
        /// Objeto de sincronizacion para operaciones Modbus concurrentes.
        /// </summary>
        private readonly object _lockModbus = new object();

        /// <summary>
        /// Temporizador interno para polling periodico.
        /// </summary>
        private ThreadingTimer? _timerPolling;

        /// <summary>
        /// Modo de sincronizacion actual.
        /// </summary>
        private ModoSincronizacion _modoActual = ModoSincronizacion.BajoDemanda;

        /// <summary>
        /// Intervalo de polling en milisegundos.
        /// </summary>
        private int _intervaloPollingMs = 1000;

        /// <summary>
        /// Mapa de dispositivos Modbus conectados.
        /// </summary>
        private readonly Dictionary<string, ContextoDispositivo> _dispositivos = new Dictionary<string, ContextoDispositivo>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Diccionario inmutable de errores soportados.
        /// </summary>
        private readonly Dictionary<string, string> _catalogoErrores;

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="clsConexionModbusTCP"/>.
        /// </summary>
        public clsConexionModbusTCP()
        {
            _catalogoErrores = new Dictionary<string, string>
            {
                { ERR_MODBUS_001, "No existe una conexion Modbus activa." },
                { ERR_MODBUS_002, "No se pudo establecer conexion con uno o mas dispositivos Modbus." },
                { ERR_MODBUS_003, "La cadena de conexion no tiene un formato JSON valido." },
                { ERR_MODBUS_004, "Se produjo un error durante la lectura Modbus." },
                { ERR_MODBUS_005, "Se produjo un error durante la escritura Modbus." },
                { ERR_MODBUS_006, "El tipo de registro indicado no es valido." },
                { ERR_MODBUS_007, "El objeto comodin recibido no es del tipo esperado." },
                { ERR_MODBUS_008, "La direccion indicada no es valida." },
                { ERR_MODBUS_009, "La cantidad solicitada debe ser mayor a cero." },
                { ERR_MODBUS_010, "No hay dispositivos Modbus disponibles para operar." },
                { ERR_MODBUS_011, "No se pudo resolver el dispositivo destino con los datos proporcionados." },
                { ERR_MODBUS_012, "El tipo de registro seleccionado no admite escritura." },
                { ERR_MODBUS_013, "La operacion no esta permitida por el modo de sincronizacion actual." },
                { ERR_MODBUS_014, "Los valores de escritura no tienen un formato valido." }
            };
        }

        #endregion

        #region Tipos publicos

        /// <summary>
        /// Tipos de registro Modbus soportados por la conexion.
        /// </summary>
        public enum TipoRegistro
        {
            /// <summary>
            /// Registro de tipo Coil.
            /// </summary>
            Coil,

            /// <summary>
            /// Registro de tipo Discrete Input.
            /// </summary>
            DiscreteInput,

            /// <summary>
            /// Registro de tipo Holding Register.
            /// </summary>
            HoldingRegister,

            /// <summary>
            /// Registro de tipo Input Register.
            /// </summary>
            InputRegister
        }

        /// <summary>
        /// Modos de sincronizacion disponibles para la conexion Modbus.
        /// </summary>
        public enum ModoSincronizacion
        {
            /// <summary>
            /// Operaciones solo bajo demanda.
            /// </summary>
            BajoDemanda,

            /// <summary>
            /// Lecturas periodicas con temporizador interno.
            /// </summary>
            PollingPeriodico,

            /// <summary>
            /// Modo mixto con polling y operaciones bajo demanda.
            /// </summary>
            Ambos
        }

        /// <summary>
        /// Elemento Modbus con metadatos de direccionamiento y tipo de registro.
        /// </summary>
        public class clsElementoModbus : clsElemento
        {
            /// <summary>
            /// Tipo de registro Modbus del elemento.
            /// </summary>
            public TipoRegistro TipoRegistro { get; set; }

            /// <summary>
            /// Direccion inicial del registro.
            /// </summary>
            public ushort Direccion { get; set; }

            /// <summary>
            /// Cantidad de valores a leer.
            /// </summary>
            public ushort Cantidad { get; set; } = 1;

            /// <summary>
            /// Unit Id opcional para este elemento.
            /// </summary>
            public byte? UnitId { get; set; }
        }

        /// <summary>
        /// Objeto comodin para solicitudes de lectura y escritura bajo demanda.
        /// </summary>
        public class ModbusComodin
        {
            /// <summary>
            /// Tipo de registro solicitado.
            /// </summary>
            public TipoRegistro TipoRegistro { get; set; }

            /// <summary>
            /// Direccion inicial del registro.
            /// </summary>
            public ushort Direccion { get; set; }

            /// <summary>
            /// Cantidad de datos a leer o escribir.
            /// </summary>
            public ushort Cantidad { get; set; } = 1;

            /// <summary>
            /// Unit Id opcional para la operacion.
            /// </summary>
            public byte? UnitId { get; set; }

            /// <summary>
            /// Valores a escribir. Puede ser arreglo, enumerable o texto separado por comas.
            /// </summary>
            public object? Valores { get; set; }
        }

        #endregion

        #region Implementacion clsConexion

        /// <summary>
        /// Establece conexion con uno o varios dispositivos Modbus TCP a partir de una cadena JSON.
        /// </summary>
        /// <param name="_cadenaConexion">Cadena de conexion en formato JSON.</param>
        /// <param name="_listaElementosGeneral">Lista de elementos a usar en polling.</param>
        /// <param name="_listaErrores">Diccionario de errores producidos.</param>
        /// <returns>Verdadero si la conexion se establece correctamente.</returns>
        public override bool Conectar(string _cadenaConexion, List<clsElemento> _listaElementosGeneral, out Dictionary<string, string> _listaErrores)
        {
            _listaErrores = new Dictionary<string, string>();

            try
            {
                lock (_lockModbus)
                {
                    if (conectado)
                    {
                        DesconectarInterno();
                    }

                    listaElementosGeneral = _listaElementosGeneral ?? new List<clsElemento>();

                    if (!TryParseConfiguracion(_cadenaConexion, out List<ConfiguracionDispositivo> configuraciones, out ModoSincronizacion modo, out int intervalo, out string errorParseo))
                    {
                        AgregarError(_listaErrores, ERR_MODBUS_003, errorParseo);
                        return false;
                    }

                    if (configuraciones.Count == 0)
                    {
                        AgregarError(_listaErrores, ERR_MODBUS_003, "No se encontro ninguna configuracion de dispositivo en la cadena de conexion.");
                        return false;
                    }

                    foreach (ConfiguracionDispositivo configuracion in configuraciones)
                    {
                        TcpClient tcpClient = new TcpClient();
                        tcpClient.ReceiveTimeout = configuracion.TimeoutMs;
                        tcpClient.SendTimeout = configuracion.TimeoutMs;
                        tcpClient.Connect(configuracion.Ip, configuracion.Puerto);

                        ModbusIpMaster master = ModbusIpMaster.CreateIp(tcpClient);
                        string clave = ObtenerClaveDispositivo(configuracion);

                        _dispositivos[clave] = new ContextoDispositivo
                        {
                            Clave = clave,
                            Ip = configuracion.Ip,
                            Puerto = configuracion.Puerto,
                            UnitIdDefecto = configuracion.UnitId,
                            TimeoutMs = configuracion.TimeoutMs,
                            TcpClient = tcpClient,
                            Master = master
                        };
                    }

                    _modoActual = modo;
                    _intervaloPollingMs = intervalo;
                    conectado = true;

                    if (_modoActual == ModoSincronizacion.PollingPeriodico || _modoActual == ModoSincronizacion.Ambos)
                    {
                        IniciarTimerPolling();
                    }

                    OnCambioEstadoConexion(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                lock (_lockModbus)
                {
                    DesconectarInterno();
                }

                AgregarError(_listaErrores, ERR_MODBUS_002, $"Detalle: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cierra todas las conexiones Modbus activas y detiene el polling interno.
        /// </summary>
        /// <param name="_listaErrores">Diccionario de errores producidos.</param>
        /// <returns>Verdadero si la desconexion se completa correctamente.</returns>
        public override bool Desconectar(out Dictionary<string, string> _listaErrores)
        {
            _listaErrores = new Dictionary<string, string>();

            try
            {
                lock (_lockModbus)
                {
                    DesconectarInterno();
                }

                return true;
            }
            catch (Exception ex)
            {
                AgregarError(_listaErrores, ERR_MODBUS_002, $"Detalle al desconectar: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ejecuta una escritura Modbus bajo demanda.
        /// </summary>
        /// <param name="_comodin">Objeto de tipo <see cref="ModbusComodin"/>.</param>
        /// <param name="_listaErrores">Diccionario de errores producidos.</param>
        /// <returns>Verdadero si la escritura finaliza correctamente.</returns>
        public override bool Escribir(object _comodin, out Dictionary<string, string> _listaErrores)
        {
            _listaErrores = new Dictionary<string, string>();

            if (!conectado)
            {
                AgregarError(_listaErrores, ERR_MODBUS_001, _catalogoErrores[ERR_MODBUS_001]);
                return false;
            }

            if (_modoActual == ModoSincronizacion.PollingPeriodico)
            {
                AgregarError(_listaErrores, ERR_MODBUS_013, "El modo PollingPeriodico no permite escrituras bajo demanda.");
                return false;
            }

            if (_comodin is not ModbusComodin comodin)
            {
                AgregarError(_listaErrores, ERR_MODBUS_007, _catalogoErrores[ERR_MODBUS_007]);
                return false;
            }

            if (comodin.Cantidad == 0)
            {
                AgregarError(_listaErrores, ERR_MODBUS_009, _catalogoErrores[ERR_MODBUS_009]);
                return false;
            }

            try
            {
                lock (_lockModbus)
                {
                    if (!TryResolverDispositivo(comodin.UnitId, out ContextoDispositivo contexto))
                    {
                        AgregarError(_listaErrores, ERR_MODBUS_011, _catalogoErrores[ERR_MODBUS_011]);
                        return false;
                    }

                    byte unitId = comodin.UnitId ?? contexto.UnitIdDefecto;

                    switch (comodin.TipoRegistro)
                    {
                        case TipoRegistro.Coil:
                            if (!TryConvertirBool(comodin.Valores, comodin.Cantidad, out bool[] coils))
                            {
                                AgregarError(_listaErrores, ERR_MODBUS_014, "Para Coil debe indicar valores booleanos o 0/1.");
                                return false;
                            }

                            if (coils.Length == 1)
                            {
                                contexto.Master.WriteSingleCoil(unitId, comodin.Direccion, coils[0]);
                            }
                            else
                            {
                                contexto.Master.WriteMultipleCoils(unitId, comodin.Direccion, coils);
                            }

                            return true;

                        case TipoRegistro.HoldingRegister:
                            if (!TryConvertirUShort(comodin.Valores, comodin.Cantidad, out ushort[] registros))
                            {
                                AgregarError(_listaErrores, ERR_MODBUS_014, "Para HoldingRegister debe indicar valores numericos entre 0 y 65535.");
                                return false;
                            }

                            if (registros.Length == 1)
                            {
                                contexto.Master.WriteSingleRegister(unitId, comodin.Direccion, registros[0]);
                            }
                            else
                            {
                                contexto.Master.WriteMultipleRegisters(unitId, comodin.Direccion, registros);
                            }

                            return true;

                        case TipoRegistro.DiscreteInput:
                        case TipoRegistro.InputRegister:
                            AgregarError(_listaErrores, ERR_MODBUS_012, _catalogoErrores[ERR_MODBUS_012]);
                            return false;

                        default:
                            AgregarError(_listaErrores, ERR_MODBUS_006, _catalogoErrores[ERR_MODBUS_006]);
                            return false;
                    }
                }
            }
            catch (Exception ex)
            {
                AgregarError(_listaErrores, ERR_MODBUS_005, $"Detalle: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ejecuta una lectura Modbus bajo demanda.
        /// </summary>
        /// <param name="_comodin">Objeto de tipo <see cref="ModbusComodin"/>.</param>
        /// <param name="respuesta">Resultado de la lectura.</param>
        /// <param name="_listaErrores">Diccionario de errores producidos.</param>
        /// <returns>Verdadero si la lectura finaliza correctamente.</returns>
        public override bool Leer(object _comodin, out object respuesta, out Dictionary<string, string> _listaErrores)
        {
            respuesta = Array.Empty<object>();
            _listaErrores = new Dictionary<string, string>();

            if (!conectado)
            {
                AgregarError(_listaErrores, ERR_MODBUS_001, _catalogoErrores[ERR_MODBUS_001]);
                return false;
            }

            if (_modoActual == ModoSincronizacion.PollingPeriodico)
            {
                AgregarError(_listaErrores, ERR_MODBUS_013, "El modo PollingPeriodico no permite lecturas bajo demanda.");
                return false;
            }

            if (_comodin is not ModbusComodin comodin)
            {
                AgregarError(_listaErrores, ERR_MODBUS_007, _catalogoErrores[ERR_MODBUS_007]);
                return false;
            }

            if (comodin.Cantidad == 0)
            {
                AgregarError(_listaErrores, ERR_MODBUS_009, _catalogoErrores[ERR_MODBUS_009]);
                return false;
            }

            try
            {
                lock (_lockModbus)
                {
                    if (!TryResolverDispositivo(comodin.UnitId, out ContextoDispositivo contexto))
                    {
                        AgregarError(_listaErrores, ERR_MODBUS_011, _catalogoErrores[ERR_MODBUS_011]);
                        return false;
                    }

                    byte unitId = comodin.UnitId ?? contexto.UnitIdDefecto;
                    object resultado = LeerInterno(contexto, unitId, comodin.TipoRegistro, comodin.Direccion, comodin.Cantidad);
                    respuesta = resultado;
                    return true;
                }
            }
            catch (Exception ex)
            {
                AgregarError(_listaErrores, ERR_MODBUS_004, $"Detalle: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Devuelve el catalogo completo de errores de la conexion Modbus.
        /// </summary>
        /// <returns>Diccionario de codigo y descripcion de errores.</returns>
        public override Dictionary<string, string> gestionErrores()
        {
            return new Dictionary<string, string>(_catalogoErrores);
        }

        #endregion

        #region API publica adicional

        /// <summary>
        /// Cambia el modo de sincronizacion en tiempo de ejecucion.
        /// </summary>
        /// <param name="modo">Nuevo modo de sincronizacion.</param>
        public void CambiarModo(ModoSincronizacion modo)
        {
            lock (_lockModbus)
            {
                _modoActual = modo;

                if (!conectado)
                {
                    return;
                }

                if (_modoActual == ModoSincronizacion.PollingPeriodico || _modoActual == ModoSincronizacion.Ambos)
                {
                    IniciarTimerPolling();
                }
                else
                {
                    DetenerTimerPolling();
                }
            }
        }

        #endregion

        #region Logica privada

        /// <summary>
        /// Ejecuta el ciclo de polling interno leyendo todos los elementos configurados.
        /// </summary>
        /// <param name="state">Estado del temporizador.</param>
        private void EjecutarPollingInterno(object? state)
        {
            if (!conectado)
            {
                return;
            }

            List<clsElementoModbus> elementos = listaElementosGeneral.OfType<clsElementoModbus>().ToList();
            if (elementos.Count == 0)
            {
                return;
            }

            foreach (clsElementoModbus elemento in elementos)
            {
                try
                {
                    lock (_lockModbus)
                    {
                        if (!TryResolverDispositivo(elemento.UnitId, out ContextoDispositivo contexto))
                        {
                            continue;
                        }

                        byte unitId = elemento.UnitId ?? contexto.UnitIdDefecto;
                        object resultado = LeerInterno(contexto, unitId, elemento.TipoRegistro, elemento.Direccion, elemento.Cantidad);
                        JObject mensaje = CrearMensajeLectura(elemento.CodElemento, elemento.TipoRegistro, elemento.Direccion, elemento.Cantidad, unitId, resultado);
                        OnMensajeEntrante(mensaje, elemento.CodElemento);
                    }
                }
                catch
                {
                    // El polling continua con el siguiente elemento para no detener el ciclo completo.
                }
            }
        }

        /// <summary>
        /// Crea un objeto JSON con los datos de una lectura.
        /// </summary>
        /// <param name="codElemento">Codigo del elemento leido.</param>
        /// <param name="tipoRegistro">Tipo de registro leido.</param>
        /// <param name="direccion">Direccion de inicio.</param>
        /// <param name="cantidad">Cantidad solicitada.</param>
        /// <param name="unitId">Unit Id usado.</param>
        /// <param name="resultado">Resultado de lectura.</param>
        /// <returns>Objeto JSON con el resultado de lectura.</returns>
        private static JObject CrearMensajeLectura(string codElemento, TipoRegistro tipoRegistro, ushort direccion, ushort cantidad, byte unitId, object resultado)
        {
            return new JObject
            {
                ["timestamp"] = DateTime.UtcNow,
                ["codElemento"] = codElemento,
                ["tipoRegistro"] = tipoRegistro.ToString(),
                ["direccion"] = direccion,
                ["cantidad"] = cantidad,
                ["unitId"] = unitId,
                ["resultado"] = JToken.FromObject(resultado)
            };
        }

        /// <summary>
        /// Lee datos desde un dispositivo Modbus de acuerdo al tipo de registro.
        /// </summary>
        /// <param name="contexto">Contexto del dispositivo.</param>
        /// <param name="unitId">Unit Id de destino.</param>
        /// <param name="tipoRegistro">Tipo de registro.</param>
        /// <param name="direccion">Direccion inicial.</param>
        /// <param name="cantidad">Cantidad a leer.</param>
        /// <returns>Resultado tipado de la lectura.</returns>
        private static object LeerInterno(ContextoDispositivo contexto, byte unitId, TipoRegistro tipoRegistro, ushort direccion, ushort cantidad)
        {
            return tipoRegistro switch
            {
                TipoRegistro.Coil => contexto.Master.ReadCoils(unitId, direccion, cantidad),
                TipoRegistro.DiscreteInput => contexto.Master.ReadInputs(unitId, direccion, cantidad),
                TipoRegistro.HoldingRegister => contexto.Master.ReadHoldingRegisters(unitId, direccion, cantidad),
                TipoRegistro.InputRegister => contexto.Master.ReadInputRegisters(unitId, direccion, cantidad),
                _ => throw new InvalidOperationException("Tipo de registro no soportado.")
            };
        }

        /// <summary>
        /// Inicia o reinicia el temporizador interno de polling.
        /// </summary>
        private void IniciarTimerPolling()
        {
            DetenerTimerPolling();
            _timerPolling = new ThreadingTimer(EjecutarPollingInterno, null, _intervaloPollingMs, _intervaloPollingMs);
        }

        /// <summary>
        /// Detiene y libera el temporizador interno de polling.
        /// </summary>
        private void DetenerTimerPolling()
        {
            _timerPolling?.Change(Timeout.Infinite, Timeout.Infinite);
            _timerPolling?.Dispose();
            _timerPolling = null;
        }

        /// <summary>
        /// Cierra recursos de conexion sin generar excepciones al consumidor.
        /// </summary>
        private void DesconectarInterno()
        {
            DetenerTimerPolling();

            foreach (ContextoDispositivo dispositivo in _dispositivos.Values)
            {
                try
                {
                    dispositivo.TcpClient.Close();
                    dispositivo.TcpClient.Dispose();
                }
                catch
                {
                    // Se ignoran excepciones durante liberacion de recursos.
                }
            }

            _dispositivos.Clear();

            if (conectado)
            {
                conectado = false;
                OnCambioEstadoConexion(false);
            }
        }

        /// <summary>
        /// Intenta resolver un dispositivo por clave o por Unit Id.
        /// </summary>
        /// <param name="unitId">Unit Id opcional.</param>
        /// <param name="contexto">Contexto del dispositivo resuelto.</param>
        /// <returns>Verdadero cuando se resuelve un dispositivo valido.</returns>
        private bool TryResolverDispositivo(byte? unitId, out ContextoDispositivo contexto)
        {
            contexto = null!;

            if (_dispositivos.Count == 0)
            {
                return false;
            }

            if (unitId.HasValue)
            {
                ContextoDispositivo? porUnidad = _dispositivos.Values.FirstOrDefault(d => d.UnitIdDefecto == unitId.Value);
                if (porUnidad != null)
                {
                    contexto = porUnidad;
                    return true;
                }
            }

            if (_dispositivos.Count == 1)
            {
                contexto = _dispositivos.Values.First();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Agrega un error al diccionario destino.
        /// </summary>
        /// <param name="errores">Diccionario de errores.</param>
        /// <param name="codigo">Codigo de error.</param>
        /// <param name="detalle">Detalle adicional.</param>
        private static void AgregarError(Dictionary<string, string> errores, string codigo, string detalle)
        {
            if (!errores.ContainsKey(codigo))
            {
                errores[codigo] = detalle;
            }
        }

        /// <summary>
        /// Obtiene la clave unica de un dispositivo a partir de su configuracion.
        /// </summary>
        /// <param name="configuracion">Configuracion del dispositivo.</param>
        /// <returns>Clave unica para diccionario de dispositivos.</returns>
        private static string ObtenerClaveDispositivo(ConfiguracionDispositivo configuracion)
        {
            if (!string.IsNullOrWhiteSpace(configuracion.Clave))
            {
                return configuracion.Clave.Trim();
            }

            return $"{configuracion.Ip}:{configuracion.Puerto}:U{configuracion.UnitId}";
        }

        /// <summary>
        /// Convierte el objeto de valores a un arreglo booleano.
        /// </summary>
        /// <param name="valores">Valores de entrada.</param>
        /// <param name="cantidad">Cantidad esperada.</param>
        /// <param name="resultado">Resultado convertido.</param>
        /// <returns>Verdadero si la conversion es correcta.</returns>
        private static bool TryConvertirBool(object? valores, ushort cantidad, out bool[] resultado)
        {
            resultado = Array.Empty<bool>();

            if (valores is bool[] arregloBool)
            {
                resultado = arregloBool;
                return arregloBool.Length > 0;
            }

            if (valores is IEnumerable<bool> enumerableBool)
            {
                bool[] items = enumerableBool.ToArray();
                if (items.Length > 0)
                {
                    resultado = items;
                    return true;
                }
            }

            if (valores is string texto)
            {
                string[] trozos = texto.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                List<bool> lista = new List<bool>();

                foreach (string trozo in trozos)
                {
                    string normalizado = trozo.Trim();
                    if (normalizado.Equals("1", StringComparison.OrdinalIgnoreCase) || normalizado.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        lista.Add(true);
                    }
                    else if (normalizado.Equals("0", StringComparison.OrdinalIgnoreCase) || normalizado.Equals("false", StringComparison.OrdinalIgnoreCase))
                    {
                        lista.Add(false);
                    }
                    else
                    {
                        return false;
                    }
                }

                if (lista.Count > 0)
                {
                    resultado = lista.ToArray();
                    return true;
                }
            }

            if (valores is bool unico)
            {
                resultado = cantidad > 1 ? Enumerable.Repeat(unico, cantidad).ToArray() : new[] { unico };
                return true;
            }

            if (valores is int entero)
            {
                if (entero == 0 || entero == 1)
                {
                    bool valor = entero == 1;
                    resultado = cantidad > 1 ? Enumerable.Repeat(valor, cantidad).ToArray() : new[] { valor };
                    return true;
                }

                return false;
            }

            if (resultado.Length == 1 && cantidad > 1)
            {
                resultado = Enumerable.Repeat(resultado[0], cantidad).ToArray();
            }

            return resultado.Length > 0;
        }

        /// <summary>
        /// Convierte el objeto de valores a un arreglo de enteros sin signo de 16 bits.
        /// </summary>
        /// <param name="valores">Valores de entrada.</param>
        /// <param name="cantidad">Cantidad esperada.</param>
        /// <param name="resultado">Resultado convertido.</param>
        /// <returns>Verdadero si la conversion es correcta.</returns>
        private static bool TryConvertirUShort(object? valores, ushort cantidad, out ushort[] resultado)
        {
            resultado = Array.Empty<ushort>();

            if (valores is ushort[] arregloUshort)
            {
                resultado = arregloUshort;
                return arregloUshort.Length > 0;
            }

            if (valores is IEnumerable<ushort> enumerableUshort)
            {
                ushort[] items = enumerableUshort.ToArray();
                if (items.Length > 0)
                {
                    resultado = items;
                    return true;
                }
            }

            if (valores is string texto)
            {
                string[] trozos = texto.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                List<ushort> lista = new List<ushort>();

                foreach (string trozo in trozos)
                {
                    if (!ushort.TryParse(trozo.Trim(), out ushort valor))
                    {
                        return false;
                    }

                    lista.Add(valor);
                }

                if (lista.Count > 0)
                {
                    resultado = lista.ToArray();
                    return true;
                }
            }

            if (valores is ushort unico)
            {
                resultado = cantidad > 1 ? Enumerable.Repeat(unico, cantidad).ToArray() : new[] { unico };
                return true;
            }

            if (valores is int entero)
            {
                if (entero < 0 || entero > ushort.MaxValue)
                {
                    return false;
                }

                ushort valor = (ushort)entero;
                resultado = cantidad > 1 ? Enumerable.Repeat(valor, cantidad).ToArray() : new[] { valor };
                return true;
            }

            if (resultado.Length == 1 && cantidad > 1)
            {
                resultado = Enumerable.Repeat(resultado[0], cantidad).ToArray();
            }

            return resultado.Length > 0;
        }

        /// <summary>
        /// Analiza la cadena de conexion y extrae configuracion de dispositivos y modo.
        /// </summary>
        /// <param name="cadenaConexion">Cadena JSON de conexion.</param>
        /// <param name="configuraciones">Lista de dispositivos configurados.</param>
        /// <param name="modo">Modo de sincronizacion resultante.</param>
        /// <param name="intervalo">Intervalo de polling resultante.</param>
        /// <param name="error">Mensaje de error si el parseo falla.</param>
        /// <returns>Verdadero si el parseo finaliza correctamente.</returns>
        private static bool TryParseConfiguracion(
            string cadenaConexion,
            out List<ConfiguracionDispositivo> configuraciones,
            out ModoSincronizacion modo,
            out int intervalo,
            out string error)
        {
            configuraciones = new List<ConfiguracionDispositivo>();
            modo = ModoSincronizacion.BajoDemanda;
            intervalo = 1000;
            error = string.Empty;

            try
            {
                JObject raiz = JObject.Parse(cadenaConexion);

                if (raiz["modo"] != null && Enum.TryParse(raiz["modo"]?.ToString(), true, out ModoSincronizacion modoParseado))
                {
                    modo = modoParseado;
                }

                if (raiz["intervaloPollingMs"] != null && int.TryParse(raiz["intervaloPollingMs"]?.ToString(), out int intervaloParseado) && intervaloParseado > 0)
                {
                    intervalo = intervaloParseado;
                }

                if (raiz["dispositivos"] is JArray dispositivosArray)
                {
                    foreach (JToken token in dispositivosArray)
                    {
                        if (token is JObject obj)
                        {
                            if (!TryCrearConfiguracion(obj, out ConfiguracionDispositivo cfg, out string errorCfg))
                            {
                                error = errorCfg;
                                return false;
                            }

                            configuraciones.Add(cfg);
                        }
                    }
                }
                else
                {
                    if (!TryCrearConfiguracion(raiz, out ConfiguracionDispositivo cfg, out string errorCfg))
                    {
                        error = errorCfg;
                        return false;
                    }

                    configuraciones.Add(cfg);
                }

                return true;
            }
            catch (Exception ex)
            {
                error = $"JSON invalido: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Crea la configuracion de un dispositivo a partir de un objeto JSON.
        /// </summary>
        /// <param name="objeto">Objeto JSON del dispositivo.</param>
        /// <param name="configuracion">Configuracion resultante.</param>
        /// <param name="error">Mensaje de error en caso de fallo.</param>
        /// <returns>Verdadero si la configuracion es valida.</returns>
        private static bool TryCrearConfiguracion(JObject objeto, out ConfiguracionDispositivo configuracion, out string error)
        {
            configuracion = new ConfiguracionDispositivo();
            error = string.Empty;

            string ip = objeto["ip"]?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(ip))
            {
                error = "El campo ip es obligatorio.";
                return false;
            }

            if (!int.TryParse(objeto["puerto"]?.ToString(), out int puerto) || puerto <= 0 || puerto > 65535)
            {
                error = "El campo puerto es obligatorio y debe estar entre 1 y 65535.";
                return false;
            }

            if (!byte.TryParse(objeto["unitId"]?.ToString(), out byte unitId))
            {
                unitId = 1;
            }

            if (!int.TryParse(objeto["timeoutMs"]?.ToString(), out int timeoutMs) || timeoutMs <= 0)
            {
                timeoutMs = 2000;
            }

            configuracion = new ConfiguracionDispositivo
            {
                Clave = objeto["clave"]?.ToString() ?? string.Empty,
                Ip = ip,
                Puerto = puerto,
                UnitId = unitId,
                TimeoutMs = timeoutMs
            };

            return true;
        }

        #endregion

        #region Clases internas

        /// <summary>
        /// Configuracion de un dispositivo Modbus dentro de la cadena de conexion.
        /// </summary>
        private sealed class ConfiguracionDispositivo
        {
            /// <summary>
            /// Clave logica opcional del dispositivo.
            /// </summary>
            public string Clave { get; set; } = string.Empty;

            /// <summary>
            /// Direccion IP del dispositivo.
            /// </summary>
            public string Ip { get; set; } = string.Empty;

            /// <summary>
            /// Puerto TCP del dispositivo.
            /// </summary>
            public int Puerto { get; set; }

            /// <summary>
            /// Unit Id por defecto del dispositivo.
            /// </summary>
            public byte UnitId { get; set; }

            /// <summary>
            /// Timeout en milisegundos para operaciones de red.
            /// </summary>
            public int TimeoutMs { get; set; }
        }

        /// <summary>
        /// Contexto de ejecucion de un dispositivo Modbus conectado.
        /// </summary>
        private sealed class ContextoDispositivo
        {
            /// <summary>
            /// Clave unica del dispositivo.
            /// </summary>
            public string Clave { get; set; } = string.Empty;

            /// <summary>
            /// Direccion IP del dispositivo.
            /// </summary>
            public string Ip { get; set; } = string.Empty;

            /// <summary>
            /// Puerto TCP del dispositivo.
            /// </summary>
            public int Puerto { get; set; }

            /// <summary>
            /// Unit Id por defecto del dispositivo.
            /// </summary>
            public byte UnitIdDefecto { get; set; }

            /// <summary>
            /// Timeout en milisegundos configurado para el socket.
            /// </summary>
            public int TimeoutMs { get; set; }

            /// <summary>
            /// Cliente TCP subyacente de la conexion Modbus.
            /// </summary>
            public TcpClient TcpClient { get; set; } = null!;

            /// <summary>
            /// Maestro Modbus asociado al cliente TCP.
            /// </summary>
            public ModbusIpMaster Master { get; set; } = null!;
        }

        #endregion
    }
}
