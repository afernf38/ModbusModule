using System;
using System.Drawing;
using System.Windows.Forms;

namespace ModbusTester
{
    /// <summary>
    /// Parte de diseno del formulario de pruebas Modbus.
    /// </summary>
    partial class frmModbusTester
    {
        #region Campos de diseno

        /// <summary>
        /// Contenedor de componentes del formulario.
        /// </summary>
        private System.ComponentModel.IContainer? components = null;

        /// <summary>
        /// Control principal de pestanas.
        /// </summary>
        private TabControl tabControlPrincipal = null!;

        /// <summary>
        /// Pestana de conexion.
        /// </summary>
        private TabPage tabConexion = null!;

        /// <summary>
        /// Pestana de lectura.
        /// </summary>
        private TabPage tabLectura = null!;

        /// <summary>
        /// Pestana de escritura.
        /// </summary>
        private TabPage tabEscritura = null!;

        /// <summary>
        /// Pestana de polling.
        /// </summary>
        private TabPage tabPolling = null!;

        /// <summary>
        /// Etiqueta de IP.
        /// </summary>
        private Label lblIp = null!;

        /// <summary>
        /// Caja de texto de IP.
        /// </summary>
        private TextBox txtIp = null!;

        /// <summary>
        /// Etiqueta de puerto.
        /// </summary>
        private Label lblPuerto = null!;

        /// <summary>
        /// Selector de puerto.
        /// </summary>
        private NumericUpDown numPuerto = null!;

        /// <summary>
        /// Etiqueta de Unit ID en conexion.
        /// </summary>
        private Label lblUnitIdConexion = null!;

        /// <summary>
        /// Selector Unit ID en conexion.
        /// </summary>
        private NumericUpDown numUnitIdConexion = null!;

        /// <summary>
        /// Etiqueta de timeout.
        /// </summary>
        private Label lblTimeout = null!;

        /// <summary>
        /// Selector de timeout.
        /// </summary>
        private NumericUpDown numTimeout = null!;

        /// <summary>
        /// Boton conectar.
        /// </summary>
        private Button btnConectar = null!;

        /// <summary>
        /// Boton desconectar.
        /// </summary>
        private Button btnDesconectar = null!;

        /// <summary>
        /// Indicador visual de estado.
        /// </summary>
        private PictureBox picEstado = null!;

        /// <summary>
        /// Etiqueta de titulo para estado.
        /// </summary>
        private Label lblEstadoTitulo = null!;

        /// <summary>
        /// Etiqueta de valor de estado.
        /// </summary>
        private Label lblEstadoValor = null!;

        /// <summary>
        /// Log de conexion.
        /// </summary>
        private RichTextBox rtbConexionLog = null!;

        /// <summary>
        /// Combo de tipo en lectura.
        /// </summary>
        private ComboBox cmbLecturaTipo = null!;

        /// <summary>
        /// Etiqueta tipo lectura.
        /// </summary>
        private Label lblLecturaTipo = null!;

        /// <summary>
        /// Etiqueta direccion lectura.
        /// </summary>
        private Label lblLecturaDireccion = null!;

        /// <summary>
        /// Selector direccion lectura.
        /// </summary>
        private NumericUpDown numLecturaDireccion = null!;

        /// <summary>
        /// Etiqueta cantidad lectura.
        /// </summary>
        private Label lblLecturaCantidad = null!;

        /// <summary>
        /// Selector cantidad lectura.
        /// </summary>
        private NumericUpDown numLecturaCantidad = null!;

        /// <summary>
        /// Etiqueta unit id lectura.
        /// </summary>
        private Label lblLecturaUnitId = null!;

        /// <summary>
        /// Selector unit id lectura.
        /// </summary>
        private NumericUpDown numLecturaUnitId = null!;

        /// <summary>
        /// Boton leer.
        /// </summary>
        private Button btnLeer = null!;

        /// <summary>
        /// Log de lectura.
        /// </summary>
        private RichTextBox rtbLecturaLog = null!;

        /// <summary>
        /// Combo de tipo escritura.
        /// </summary>
        private ComboBox cmbEscrituraTipo = null!;

