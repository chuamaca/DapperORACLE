using Dapper;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Runtime.ConstrainedExecution;
using System.Xml.Linq;
using static Dapper.SqlMapper;

namespace DapperORACLE.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {

        IConfiguration configuration;
        public EmployeeRepository(IConfiguration _configuration)
        {
            configuration = _configuration;
        }



        public IDbConnection GetConnection()
        {
            var connectionString = configuration.GetSection("ConnectionStrings").GetSection("DefaultConnectionOracle").Value;
            var conn = new OracleConnection(connectionString);
            return conn;
        }


        public object GetCompaniaListar()
        {

            object result = null;

            IEnumerable<dynamic> results = null;

            try
            {


                //var dyParam = new DynamicParameters();



                //dyParam.Add("nCOD_ERROR", null, dbType: DbType.Int32, direction: ParameterDirection.Output);
                //dyParam.Add("sDSC_ERROR",  null,dbType: DbType.String, direction: ParameterDirection.Output);
                //dyParam.Add("cLIST_COMPANIAS", null, dbType: DbType.Object, direction: ParameterDirection.Output);
                //dyParam.Add("cLIST_ERROR", null, dbType: DbType.Object, direction: ParameterDirection.Output);

                //   oraDyParam.Add(name: "cLIST_COMP", OracleDbType.RefCursor, ParameterDirection.Output);

                DynamicParameters dynParam = new DynamicParameters();



                var oraDyParam = new OracleDynamicParameters();
                oraDyParam.Add(name:"nCOD_ERROR", dbType: OracleMappingType.Int32, direction: ParameterDirection.Output);
                oraDyParam.Add(name:"sDSC_ERROR", dbType: OracleMappingType.Varchar2, direction: ParameterDirection.Output, size:4000);
                oraDyParam.Add(name: "cLIST_COMPANIAS", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.Output);
                oraDyParam.Add(name:"cLIST_ERROR", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.Output);

                var conn = this.GetConnection();
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                if (conn.State == ConnectionState.Open)
                {
                    var query = "PK_LOGUEO.SP_COMPANIA_SEL";
                    var reader = conn.Query<dynamic>(query, param: oraDyParam, commandType: CommandType.StoredProcedure);

                    var Juegos = oraDyParam.GetParameter("sDSC_ERROR");


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return results;

        }


   
    }
}
