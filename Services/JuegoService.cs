using CienEstudiantesDijeron.Models;
using System.Timers;

namespace CienEstudiantesDijeron
{
public class JuegoService
{
    //Pregunta Actual
    public string PreguntaActual { get; private set; } = "";
    //Id de Pregunta Actual
    public int PreguntaActualId { get; private set; } = 0;
    //Verificador de Pregunta Revelada
    public bool PreguntaRevelada { get; private set; } = false; 
    //Lista de Respuestas Actuales
    public List<Respuesta> RespuestasActuales {get; private set; } = new List<Respuesta>();
    //Lista de Respuestas Reveladas
    public List<string> RespuestasReveladas { get; set; } = new();

    // El evento que notificará a las pantallas cuando algo cambia
    public event Action? OnChange;

    //Nombres de equipo
    public string Equipo1 { get; set; } = "Equipo 1";
    public string Equipo2 { get; set; } = "Equipo 2";

    //Puntos de equipos
    public int puntosEquipo1 {get; set; } = 0;
    public int puntosEquipo2 {get; set; } = 0;

    //Puntos de ronda
    public int puntosPartida {get; set; } = 0;

    //Verificador si se asignaron puntos
    public bool puntosAsignadosEnRonda { get; private set; } = false;

    //Valor de multiplicador, por defecto es 1
    public int Multiplicador { get; private set; } = 1; 
    //Verificador si la partida termino
    public bool PartidaTerminada { get; private set; } = false;
    // Lista para guardar el historial de asignacion de puntos
    public List<RegistroPuntos> HistorialPuntos { get; private set; } = new();
    //Errores de los equipos
    public int ErroresEquipo1 { get; private set; } = 0;
    public int ErroresEquipo2 { get; private set; } = 0;
    //Verificador para el efecto de la X
    public bool MostrarX { get; private set; } = false; 

    //Timer
    public int TiempoRestante { get; private set; } = 10;
    public bool TimerActivo { get; private set; } = false;
    public string? EquipoConTimer { get; private set; }
    private System.Timers.Timer? _timer;
    
    //Metodo para actualizar el juego
    public void ActualizarJuego(string pregunta, int preguntaId, List<Respuesta> respuestas)
    {
        PreguntaActual = pregunta;
        PreguntaActualId = preguntaId;
        // Ordenamos por cantidad descendente las respuestas
        RespuestasActuales = respuestas.OrderByDescending(r => r.res_cantidad).ToList();
        RespuestasReveladas.Clear(); 
        PreguntaRevelada = false; 
        puntosPartida = 0;
        puntosAsignadosEnRonda = false;
        Multiplicador = 1;
        NotifyStateChanged();
    }

    //Metodo para revelar pregunta
    public void RevelarPregunta()
    {
        PreguntaRevelada = true;
        NotifyStateChanged();
    }

    //Metodo para revelar respuesta
    public void RevelarRespuestaPorTexto(string respuesta)
    {
        if (!RespuestasReveladas.Contains(respuesta))
        {
            if (TimerActivo) CancelarTimer();

            RespuestasReveladas.Add(respuesta);
            AumentarPuntosRonda(respuesta);
            NotifyStateChanged();
        }
    }

    //Metodo para aumentar puntos a la ronda
    public void AumentarPuntosRonda(string respuesta)
    {
        if (puntosAsignadosEnRonda) return;

        foreach (Respuesta r in RespuestasActuales)
        {
            if (r.res_respuesta.Equals(respuesta))
            {
                puntosPartida += (r.res_cantidad * Multiplicador);
                break;
            }
        }
    }

    //Metodo para Actualizar nombres
    public void ActualizarNombres(string nombre1, string nombre2)
    {
        Equipo1 = nombre1;
        Equipo2 = nombre2;
        NotifyStateChanged();
    }