        /// <summary>
        /// Etiqueta tipo escritura.
        /// </summary>
        private Label lblEscrituraTipo = null!;

        /// <summary>
        /// Etiqueta direccion escritura.
        /// </summary>
        private Label lblEscrituraDireccion = null!;

        /// <summary>
        /// Selector direccion escritura.
        /// </summary>
        private NumericUpDown numEscrituraDireccion = null!;

        /// <summary>
        /// Etiqueta unit id escritura.
        /// </summary>
        private Label lblEscrituraUnitId = null!;

        /// <summary>
        /// Selector unit id escritura.
        /// </summary>
        private NumericUpDown numEscrituraUnitId = null!;

        /// <summary>
        /// Etiqueta valores escritura.
        /// </summary>
        private Label lblEscrituraValores = null!;

        /// <summary>
        /// Caja de texto de valores escritura.
        /// </summary>
        private TextBox txtValoresEscritura = null!;

        /// <summary>
        /// Etiqueta de ayuda para formato de valores de escritura.
        /// </summary>
        private Label lblEscrituraAyuda = null!;

        /// <summary>
        /// Boton escribir.
        /// </summary>
        private Button btnEscribir = null!;

        /// <summary>
        /// Log de escritura.
        /// </summary>
        private RichTextBox rtbEscrituraLog = null!;

        /// <summary>
        /// Combo tipo polling.
        /// </summary>
        private ComboBox cmbPollingTipo = null!;

        /// <summary>
        /// Etiqueta tipo polling.
        /// </summary>
        private Label lblPollingTipo = null!;

        /// <summary>
        /// Etiqueta direccion polling.
        /// </summary>
        private Label lblPollingDireccion = null!;

        /// <summary>
        /// Selector direccion polling.
        /// </summary>
        private NumericUpDown numPollingDireccion = null!;

        /// <summary>
        /// Etiqueta cantidad polling.
        /// </summary>
        private Label lblPollingCantidad = null!;

        /// <summary>
        /// Selector cantidad polling.
        /// </summary>
        private NumericUpDown numPollingCantidad = null!;

        /// <summary>
        /// Etiqueta intervalo polling.
        /// </summary>
        private Label lblPollingIntervalo = null!;

        /// <summary>
        /// Selector intervalo polling.
        /// </summary>
        private NumericUpDown numPollingIntervalo = null!;

        /// <summary>
        /// Etiqueta unit id polling.
        /// </summary>
        private Label lblPollingUnitId = null!;

        /// <summary>
        /// Selector unit id polling.
        /// </summary>
        private NumericUpDown numPollingUnitId = null!;

        /// <summary>
        /// Boton iniciar polling.
        /// </summary>
        private Button btnPollingIniciar = null!;

        /// <summary>
        /// Boton detener polling.
        /// </summary>
        private Button btnPollingDetener = null!;

        /// <summary>
        /// Boton limpiar log de polling.
        /// </summary>
        private Button btnPollingLimpiar = null!;

        /// <summary>
        /// Etiqueta de titulo de ciclos.
        /// </summary>
        private Label lblPollingCiclosTitulo = null!;

        /// <summary>
        /// Etiqueta de valor de ciclos.
        /// </summary>
        private Label lblPollingCiclosValor = null!;

        /// <summary>
        /// Log de polling.
        /// </summary>
        private RichTextBox rtbPollingLog = null!;

        #endregion

        #region Dispose

        /// <summary>
        /// Libera recursos de los componentes del formulario.
        /// </summary>
        /// <param name="disposing">Indica si se deben liberar recursos administrados.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Inicializacion de diseno

        /// <summary>
        /// Configura todos los controles visuales del formulario.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            tabControlPrincipal = new TabControl();
            tabConexion = new TabPage();
            tabLectura = new TabPage();
            tabEscritura = new TabPage();
            tabPolling = new TabPage();

