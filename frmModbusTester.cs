using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using UNE135411;
using ThreadingTimer = System.Threading.Timer;

namespace ModbusTester
{
    /// <summary>
    /// Formulario de pruebas para conexion Modbus TCP.
    /// </summary>
    public partial class frmModbusTester : Form
    {
        #region Campos privados

        /// <summary>
        /// Conexion principal usada por el formulario.
        /// </summary>
        private readonly clsConexionModbusTCP _conexion;

        /// <summary>
        /// Temporizador para polling manual de la pestana de polling.
        /// </summary>
        private ThreadingTimer? _timerPollingManual;

        /// <summary>
        /// Objeto de sincronizacion para operaciones del polling manual.
        /// </summary>
        private readonly object _lockPollingManual = new object();

        /// <summary>
        /// Contador de ciclos ejecutados por el polling manual.
        /// </summary>
        private int _contadorCiclosPolling;

        /// <summary>
        /// Indica si el formulario esta en proceso de cierre.
        /// </summary>
        private bool _cerrando;

        /// <summary>
        /// Token de cancelacion para carga completa de registros.
        /// </summary>
        private CancellationTokenSource? _scanLecturaCts;

        /// <summary>
        /// Indica si hay una carga completa de registros en ejecucion.
        /// </summary>
        private bool _scanLecturaEnCurso;

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="frmModbusTester"/>.
        /// </summary>
        public frmModbusTester()
        {
            InitializeComponent();

            _conexion = new clsConexionModbusTCP();
            _conexion.CambioEstadoConexion += Conexion_CambioEstadoConexion;
            _conexion.MensajeEntrante += Conexion_MensajeEntrante;

            CargarCombos();
            ActualizarEstadoConexion(false);
            AppendLog(rtbConexionLog, "Aplicacion iniciada.", Color.LightBlue);
        }

        #endregion

        #region Eventos de conexion

