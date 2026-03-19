using CienEstudiantesDijeron.Data;
using CienEstudiantesDijeron.Models;
using CsvHelper;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace CienEstudiantesDijeron.Services
{
    public class ImportadorService
    {
        private readonly ApplicationDbContext _context;

        public ImportadorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ImportarPreguntasCsv(Stream fileStream)
        {
            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var registros = new List<dynamic>();
            await foreach (var registro in csv.GetRecordsAsync<dynamic>())
            {
                registros.Add(registro);
            }

            var grupos = registros.GroupBy(f => f.Pregunta);

            foreach (var grupo in grupos)
            {
                var nuevaPregunta = new Pregunta { pr_pregunta = grupo.Key.ToString() };
                _context.Preguntas.Add(nuevaPregunta);
                
                await _context.SaveChangesAsync(); 

                foreach (var fila in grupo)
                {
                    var nuevaRespuesta = new Respuesta
                    {
                        pr_id = nuevaPregunta.pr_id,
                        res_respuesta = fila.Respuesta.ToString(),
                        res_cantidad = int.TryParse(fila.Puntos.ToString(), out int p) ? p : 0
                    };
                    _context.Respuestas.Add(nuevaRespuesta);
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}