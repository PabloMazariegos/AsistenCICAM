using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using bdCICAM;
using System.Web.Http.Cors;

namespace apiCICAM.Controllers
{
    [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
    public class EmpleadosController : ApiController
    {
        //METODO PARA LISTAR TODOS LOS EMPLEADOS
        public IEnumerable<EMPLEADO> Get()
        {
            using (CICAMEntities entities = new CICAMEntities())
            {
                return entities.EMPLEADO.ToList();
            }
        }

        //METODO PARA BUSCAR UN EMPLEADO POR MEDIO DE DIFERENTES PARAMETROS
        [Route("CICAM/Empleados/Find/")]
        public IEnumerable<EMPLEADO> Get([FromUri] EMPLEADO emp)
        {
            IList<EMPLEADO> emps = null;
            using (CICAMEntities entities = new CICAMEntities())
            {
                if (emp.ID != 0)
                {
                    emps = entities.EMPLEADO.Where(e => e.ID == emp.ID).ToList();
                }
                else if (emp.NOMBRE != null && emp.NOMBRE != "")
                {
                    emps = entities.EMPLEADO.Where(e => e.NOMBRE.Contains(emp.NOMBRE)).ToList();

                } else if (emp.PUESTO != null && emp.PUESTO != "")
                {
                    emps = entities.EMPLEADO.Where(e => e.PUESTO.Contains(emp.PUESTO)).ToList();

                } else if (emp.APELLIDO != null && emp.APELLIDO != "")
                {
                    emps = entities.EMPLEADO.Where(e => e.APELLIDO.Contains(emp.APELLIDO)).ToList();
                }


                return emps;
            }
        }

        public IHttpActionResult post([FromUri] EMPLEADO emp){
            using (CICAMEntities entities = new CICAMEntities())
            {
                entities.EMPLEADO.Add(emp);
                entities.SaveChanges();
            }

            return Ok(new { StatusCode = 200 , message = "Empleado grabado exitosamente"});
        }


        [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
        public IHttpActionResult Delete([FromUri] EMPLEADO emp)
        {
            using (CICAMEntities entities = new CICAMEntities())
            {
                EMPLEADO empAnt = new EMPLEADO();
                empAnt = entities.EMPLEADO.Where(iid => iid.ID == emp.ID).FirstOrDefault();
                entities.Entry(empAnt).State = System.Data.Entity.EntityState.Deleted;
                entities.SaveChanges();
            }

            return Ok(new { StatusCode = 200, message = "Empleado eliminado exitosamente" });
        }
    }
}
