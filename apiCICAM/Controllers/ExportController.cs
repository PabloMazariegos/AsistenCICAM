using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using bdCICAM;

namespace apiCICAM.Controllers
{
    public class ExportController : ApiController
    {
        string query = @"SELECT 
	                    CAST(HORARIOS.FECHA AS varchar), 
	                    CAST(EMPLEADO.ID AS varchar) as [CODIGO],
	                    EMPLEADO.PUESTO,
	                    EMPLEADO.NOMBRE, 
	                    EMPLEADO.APELLIDO,
	                    CAST(HORARIOS.ENTRADA AS varchar), 
	                    CAST(HORARIOS.SALIDA AS varchar) 
                    FROM (
	                    SELECT ENTRADA.EMPE_IDENTITY, ENTRADA.FECHA, CONVERT(VARCHAR,ENTRADA.HORA,22)[ENTRADA], CONVERT(VARCHAR,SALIDA.HORA,22)[SALIDA] FROM (
		                    SELECT 
			                    MARK_IDENTITY, 
			                    EMPE_IDENTITY, 
			                    FECHA, 
			                    HORA 
		                    FROM MARK 
		                    WHERE MARK_IDENTITY IN (
			                    SELECT 
				                    MIN(MARK_IDENTITY) MINIMO 
			                    FROM MARK
			                    GROUP BY EMPE_IDENTITY, FECHA
		                    )
	                    )AS ENTRADA
	                    JOIN(
		                    SELECT 
			                    MARK_IDENTITY, 
			                    EMPE_IDENTITY, 
			                    FECHA, 
			                    HORA 
		                    FROM MARK 
		                    WHERE MARK_IDENTITY IN (
			                    SELECT 
				                    MAX(MARK_IDENTITY) MINIMO 
			                    FROM MARK
			                    GROUP BY EMPE_IDENTITY, FECHA
		                    )
	                    )AS SALIDA
	                    ON(
		                    SALIDA.EMPE_IDENTITY = ENTRADA.EMPE_IDENTITY
		                    AND SALIDA.FECHA = ENTRADA.FECHA
	                    )
                    )AS HORARIOS
                    JOIN(
	                    SELECT * FROM EMPLEADO
                    )AS EMPLEADO
                    ON(HORARIOS.EMPE_IDENTITY = EMPLEADO.ID)";

        // GET: api/Prueba/5
        public List<DataExportTable> Get()
        {
            var Result = new List<DataExportTable>();
            string cnxString = @"data source=DESKTOP-ISBI9FJ;initial catalog=bd-cicam;integrated security=True;";
            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();
                using(SqlCommand cmd = new SqlCommand(query, cnx))
                {
                    using(SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {

                            Result.Add(new DataExportTable()
                            {
                                FECHA       =   (string)rdr[0],
                                CODIGO      =   (string)rdr[1],
                                PUESTO      =   (string)rdr[2],
                                NOMBRE      =   (string)rdr[3],
                                APELLIDO    =   (string)rdr[4],
                                ENTRADA     =   (string)rdr[5],
                                SALIDA      =   (string)rdr[6],
                            });
                        }
                    }
                }
            }
            return Result;
    
        }

        public class DataExportTable
        {
            public string FECHA { get; set; }
            public string CODIGO { get; set; }
            public string PUESTO { get; set; }
            public string NOMBRE { get; set; }
            public string APELLIDO { get; set; }
            public string ENTRADA { get; set; }
            public string SALIDA { get; set; }
        }

    }
}