            lblIp = new Label();
            txtIp = new TextBox();
            lblPuerto = new Label();
            numPuerto = new NumericUpDown();
            lblUnitIdConexion = new Label();
            numUnitIdConexion = new NumericUpDown();
            lblTimeout = new Label();
            numTimeout = new NumericUpDown();
            btnConectar = new Button();
            btnDesconectar = new Button();
            picEstado = new PictureBox();
            lblEstadoTitulo = new Label();
            lblEstadoValor = new Label();
            rtbConexionLog = new RichTextBox();

            lblLecturaTipo = new Label();
            cmbLecturaTipo = new ComboBox();
            lblLecturaDireccion = new Label();
            numLecturaDireccion = new NumericUpDown();
            lblLecturaCantidad = new Label();
            numLecturaCantidad = new NumericUpDown();
            lblLecturaUnitId = new Label();
            numLecturaUnitId = new NumericUpDown();
            btnLeer = new Button();
            rtbLecturaLog = new RichTextBox();

            lblEscrituraTipo = new Label();
            cmbEscrituraTipo = new ComboBox();
            lblEscrituraDireccion = new Label();
            numEscrituraDireccion = new NumericUpDown();
            lblEscrituraUnitId = new Label();
            numEscrituraUnitId = new NumericUpDown();
            lblEscrituraValores = new Label();
            txtValoresEscritura = new TextBox();
            lblEscrituraAyuda = new Label();
            btnEscribir = new Button();
            rtbEscrituraLog = new RichTextBox();

            lblPollingTipo = new Label();
            cmbPollingTipo = new ComboBox();
            lblPollingDireccion = new Label();
            numPollingDireccion = new NumericUpDown();
            lblPollingCantidad = new Label();
            numPollingCantidad = new NumericUpDown();
            lblPollingIntervalo = new Label();
            numPollingIntervalo = new NumericUpDown();
            lblPollingUnitId = new Label();
            numPollingUnitId = new NumericUpDown();
            btnPollingIniciar = new Button();
            btnPollingDetener = new Button();
            btnPollingLimpiar = new Button();
            lblPollingCiclosTitulo = new Label();
            lblPollingCiclosValor = new Label();
            rtbPollingLog = new RichTextBox();

            SuspendLayout();

            tabControlPrincipal.Dock = DockStyle.Fill;
            tabControlPrincipal.TabPages.AddRange(new[] { tabConexion, tabLectura, tabEscritura, tabPolling });

            tabConexion.Text = "🔌 Conexión";
            tabLectura.Text = "📖 Lectura";
            tabEscritura.Text = "✏️ Escritura";
            tabPolling.Text = "🔄 Polling";

            ConfigurarTabConexion();
            ConfigurarTabLectura();
            ConfigurarTabEscritura();
            ConfigurarTabPolling();

            Controls.Add(tabControlPrincipal);

            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(980, 620);
            MinimumSize = new Size(980, 620);
            Name = "frmModbusTester";
            Text = "Modbus TCP Tester";
            FormClosing += frmModbusTester_FormClosing;

            ResumeLayout(false);
        }

        #endregion

        #region Configuracion de tabs

