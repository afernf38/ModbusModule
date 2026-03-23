using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace UNE135411
{
    public abstract class clsConexion
    {
        protected List<clsElemento> listaElementosGeneral = new List<clsElemento>();
        protected bool conectado = false;
        public string nombreConexion { get; set; } = string.Empty;
        public string codElemento { get; set; } = string.Empty;

        public event System.Action<bool>? CambioEstadoConexion;
        public event System.Action<JObject, string>? MensajeEntrante;

        protected void OnCambioEstadoConexion(bool estado) => CambioEstadoConexion?.Invoke(estado);
        protected void OnMensajeEntrante(JObject mensaje, string codElemento) => MensajeEntrante?.Invoke(mensaje, codElemento);

        public abstract bool Conectar(string _cadenaConexion, List<clsElemento> _listaElementosGeneral, out Dictionary<string, string> _listaErrores);
        public abstract bool Desconectar(out Dictionary<string, string> _listaErrores);
        public abstract bool Escribir(object _comodin, out Dictionary<string, string> _listaErrores);
        public abstract bool Leer(object _comodin, out object respuesta, out Dictionary<string, string> _listaErrores);
        public abstract Dictionary<string, string> gestionErrores();

        public bool Conectado => conectado;
    }

    public class clsElemento
    {
        public string CodElemento { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}