    //Metodo para limpiar pregunta
    public void LimpiarPregunta()
    {
        CancelarTimer();

        PreguntaActual = "";
        PreguntaActualId = 0;
        PreguntaRevelada = false;

        RespuestasActuales.Clear();
        RespuestasReveladas.Clear();

        puntosPartida = 0;
        puntosAsignadosEnRonda = false;
        Multiplicador = 1;

        ErroresEquipo1 = 0;
        ErroresEquipo2 = 0;
        MostrarX = false;
        NotifyStateChanged();
    }

    //Metodo para asignar puntos a un equipo
    public void AsignarPuntosEquipo(string equipo, int numRonda)
    {
        if (!puntosAsignadosEnRonda && puntosPartida > 0)
        {
            int puntosAEntregar = puntosPartida;

            if (equipo == Equipo1) puntosEquipo1 += puntosAEntregar;
            else if (equipo == Equipo2) puntosEquipo2 += puntosAEntregar;

            // Guardar en el historial
            HistorialPuntos.Add(new RegistroPuntos 
            { 
                Ronda = numRonda, 
                Equipo = equipo, 
                Puntos = puntosAEntregar 
            });

            puntosAsignadosEnRonda = true;
            puntosPartida = 0;
            NotifyStateChanged();
        }
    }

    // Método para cambiar el multiplicador desde el panel de control
    public void SetMultiplicador(int factor)
    {
        if (!PreguntaRevelada)
        {
            Multiplicador = factor;
            NotifyStateChanged();
        }
    }

    //Metodo para reinicar todo
    public void ReiniciarTodo()
    {
        HistorialPuntos.Clear();
        PartidaTerminada = false;
        puntosEquipo1 = 0;
        puntosEquipo2 = 0;
        puntosPartida = 0;
        Equipo1 = "Equipo 1";
        Equipo2 = "Equipo 2";
        LimpiarPregunta(); 
    }

    //Metodo para obtener los puntos de las respuestas multiplicados
    public int ObtenerPuntosMultiplicados(int cantidadOriginal)
    {
        return cantidadOriginal * Multiplicador;
    }

    //Metodo para finalizar la partida
    public void FinalizarPartida()
    {
        CancelarTimer();
        
        PartidaTerminada = true;
        NotifyStateChanged();
    }

    //Metodo para agregar error a un equipo
    public void AgregarError(string equipo)
    {
        CancelarTimer();
        if (equipo == Equipo1 && ErroresEquipo1 < 3) ErroresEquipo1++;
        else if (equipo == Equipo2 && ErroresEquipo2 < 3) ErroresEquipo2++;
        
        ActivarEfectoX();
    }

    //Metodo para activar el efecto de X
    private async void ActivarEfectoX()
    {
        MostrarX = true;
        NotifyStateChanged();
        await Task.Delay(2000); 
        MostrarX = false;
        NotifyStateChanged();
    }

    public void IniciarTimer(string equipo)
    {
        CancelarTimer(); // Por seguridad, limpiar anterior
        EquipoConTimer = equipo;
        TiempoRestante = 10;
        TimerActivo = true;
        
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += async (s, e) => {
            if (TiempoRestante > 0)
            {
                TiempoRestante--;
                NotifyStateChanged();
            }
            else
            {
                _timer.Stop();
                TimerActivo = false;
                // Si el tiempo llega a 0, marcar error automáticamente
                if (!string.IsNullOrEmpty(EquipoConTimer))
                {
                    AgregarError(EquipoConTimer);
                }
                EquipoConTimer = null;
                NotifyStateChanged();
            }
        };
        _timer.Start();
        NotifyStateChanged();
    }

    public void CancelarTimer()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
        }
        TimerActivo = false;
        EquipoConTimer = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    //Clase de RegistroPuntos para el historial
    public class RegistroPuntos 
    {
        public int Ronda { get; set; }
        public string Equipo { get; set; } = "";
        public int Puntos { get; set; }
    }
}
}