        /// <summary>
        /// Configura controles de la pestana Conexion.
        /// </summary>
        private void ConfigurarTabConexion()
        {
            lblIp.Text = "IP:";
            lblIp.Location = new Point(20, 20);
            lblIp.AutoSize = true;

            txtIp.Location = new Point(140, 16);
            txtIp.Size = new Size(160, 23);
            txtIp.Text = "127.0.0.1";

            lblPuerto.Text = "Puerto:";
            lblPuerto.Location = new Point(20, 55);
            lblPuerto.AutoSize = true;

            numPuerto.Location = new Point(140, 51);
            numPuerto.Minimum = 1;
            numPuerto.Maximum = 65535;
            numPuerto.Value = 502;

            lblUnitIdConexion.Text = "Unit ID:";
            lblUnitIdConexion.Location = new Point(20, 90);
            lblUnitIdConexion.AutoSize = true;

            numUnitIdConexion.Location = new Point(140, 86);
            numUnitIdConexion.Minimum = 1;
            numUnitIdConexion.Maximum = 247;
            numUnitIdConexion.Value = 1;

            lblTimeout.Text = "Timeout (ms):";
            lblTimeout.Location = new Point(20, 125);
            lblTimeout.AutoSize = true;

            numTimeout.Location = new Point(140, 121);
            numTimeout.Minimum = 100;
            numTimeout.Maximum = 30000;
            numTimeout.Increment = 100;
            numTimeout.Value = 2000;

            btnConectar.Text = "Conectar";
            btnConectar.Location = new Point(20, 165);
            btnConectar.Size = new Size(130, 32);
            btnConectar.BackColor = Color.SeaGreen;
            btnConectar.ForeColor = Color.White;
            btnConectar.FlatStyle = FlatStyle.Flat;
            btnConectar.Click += btnConectar_Click;

            btnDesconectar.Text = "Desconectar";
            btnDesconectar.Location = new Point(170, 165);
            btnDesconectar.Size = new Size(130, 32);
            btnDesconectar.BackColor = Color.Firebrick;
            btnDesconectar.ForeColor = Color.White;
            btnDesconectar.FlatStyle = FlatStyle.Flat;
            btnDesconectar.Click += btnDesconectar_Click;

            picEstado.Location = new Point(20, 220);
            picEstado.Size = new Size(20, 20);
            picEstado.BackColor = Color.IndianRed;

            lblEstadoTitulo.Text = "Estado:";
            lblEstadoTitulo.Location = new Point(50, 222);
            lblEstadoTitulo.AutoSize = true;

            lblEstadoValor.Text = "Desconectado";
            lblEstadoValor.Location = new Point(105, 222);
            lblEstadoValor.AutoSize = true;

            ConfigurarLog(rtbConexionLog);
            rtbConexionLog.Location = new Point(330, 16);
            rtbConexionLog.Size = new Size(620, 540);

            tabConexion.Controls.AddRange(new Control[]
            {
                lblIp,
                txtIp,
                lblPuerto,
                numPuerto,
                lblUnitIdConexion,
                numUnitIdConexion,
                lblTimeout,
                numTimeout,
                btnConectar,
                btnDesconectar,
                picEstado,
                lblEstadoTitulo,
                lblEstadoValor,
                rtbConexionLog
            });
        }

        /// <summary>
        /// Configura controles de la pestana Lectura.
        /// </summary>
        private void ConfigurarTabLectura()
        {
            lblLecturaTipo.Text = "Tipo registro:";
            lblLecturaTipo.Location = new Point(20, 20);
            lblLecturaTipo.AutoSize = true;

            cmbLecturaTipo.Location = new Point(150, 16);
            cmbLecturaTipo.Size = new Size(180, 23);
            cmbLecturaTipo.DropDownStyle = ComboBoxStyle.DropDownList;

            lblLecturaDireccion.Text = "Direccion:";
            lblLecturaDireccion.Location = new Point(20, 55);
            lblLecturaDireccion.AutoSize = true;

            numLecturaDireccion.Location = new Point(150, 51);
            numLecturaDireccion.Maximum = ushort.MaxValue;

            lblLecturaCantidad.Text = "Cantidad:";
            lblLecturaCantidad.Location = new Point(20, 90);
            lblLecturaCantidad.AutoSize = true;

            numLecturaCantidad.Location = new Point(150, 86);
            numLecturaCantidad.Minimum = 1;
            numLecturaCantidad.Maximum = 125;
            numLecturaCantidad.Value = 1;

            lblLecturaUnitId.Text = "Unit ID:";
            lblLecturaUnitId.Location = new Point(20, 125);
            lblLecturaUnitId.AutoSize = true;

            numLecturaUnitId.Location = new Point(150, 121);
            numLecturaUnitId.Minimum = 1;
            numLecturaUnitId.Maximum = 247;
            numLecturaUnitId.Value = 1;

            btnLeer.Text = "Leer";
            btnLeer.Location = new Point(20, 165);
            btnLeer.Size = new Size(130, 32);
            btnLeer.BackColor = Color.RoyalBlue;
            btnLeer.ForeColor = Color.White;
            btnLeer.FlatStyle = FlatStyle.Flat;
            btnLeer.Click += btnLeer_Click;

            ConfigurarLog(rtbLecturaLog);
            rtbLecturaLog.Location = new Point(350, 16);
            rtbLecturaLog.Size = new Size(600, 540);

            tabLectura.Controls.AddRange(new Control[]
            {
                lblLecturaTipo,
                cmbLecturaTipo,
                lblLecturaDireccion,
                numLecturaDireccion,
                lblLecturaCantidad,
                numLecturaCantidad,
                lblLecturaUnitId,
                numLecturaUnitId,
                btnLeer,
                rtbLecturaLog
            });
        }

