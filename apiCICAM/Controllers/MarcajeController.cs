using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web.Http;
using bdCICAM;
using libzkfpcsharp;
using System.Web.Http.Cors;

namespace apiCICAM.Controllers
{
    [EnableCors(origins: "http://localhost:3000", headers: "*", methods: "*")]
    public class MarcajeController : ApiController
    {
        //variables para inicializar lector
        IntPtr mDevHandle = IntPtr.Zero;
        IntPtr mDBHandle = IntPtr.Zero;

        //variables para guardar datos de huella
        byte[] paramValue = new byte[4];
        int mfpWidth = 0;
        int mfpHeight = 0;
        int mfpDpi = 0;
        int size = 4;
        int ret;
        int cbCapTmp = 2048;
        int conteo = 0;
        int cbRegTmp = 0;
        int fid = 0;
        int score = 0;

        //byte de huella
        byte[] FPBuffer;
        byte[][] RegTmps = new byte[3][];
        byte[] CapTmp = new byte[2048];
        byte[] RegTmp = new byte[2048];


        public string POST([FromUri] EMPLEADO emp)
        {
            //INICIANDO DISPOSITIVO
            zkfp2.Init();
            mDevHandle = zkfp2.OpenDevice(0);

            //CREANDO VECTOR QUE GUARDA HUELLA
            zkfp2.GetParameters(mDevHandle, 1, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref mfpWidth);
            size = 4;
            zkfp2.GetParameters(mDevHandle, 2, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref mfpHeight);


            //CREANDO BYTE PARA HUELLA
            FPBuffer = new byte[mfpWidth * mfpHeight];


            //CREANDO VECTOR PARA DENSIDAD DE HUELLA
            size = 4;
            zkfp2.GetParameters(mDevHandle, 3, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref mfpDpi);


            //INCIANDO BASE DE DATOS DEL LECTOR
            mDBHandle = zkfp2.DBInit();


            //LIMPIANDO BYTES DE LECTURA
            for (int i = 0; i < 3; i++)
            {
                RegTmps[i] = new byte[2048];
            }

            ret = zkfp.ZKFP_ERR_FAIL;
            cbCapTmp = 2048;
            while (conteo < 3)
            {
                ret = zkfp2.AcquireFingerprint(mDevHandle, FPBuffer, CapTmp, ref cbCapTmp);
                if (ret == zkfp.ZKFP_ERR_OK)
                {
                    Array.Copy(CapTmp, RegTmps[conteo], cbCapTmp);
                    String strBase64 = zkfp2.BlobToBase64(CapTmp, cbCapTmp);
                    byte[] blob = zkfp2.Base64ToBlob(strBase64);
                    conteo++;
                }
            }

            conteo = 0;
            zkfp2.DBMerge(mDBHandle, RegTmps[0], RegTmps[1], RegTmps[2], RegTmp, ref cbRegTmp);
            String strShow = zkfp2.BlobToBase64(RegTmp, cbCapTmp);
            
            using (CICAMEntities entities = new CICAMEntities())
            {
                var empAnt = entities.EMPLEADO.Find(emp.ID);
                empAnt.HUELLA = strShow;
                entities.Entry(empAnt).State = System.Data.Entity.EntityState.Modified;
                entities.SaveChanges();
            }
            zkfp2.DBAdd(mDBHandle, emp.ID, RegTmp);
            return strShow;

        }

        public IHttpActionResult GET([FromUri] MARK mark)
        {
            EMPLEADO empAnt = new EMPLEADO();

            //INICIANDO DISPOSITIVO
            zkfp2.Init();
            mDevHandle = zkfp2.OpenDevice(0);

            //CREANDO VECTOR QUE GUARDA HUELLA
            zkfp2.GetParameters(mDevHandle, 1, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref mfpWidth);
            size = 4;
            zkfp2.GetParameters(mDevHandle, 2, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref mfpHeight);


            //CREANDO BYTE PARA HUELLA
            FPBuffer = new byte[mfpWidth * mfpHeight];


            //CREANDO VECTOR PARA DENSIDAD DE HUELLA
            size = 4;
            zkfp2.GetParameters(mDevHandle, 3, paramValue, ref size);
            zkfp2.ByteArray2Int(paramValue, ref mfpDpi);


            //INCIANDO BASE DE DATOS DEL LECTOR
            mDBHandle = zkfp2.DBInit();


            ret = zkfp.ZKFP_ERR_FAIL;
            cbCapTmp = 2048;
            while (ret != zkfp.ZKFP_ERR_OK)
            {
                ret = zkfp2.AcquireFingerprint(mDevHandle, FPBuffer, CapTmp, ref cbCapTmp);
            }

            ret = zkfp2.DBIdentify(mDBHandle, CapTmp, ref fid, ref score);
                
            if (zkfp.ZKFP_ERR_OK == ret)
            {

                using (CICAMEntities entities = new CICAMEntities())
                {
                    empAnt = entities.EMPLEADO.Find(fid);
                    mark.EMPE_IDENTITY = empAnt.ID;
                    mark.FECHA = DateTime.Now.Date;
                    mark.HORA = DateTime.Now.TimeOfDay;
                    entities.MARK.Add(mark);
                    entities.SaveChanges();
                }
                 
                    return Ok(new { status = "Registro encontrado", ID = fid });
            }
            else
            {
                return Ok(new { status = "Registro no encontrado" });
            }

        }
    }
}