        /// <summary>
        /// Gestiona cambios de estado de la conexion.
        /// </summary>
        /// <param name="estado">Nuevo estado de la conexion.</param>
        private void Conexion_CambioEstadoConexion(bool estado)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<bool>(Conexion_CambioEstadoConexion), estado);
                return;
            }

            ActualizarEstadoConexion(estado);
            AppendLog(rtbConexionLog, estado ? "Conexion establecida correctamente." : "Conexion cerrada.", estado ? Color.LightGreen : Color.LightBlue);
        }

        /// <summary>
        /// Gestiona mensajes entrantes del modulo.
        /// </summary>
        /// <param name="mensaje">Mensaje JSON recibido.</param>
        /// <param name="codElemento">Codigo de elemento asociado.</param>
        private void Conexion_MensajeEntrante(JObject mensaje, string codElemento)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<JObject, string>(Conexion_MensajeEntrante), mensaje, codElemento);
                return;
            }

            AppendLog(rtbConexionLog, $"Evento interno [{codElemento}]: {mensaje}", Color.LightBlue);
        }

        #endregion

        #region Eventos de controles

        /// <summary>
        /// Ejecuta la accion de conectar.
        /// </summary>
        /// <param name="sender">Control origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void btnConectar_Click(object? sender, EventArgs e)
        {
            try
            {
                string cadenaConexion = new JObject
                {
                    ["ip"] = txtIp.Text.Trim(),
                    ["puerto"] = (int)numPuerto.Value,
                    ["unitId"] = (int)numUnitIdConexion.Value,
                    ["timeoutMs"] = (int)numTimeout.Value,
                    ["modo"] = clsConexionModbusTCP.ModoSincronizacion.BajoDemanda.ToString(),
                    ["intervaloPollingMs"] = 1000
                }.ToString();

                AppendLog(rtbConexionLog, $"Intentando conectar a {txtIp.Text.Trim()}:{(int)numPuerto.Value}...", Color.Yellow);

                bool ok = _conexion.Conectar(cadenaConexion, new List<clsElemento>(), out Dictionary<string, string> errores);
                if (ok)
                {
                    AppendLog(rtbConexionLog, "Conexion completada con exito.", Color.LightGreen);
                }
                else
                {
                    AppendLog(rtbConexionLog, FormatearErrores(errores), Color.OrangeRed);
                }
            }
            catch (Exception ex)
            {
                AppendLog(rtbConexionLog, $"Error al conectar: {ex.Message}", Color.OrangeRed);
            }
        }

        /// <summary>
        /// Ejecuta la accion de desconectar.
        /// </summary>
        /// <param name="sender">Control origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void btnDesconectar_Click(object? sender, EventArgs e)
        {
            try
            {
                AppendLog(rtbConexionLog, "Solicitando desconexion...", Color.Yellow);
                bool ok = _conexion.Desconectar(out Dictionary<string, string> errores);
                if (ok)
                {
                    AppendLog(rtbConexionLog, "Desconexion realizada con exito.", Color.LightGreen);
                }
                else
                {
                    AppendLog(rtbConexionLog, FormatearErrores(errores), Color.OrangeRed);
                }
            }
            catch (Exception ex)
            {
                AppendLog(rtbConexionLog, $"Error al desconectar: {ex.Message}", Color.OrangeRed);
            }
        }

        /// <summary>
        /// Ejecuta una lectura bajo demanda.
        /// </summary>
        /// <param name="sender">Control origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void btnLeer_Click(object? sender, EventArgs e)
        {
            try
            {
                clsConexionModbusTCP.ModbusComodin comodin = new clsConexionModbusTCP.ModbusComodin
                {
                    TipoRegistro = (clsConexionModbusTCP.TipoRegistro)cmbLecturaTipo.SelectedItem!,
                    Direccion = (ushort)numLecturaDireccion.Value,
                    Cantidad = (ushort)numLecturaCantidad.Value,
                    UnitId = (byte)numLecturaUnitId.Value
                };

                AppendLog(rtbLecturaLog, $"Lectura solicitada: {comodin.TipoRegistro} Dir={comodin.Direccion} Cant={comodin.Cantidad}", Color.Yellow);

                bool ok = _conexion.Leer(comodin, out object respuesta, out Dictionary<string, string> errores);
                if (ok)
                {
                    AppendLog(rtbLecturaLog, $"Resultado: {ConvertirResultadoLectura(respuesta)}", Color.Cyan);
                }
                else
                {
                    AppendLog(rtbLecturaLog, FormatearErrores(errores), Color.OrangeRed);
                }
            }
            catch (Exception ex)
            {
                AppendLog(rtbLecturaLog, $"Error de lectura: {ex.Message}", Color.OrangeRed);
            }
        }

        /// <summary>
        /// Inicia la carga completa de registros conocidos en segundo plano.
        /// </summary>
        /// <param name="sender">Control origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private async void btnLecturaScanTodo_Click(object? sender, EventArgs e)
        {
            if (_scanLecturaEnCurso)
            {
                return;
            }

            _scanLecturaEnCurso = true;
            _scanLecturaCts?.Dispose();
            _scanLecturaCts = new CancellationTokenSource();

            byte unitId = (byte)numLecturaUnitId.Value;

            PrepararUiInicioScan();
            AppendLog(rtbLecturaLog, "Carga de registros conocidos iniciada.", Color.Yellow);

            try
            {
                (int ok, int error) resultado = await Task.Run(() => CargarTodosLosRegistrosConocidos(unitId, _scanLecturaCts.Token));
                AppendLog(rtbLecturaLog, $"Carga completada. OK={resultado.ok}, Error={resultado.error}", Color.LightGreen);
            }
            catch (OperationCanceledException)
            {
                AppendLog(rtbLecturaLog, "Carga cancelada por el usuario.", Color.LightBlue);
            }
            catch (Exception ex)
            {
                AppendLog(rtbLecturaLog, $"Error durante carga: {ex.Message}", Color.OrangeRed);
            }
            finally
            {
                FinalizarUiScan();
                _scanLecturaCts?.Dispose();
                _scanLecturaCts = null;
                _scanLecturaEnCurso = false;
            }
        }

        /// <summary>
        /// Solicita la cancelacion de la carga completa de registros.
        /// </summary>
        /// <param name="sender">Control origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void btnLecturaCancelarScan_Click(object? sender, EventArgs e)
        {
            _scanLecturaCts?.Cancel();
        }

        /// <summary>
        /// Ejecuta una escritura bajo demanda.
        /// </summary>
        /// <param name="sender">Control origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void btnEscribir_Click(object? sender, EventArgs e)
        {
            try
            {
                clsConexionModbusTCP.ModbusComodin comodin = new clsConexionModbusTCP.ModbusComodin
                {
                    TipoRegistro = (clsConexionModbusTCP.TipoRegistro)cmbEscrituraTipo.SelectedItem!,
                    Direccion = (ushort)numEscrituraDireccion.Value,
                    Cantidad = (ushort)Math.Max(1, txtValoresEscritura.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length),
                    UnitId = (byte)numEscrituraUnitId.Value,
                    Valores = txtValoresEscritura.Text.Trim()
                };

                AppendLog(rtbEscrituraLog, $"Escritura solicitada: {comodin.TipoRegistro} Dir={comodin.Direccion} Valores={txtValoresEscritura.Text}", Color.Yellow);

                bool ok = _conexion.Escribir(comodin, out Dictionary<string, string> errores);
                if (ok)
                {
                    AppendLog(rtbEscrituraLog, "Escritura completada correctamente.", Color.LightGreen);
                }
                else
                {
                    AppendLog(rtbEscrituraLog, FormatearErrores(errores), Color.OrangeRed);
                }
            }
            catch (Exception ex)
            {
                AppendLog(rtbEscrituraLog, $"Error de escritura: {ex.Message}", Color.OrangeRed);
            }
        }

        /// <summary>
        /// Inicia el polling manual.
        /// </summary>
        /// <param name="sender">Control origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void btnPollingIniciar_Click(object? sender, EventArgs e)
        {
            lock (_lockPollingManual)
            {
                DetenerPollingManual();
                _contadorCiclosPolling = 0;
                lblPollingCiclosValor.Text = "0";
                int intervalo = (int)numPollingIntervalo.Value;
                _timerPollingManual = new ThreadingTimer(EjecutarPollingManual, null, intervalo, intervalo);
            }

            AppendLog(rtbPollingLog, "Polling manual iniciado.", Color.LightGreen);
        }

        /// <summary>
        /// Detiene el polling manual.
        /// </summary>
        /// <param name="sender">Control origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void btnPollingDetener_Click(object? sender, EventArgs e)
        {
            lock (_lockPollingManual)
            {
                DetenerPollingManual();
            }

            AppendLog(rtbPollingLog, "Polling manual detenido.", Color.LightBlue);
        }

        /// <summary>
        /// Limpia el contenido del log de polling.
        /// </summary>
        /// <param name="sender">Control origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void btnPollingLimpiar_Click(object? sender, EventArgs e)
        {
            if (rtbPollingLog.InvokeRequired)
            {
                rtbPollingLog.BeginInvoke(new Action<object?, EventArgs>(btnPollingLimpiar_Click), sender, e);
                return;
            }

            rtbPollingLog.Clear();
            AppendLog(rtbPollingLog, "Log de polling limpiado.", Color.LightBlue);
        }

        /// <summary>
        /// Detiene recursos al cerrar el formulario.
        /// </summary>
        /// <param name="sender">Control origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void frmModbusTester_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _cerrando = true;
            _scanLecturaCts?.Cancel();

            lock (_lockPollingManual)
            {
                DetenerPollingManual();
            }

            _conexion.Desconectar(out _);
        }

        #endregion

        #region Carga completa de lectura

        /// <summary>
        /// Definicion de un lote fijo para lectura de registros conocidos.
        /// </summary>
        /// <param name="Tipo">Tipo de registro Modbus.</param>
        /// <param name="DireccionInicio">Direccion inicial del lote.</param>
        /// <param name="Cantidad">Cantidad de registros a leer.</param>
        /// <param name="DescripcionPorOffset">Descripcion por offset relativo dentro del lote.</param>
        private sealed record LoteLecturaConocida(
            clsConexionModbusTCP.TipoRegistro Tipo,
            ushort DireccionInicio,
            ushort Cantidad,
            IReadOnlyDictionary<int, string> DescripcionPorOffset);

        /// <summary>
        /// Ejecuta la lectura de todos los lotes de registros conocidos del PLC.
        /// </summary>
        /// <param name="unitId">Unit ID de lectura.</param>
        /// <param name="token">Token de cancelacion.</param>
        /// <returns>Totales de filas OK y Error.</returns>
        private (int ok, int error) CargarTodosLosRegistrosConocidos(byte unitId, CancellationToken token)
        {
            List<LoteLecturaConocida> lotes = ConstruirMapaRegistrosConocidos();

            int totalOk = 0;
            int totalError = 0;
            int progreso = 0;

            foreach (LoteLecturaConocida lote in lotes)
            {
                token.ThrowIfCancellationRequested();

                clsConexionModbusTCP.ModbusComodin comodin = new clsConexionModbusTCP.ModbusComodin
                {
                    TipoRegistro = lote.Tipo,
                    Direccion = lote.DireccionInicio,
                    Cantidad = lote.Cantidad,
                    UnitId = unitId
                };

                int cantidad = lote.Cantidad;
                List<FilaScanRegistro> filas = new List<FilaScanRegistro>(cantidad);

                bool ok = _conexion.Leer(comodin, out object respuesta, out _);
                if (ok)
                {
                    string[] valores = ConvertirRespuestaABloque(respuesta, cantidad);
                    for (int i = 0; i < cantidad; i++)
                    {
                        int direccionActual = lote.DireccionInicio + i;
                        string descripcion = lote.DescripcionPorOffset.TryGetValue(i, out string? nombre) ? nombre : "(sin descripcion)";
                        filas.Add(new FilaScanRegistro(lote.Tipo.ToString(), direccionActual, descripcion, valores[i], "OK"));
                    }

                    totalOk += cantidad;
                }
                else
                {
                    for (int i = 0; i < cantidad; i++)
                    {
                        int direccionActual = lote.DireccionInicio + i;
                        string descripcion = lote.DescripcionPorOffset.TryGetValue(i, out string? nombre) ? nombre : "(sin descripcion)";
                        filas.Add(new FilaScanRegistro(lote.Tipo.ToString(), direccionActual, descripcion, string.Empty, "Error"));
                    }

                    totalError += cantidad;
                }

                AgregarFilasScan(filas);
                progreso += cantidad;
                ActualizarProgresoScan(progreso);
            }

            return (totalOk, totalError);
        }

        /// <summary>
        /// Construye el mapa fijo de lotes y descripciones conocidas del PLC WAGO.
        /// </summary>
        /// <returns>Lista de lotes a leer.</returns>
        private static List<LoteLecturaConocida> ConstruirMapaRegistrosConocidos()
        {
            Dictionary<int, string> ai = new Dictionary<int, string>
            {
                [0] = "Luminancimetro 1",
                [1] = "Detector CO2 1",
                [2] = "Anemometro 1",
                [3] = "Detector NO 1",
                [4] = "Opacimetro 1",
                [5] = "Opacimetro 2",
                [6] = "Luminancimetro 2",
                [7] = "Detector CO2 2",
                [8] = "Anemometro 2",
                [9] = "Detector NO 2",
                [10] = "(reserved)",
                [11] = "(reserved)"
            };

            Dictionary<int, string> di = new Dictionary<int, string>
            {
                [0] = "Galibo alerta 1",
                [1] = "Extintor puerta abierta",
                [2] = "BIE puerta abierta",
                [3] = "Galibo alerta 2",
                [4] = "Zona de incendio 1a",
                [5] = "Zona de incendio 1b",
                [6] = "Zona de incendio 2a",
                [7] = "Zona de incendio 2b"
            };

            Dictionary<int, string> doFisicos = Enumerable.Range(0, 176)
                .ToDictionary(i => i, i => i == 0 ? "All traffic lights and actuators (RAV/SEM, CIS/CIC/CIN, JET, exutories, grilles, signs, barriers, dampers, STOP, axial fans, etc.)" : "(mapped output)");

            Dictionary<int, string> sdv = Enumerable.Range(0, 16)
                .ToDictionary(i => i, i => i == 0 ? "Barrier locks, damper close, exutory close commands" : "(virtual output)");

            Dictionary<int, string> edv = Enumerable.Range(0, 112)
                .ToDictionary(i => i, i => i == 0 ? "Barrier/ventilation/grille/damper/exutory/JET states" : "(virtual input)");

            return new List<LoteLecturaConocida>
            {
                new LoteLecturaConocida(clsConexionModbusTCP.TipoRegistro.InputRegister, 256, 12, ai),
                new LoteLecturaConocida(clsConexionModbusTCP.TipoRegistro.DiscreteInput, 4800, 8, di),
                new LoteLecturaConocida(clsConexionModbusTCP.TipoRegistro.Coil, 8192, 176, doFisicos),
                new LoteLecturaConocida(clsConexionModbusTCP.TipoRegistro.Coil, 8368, 16, sdv),
                new LoteLecturaConocida(clsConexionModbusTCP.TipoRegistro.DiscreteInput, 4808, 112, edv)
            };
        }

        /// <summary>
        /// Convierte la respuesta de lectura a un arreglo de textos de longitud fija.
        /// </summary>
        /// <param name="respuesta">Respuesta de lectura.</param>
        /// <param name="cantidad">Cantidad esperada de elementos.</param>
        /// <returns>Arreglo de texto con un valor por direccion.</returns>
        private static string[] ConvertirRespuestaABloque(object respuesta, int cantidad)
        {
            string[] valores = respuesta switch
            {
                bool[] bools => bools.Select(v => v ? "1" : "0").ToArray(),
                ushort[] ushorts => ushorts.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToArray(),
                Array array => array.Cast<object>().Select(v => v?.ToString() ?? string.Empty).ToArray(),
                _ => new[] { respuesta?.ToString() ?? string.Empty }
            };

            if (valores.Length == cantidad)
            {
                return valores;
            }

            string[] normalizado = Enumerable.Repeat(string.Empty, cantidad).ToArray();
            int limite = Math.Min(cantidad, valores.Length);
            for (int i = 0; i < limite; i++)
            {
                normalizado[i] = valores[i];
            }

            return normalizado;
        }

        /// <summary>
        /// Prepara la interfaz antes de iniciar la carga completa.
        /// </summary>
        private void PrepararUiInicioScan()
        {
            dgvLecturaScan.Rows.Clear();
            pbLecturaScan.Minimum = 0;
            pbLecturaScan.Maximum = 324;
            pbLecturaScan.Value = 0;

            btnLeer.Enabled = false;
            btnLecturaScanTodo.Enabled = false;
            btnLecturaCancelarScan.Enabled = true;
        }

        /// <summary>
        /// Restaura la interfaz al finalizar o cancelar la carga completa.
        /// </summary>
        private void FinalizarUiScan()
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(FinalizarUiScan));
                return;
            }

            btnLeer.Enabled = true;
            btnLecturaScanTodo.Enabled = true;
            btnLecturaCancelarScan.Enabled = false;
        }

        /// <summary>
        /// Agrega un bloque de filas a la grilla de resultados.
        /// </summary>
        /// <param name="filas">Filas a insertar.</param>
        private void AgregarFilasScan(IReadOnlyCollection<FilaScanRegistro> filas)
        {
            if (dgvLecturaScan.IsDisposed)
            {
                return;
            }

            if (dgvLecturaScan.InvokeRequired)
            {
                dgvLecturaScan.BeginInvoke(new Action<IReadOnlyCollection<FilaScanRegistro>>(AgregarFilasScan), filas);
                return;
            }

            foreach (FilaScanRegistro fila in filas)
            {
                dgvLecturaScan.Rows.Add(fila.Tipo, fila.Direccion, fila.Descripcion, fila.Valor, fila.Estado);
            }
        }

        /// <summary>
        /// Actualiza la barra de progreso de la carga completa.
        /// </summary>
        /// <param name="valor">Valor acumulado de progreso.</param>
        private void ActualizarProgresoScan(int valor)
        {
            if (pbLecturaScan.IsDisposed)
            {
                return;
            }

            if (pbLecturaScan.InvokeRequired)
            {
                pbLecturaScan.BeginInvoke(new Action<int>(ActualizarProgresoScan), valor);
                return;
            }

            int valorNormalizado = Math.Max(pbLecturaScan.Minimum, Math.Min(pbLecturaScan.Maximum, valor));
            pbLecturaScan.Value = valorNormalizado;
        }

        /// <summary>
        /// Representa una fila de resultado para la carga completa.
        /// </summary>
        /// <param name="Tipo">Tipo de registro.</param>
        /// <param name="Direccion">Direccion del registro.</param>
        /// <param name="Descripcion">Descripcion de la senal.</param>
        /// <param name="Valor">Valor leido.</param>
        /// <param name="Estado">Estado de lectura.</param>
        private sealed record FilaScanRegistro(string Tipo, int Direccion, string Descripcion, string Valor, string Estado);

        #endregion

        #region Polling manual

        /// <summary>
        /// Ejecuta cada ciclo del polling manual.
        /// </summary>
        /// <param name="state">Estado del temporizador.</param>
        private void EjecutarPollingManual(object? state)
        {
            if (_cerrando)
            {
                return;
            }

            try
            {
                clsConexionModbusTCP.ModbusComodin? comodin = ConstruirComodinPollingManualThreadSafe();
                if (comodin == null)
                {
                    return;
                }

                bool ok = _conexion.Leer(comodin, out object respuesta, out Dictionary<string, string> errores);
                string marca = DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);

                if (ok)
                {
                    int ciclos = Interlocked.Increment(ref _contadorCiclosPolling);
                    ActualizarCiclosPolling(ciclos);
                    AppendLog(rtbPollingLog, $"[{marca}] Ciclo {ciclos}: {ConvertirResultadoLectura(respuesta)}", Color.Cyan);
                }
                else
                {
                    AppendLog(rtbPollingLog, $"[{marca}] Error: {FormatearErrores(errores)}", Color.OrangeRed);
                }
            }
            catch (Exception ex)
            {
                string marca = DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
                AppendLog(rtbPollingLog, $"[{marca}] Excepcion en polling: {ex.Message}", Color.OrangeRed);
            }
        }

        /// <summary>
        /// Actualiza el contador visual de ciclos.
        /// </summary>
        /// <param name="ciclos">Numero de ciclos completados.</param>
        private void ActualizarCiclosPolling(int ciclos)
        {
            if (lblPollingCiclosValor.InvokeRequired)
            {
                lblPollingCiclosValor.BeginInvoke(new Action<int>(ActualizarCiclosPolling), ciclos);
                return;
            }

            lblPollingCiclosValor.Text = ciclos.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Detiene y libera el timer de polling manual.
        /// </summary>
        private void DetenerPollingManual()
        {
            _timerPollingManual?.Change(Timeout.Infinite, Timeout.Infinite);
            _timerPollingManual?.Dispose();
            _timerPollingManual = null;
        }

        /// <summary>
        /// Construye de forma segura el objeto comodin para el ciclo de polling manual.
        /// </summary>
        /// <returns>Objeto comodin si la configuracion es valida; en caso contrario, nulo.</returns>
        private clsConexionModbusTCP.ModbusComodin? ConstruirComodinPollingManualThreadSafe()
        {
            if (IsDisposed)
            {
                return null;
            }

            if (InvokeRequired)
            {
                return (clsConexionModbusTCP.ModbusComodin?)Invoke(new Func<clsConexionModbusTCP.ModbusComodin?>(ConstruirComodinPollingManualThreadSafe));
            }

            if (cmbPollingTipo.SelectedItem is not clsConexionModbusTCP.TipoRegistro tipo)
            {
                return null;
            }

            return new clsConexionModbusTCP.ModbusComodin
            {
                TipoRegistro = tipo,
                Direccion = (ushort)numPollingDireccion.Value,
                Cantidad = (ushort)numPollingCantidad.Value,
                UnitId = (byte)numPollingUnitId.Value
            };
        }

        #endregion

        #region Utilidades privadas

        /// <summary>
        /// Carga los combos con tipos de registro.
        /// </summary>
        private void CargarCombos()
        {
            cmbLecturaTipo.Items.Clear();
            cmbPollingTipo.Items.Clear();
            cmbEscrituraTipo.Items.Clear();

            foreach (clsConexionModbusTCP.TipoRegistro tipo in Enum.GetValues(typeof(clsConexionModbusTCP.TipoRegistro)))
            {
                cmbLecturaTipo.Items.Add(tipo);
                cmbPollingTipo.Items.Add(tipo);
            }

            cmbEscrituraTipo.Items.Add(clsConexionModbusTCP.TipoRegistro.Coil);
            cmbEscrituraTipo.Items.Add(clsConexionModbusTCP.TipoRegistro.HoldingRegister);

            cmbLecturaTipo.SelectedIndex = 0;
            cmbPollingTipo.SelectedIndex = 0;
            cmbEscrituraTipo.SelectedIndex = 0;
            ActualizarAyudaEscritura();
        }

        /// <summary>
        /// Actualiza el texto de ayuda de formato para la escritura.
        /// </summary>
        private void ActualizarAyudaEscritura()
        {
            if (cmbEscrituraTipo.SelectedItem is clsConexionModbusTCP.TipoRegistro tipo)
            {
                lblEscrituraAyuda.Text = tipo == clsConexionModbusTCP.TipoRegistro.Coil
                    ? "Formato esperado: 1,0,1 o true,false,true"
                    : "Formato esperado: 100,200,300";
            }
        }

        /// <summary>
        /// Maneja el cambio de tipo de registro en la pestana de escritura.
        /// </summary>
        /// <param name="sender">Control origen del evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void cmbEscrituraTipo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ActualizarAyudaEscritura();
        }

        /// <summary>
        /// Actualiza el indicador visual de estado de conexion.
        /// </summary>
        /// <param name="conectado">Estado de conexion.</param>
        private void ActualizarEstadoConexion(bool conectado)
        {
            if (picEstado.InvokeRequired)
            {
                picEstado.BeginInvoke(new Action<bool>(ActualizarEstadoConexion), conectado);
                return;
            }

            picEstado.BackColor = conectado ? Color.LimeGreen : Color.IndianRed;
            lblEstadoValor.Text = conectado ? "Conectado" : "Desconectado";
        }

        /// <summary>
        /// Convierte una respuesta de lectura en texto.
        /// </summary>
        /// <param name="respuesta">Respuesta de lectura.</param>
        /// <returns>Texto legible de la respuesta.</returns>
        private static string ConvertirResultadoLectura(object respuesta)
        {
            if (respuesta is bool[] bools)
            {
                return string.Join(", ", bools.Select(v => v ? "1" : "0"));
            }

            if (respuesta is ushort[] ushorts)
            {
                return string.Join(", ", ushorts);
            }

            if (respuesta is Array array)
            {
                return string.Join(", ", array.Cast<object>());
            }

            return respuesta?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Convierte el diccionario de errores a una sola linea de texto.
        /// </summary>
        /// <param name="errores">Diccionario de errores.</param>
        /// <returns>Texto formateado.</returns>
        private static string FormatearErrores(Dictionary<string, string> errores)
        {
            if (errores == null || errores.Count == 0)
            {
                return "Operacion fallida sin detalle de error.";
            }

            return string.Join(" | ", errores.Select(kv => $"{kv.Key}: {kv.Value}"));
        }

        /// <summary>
        /// Agrega una entrada de log con color y auto-limpieza.
        /// </summary>
        /// <param name="log">Control de log destino.</param>
        /// <param name="texto">Texto de la entrada.</param>
        /// <param name="color">Color del texto.</param>
        private static void AppendLog(RichTextBox log, string texto, Color color)
        {
            if (log.IsDisposed)
            {
                return;
            }

            if (log.InvokeRequired)
            {
                log.BeginInvoke(new Action<RichTextBox, string, Color>(AppendLog), log, texto, color);
                return;
            }

            if (log.Lines.Length > 500)
            {
                log.Clear();
            }

            string prefijo = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            log.SelectionStart = log.TextLength;
            log.SelectionLength = 0;
            log.SelectionColor = color;
            log.AppendText($"[{prefijo}] {texto}{Environment.NewLine}");
            log.SelectionColor = log.ForeColor;
            log.ScrollToCaret();
        }

        #endregion
    }
}