        /// <summary>
        /// Configura controles de la pestana Escritura.
        /// </summary>
        private void ConfigurarTabEscritura()
        {
            lblEscrituraTipo.Text = "Tipo registro:";
            lblEscrituraTipo.Location = new Point(20, 20);
            lblEscrituraTipo.AutoSize = true;

            cmbEscrituraTipo.Location = new Point(150, 16);
            cmbEscrituraTipo.Size = new Size(180, 23);
            cmbEscrituraTipo.DropDownStyle = ComboBoxStyle.DropDownList;

            lblEscrituraDireccion.Text = "Direccion:";
            lblEscrituraDireccion.Location = new Point(20, 55);
            lblEscrituraDireccion.AutoSize = true;

            numEscrituraDireccion.Location = new Point(150, 51);
            numEscrituraDireccion.Maximum = ushort.MaxValue;

            lblEscrituraUnitId.Text = "Unit ID:";
            lblEscrituraUnitId.Location = new Point(20, 90);
            lblEscrituraUnitId.AutoSize = true;

            numEscrituraUnitId.Location = new Point(150, 86);
            numEscrituraUnitId.Minimum = 1;
            numEscrituraUnitId.Maximum = 247;
            numEscrituraUnitId.Value = 1;

            lblEscrituraValores.Text = "Valores (coma):";
            lblEscrituraValores.Location = new Point(20, 125);
            lblEscrituraValores.AutoSize = true;

            txtValoresEscritura.Location = new Point(150, 121);
            txtValoresEscritura.Size = new Size(180, 23);
            txtValoresEscritura.Text = "1";

            lblEscrituraAyuda.Text = "Formato esperado: 1,0,1 o true,false,true";
            lblEscrituraAyuda.Location = new Point(20, 151);
            lblEscrituraAyuda.Size = new Size(310, 15);
            lblEscrituraAyuda.AutoSize = false;

            btnEscribir.Text = "Escribir";
            btnEscribir.Location = new Point(20, 180);
            btnEscribir.Size = new Size(130, 32);
            btnEscribir.BackColor = Color.DarkOrange;
            btnEscribir.ForeColor = Color.White;
            btnEscribir.FlatStyle = FlatStyle.Flat;
            btnEscribir.Click += btnEscribir_Click;

            cmbEscrituraTipo.SelectedIndexChanged += cmbEscrituraTipo_SelectedIndexChanged;

            ConfigurarLog(rtbEscrituraLog);
            rtbEscrituraLog.Location = new Point(350, 16);
            rtbEscrituraLog.Size = new Size(600, 540);

            tabEscritura.Controls.AddRange(new Control[]
            {
                lblEscrituraTipo,
                cmbEscrituraTipo,
                lblEscrituraDireccion,
                numEscrituraDireccion,
                lblEscrituraUnitId,
                numEscrituraUnitId,
                lblEscrituraValores,
                txtValoresEscritura,
                lblEscrituraAyuda,
                btnEscribir,
                rtbEscrituraLog
            });
        }

