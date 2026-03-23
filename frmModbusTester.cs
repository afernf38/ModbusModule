using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
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

            lock (_lockPollingManual)
            {
                DetenerPollingManual();
            }

            _conexion.Desconectar(out _);
        }

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