        /// <summary>
        /// Configura controles de la pestana Polling.
        /// </summary>
        private void ConfigurarTabPolling()
        {
            lblPollingTipo.Text = "Tipo registro:";
            lblPollingTipo.Location = new Point(20, 20);
            lblPollingTipo.AutoSize = true;

            cmbPollingTipo.Location = new Point(150, 16);
            cmbPollingTipo.Size = new Size(180, 23);
            cmbPollingTipo.DropDownStyle = ComboBoxStyle.DropDownList;

            lblPollingDireccion.Text = "Direccion:";
            lblPollingDireccion.Location = new Point(20, 55);
            lblPollingDireccion.AutoSize = true;

            numPollingDireccion.Location = new Point(150, 51);
            numPollingDireccion.Maximum = ushort.MaxValue;

            lblPollingCantidad.Text = "Cantidad:";
            lblPollingCantidad.Location = new Point(20, 90);
            lblPollingCantidad.AutoSize = true;

            numPollingCantidad.Location = new Point(150, 86);
            numPollingCantidad.Minimum = 1;
            numPollingCantidad.Maximum = 125;
            numPollingCantidad.Value = 1;

            lblPollingIntervalo.Text = "Intervalo (ms):";
            lblPollingIntervalo.Location = new Point(20, 125);
            lblPollingIntervalo.AutoSize = true;

            numPollingIntervalo.Location = new Point(150, 121);
            numPollingIntervalo.Minimum = 100;
            numPollingIntervalo.Maximum = 60000;
            numPollingIntervalo.Increment = 100;
            numPollingIntervalo.Value = 1000;

            lblPollingUnitId.Text = "Unit ID:";
            lblPollingUnitId.Location = new Point(20, 160);
            lblPollingUnitId.AutoSize = true;

            numPollingUnitId.Location = new Point(150, 156);
            numPollingUnitId.Minimum = 1;
            numPollingUnitId.Maximum = 247;
            numPollingUnitId.Value = 1;

            btnPollingIniciar.Text = "▶ Iniciar";
            btnPollingIniciar.Location = new Point(20, 200);
            btnPollingIniciar.Size = new Size(95, 32);
            btnPollingIniciar.BackColor = Color.SeaGreen;
            btnPollingIniciar.ForeColor = Color.White;
            btnPollingIniciar.FlatStyle = FlatStyle.Flat;
            btnPollingIniciar.Click += btnPollingIniciar_Click;

            btnPollingDetener.Text = "⏹ Detener";
            btnPollingDetener.Location = new Point(125, 200);
            btnPollingDetener.Size = new Size(95, 32);
            btnPollingDetener.BackColor = Color.Firebrick;
            btnPollingDetener.ForeColor = Color.White;
            btnPollingDetener.FlatStyle = FlatStyle.Flat;
            btnPollingDetener.Click += btnPollingDetener_Click;

            btnPollingLimpiar.Text = "🗑 Limpiar log";
            btnPollingLimpiar.Location = new Point(230, 200);
            btnPollingLimpiar.Size = new Size(100, 32);
            btnPollingLimpiar.Click += btnPollingLimpiar_Click;

            lblPollingCiclosTitulo.Text = "Ciclos:";
            lblPollingCiclosTitulo.Location = new Point(20, 248);
            lblPollingCiclosTitulo.AutoSize = true;

            lblPollingCiclosValor.Text = "0";
            lblPollingCiclosValor.Location = new Point(72, 248);
            lblPollingCiclosValor.AutoSize = true;

            ConfigurarLog(rtbPollingLog);
            rtbPollingLog.Location = new Point(350, 16);
            rtbPollingLog.Size = new Size(600, 540);

            tabPolling.Controls.AddRange(new Control[]
            {
                lblPollingTipo,
                cmbPollingTipo,
                lblPollingDireccion,
                numPollingDireccion,
                lblPollingCantidad,
                numPollingCantidad,
                lblPollingIntervalo,
                numPollingIntervalo,
                lblPollingUnitId,
                numPollingUnitId,
                btnPollingIniciar,
                btnPollingDetener,
                btnPollingLimpiar,
                lblPollingCiclosTitulo,
                lblPollingCiclosValor,
                rtbPollingLog
            });
        }

        /// <summary>
        /// Configura propiedades base para controles de tipo log.
        /// </summary>
        /// <param name="log">Control de log a configurar.</param>
        private static void ConfigurarLog(RichTextBox log)
        {
            log.BackColor = Color.FromArgb(20, 20, 30);
            log.ForeColor = Color.Gainsboro;
            log.BorderStyle = BorderStyle.FixedSingle;
            log.ReadOnly = true;
            log.WordWrap = false;
            log.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
        }

        #endregion
    }
}
