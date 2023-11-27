using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.IO.Pipes;
using System;
using System.Collections.Generic;

using System.Linq;

namespace DapperORACLE.Repositories
{
    public class OracleDynamicParameters : SqlMapper.IDynamicParameters
    {
        public class OracleParameterInfo
        {
            public string Name { get; set; }

            public object Value { get; set; }

            public ParameterDirection ParameterDirection { get; set; }

            public OracleMappingType? DbType { get; set; }

            public int? Size { get; set; }

            public bool? IsNullable { get; set; }

            public byte? Precision { get; set; }

            public byte? Scale { get; set; }

            public string SourceColumn { get; set; } = string.Empty;


            public DataRowVersion SourceVersion { get; set; }

            public OracleMappingCollectionType CollectionType { get; set; }

            public int[] ArrayBindSize { get; set; }

            public OracleParameterMappingStatus Status { get; set; }

            public IDbDataParameter AttachedParam { get; set; }
        }

        private List<object> templates;

        private static Dictionary<SqlMapper.Identity, Action<IDbCommand, object>> ParamReaderCache { get; } = new Dictionary<SqlMapper.Identity, Action<IDbCommand, object>>();


        private Dictionary<string, OracleParameterInfo> Parameters { get; } = new Dictionary<string, OracleParameterInfo>();


        public int ArrayBindCount { get; set; }

        public int InitialLOBFetchSize { get; set; }

        public bool BindByName { get; set; }

        public IEnumerable<string> ParameterNames => Parameters.Select((KeyValuePair<string, OracleParameterInfo> p) => p.Key);

        public OracleDynamicParameters()
        {
        }

        public OracleDynamicParameters(object template)
        {
            AddDynamicParams(template);
        }

        public void AddDynamicParams(dynamic param)
        {
            object obj;
            if ((obj = param) == null)
            {
                return;
            }

            OracleDynamicParameters oracleDynamicParameters;
            if ((oracleDynamicParameters = obj as OracleDynamicParameters) == null)
            {
                IEnumerable<KeyValuePair<string, object>> enumerable;
                if ((enumerable = obj as IEnumerable<KeyValuePair<string, object>>) == null)
                {
                    templates = templates ?? new List<object>();
                    templates.Add(obj);
                    return;
                }

                foreach (KeyValuePair<string, object> item in enumerable)
                {
                    Add(item.Key, item.Value);
                }

                return;
            }

            if (oracleDynamicParameters.Parameters != null)
            {
                foreach (KeyValuePair<string, OracleParameterInfo> parameter in oracleDynamicParameters.Parameters)
                {
                    Parameters.Add(parameter.Key, parameter.Value);
                }
            }

            if (oracleDynamicParameters.templates == null)
            {
                return;
            }

            templates = templates ?? new List<object>();
            foreach (object template in oracleDynamicParameters.templates)
            {
                templates.Add(template);
            }
        }

        public void Add(string name, object value = null, OracleMappingType? dbType = null, ParameterDirection? direction = null, int? size = null, bool? isNullable = null, byte? precision = null, byte? scale = null, string sourceColumn = null, DataRowVersion? sourceVersion = null, OracleMappingCollectionType? collectionType = null, int[] arrayBindSize = null)
        {
            Parameters[Clean(name)] = new OracleParameterInfo
            {
                Name = Clean(name),
                Value = value,
                ParameterDirection = (direction ?? ParameterDirection.Input),
                DbType = dbType,
                Size = size,
                IsNullable = (isNullable ?? false),
                Precision = precision,
                Scale = scale,
                SourceColumn = sourceColumn,
                SourceVersion = (sourceVersion ?? DataRowVersion.Current),
                CollectionType = (collectionType ?? OracleMappingCollectionType.None),
                ArrayBindSize = arrayBindSize
            };
        }

        public T Get<T>(string name)
        {
            return OracleValueConverter.Convert<T>(Parameters[Clean(name)].AttachedParam.Value);
        }

        public OracleParameterInfo GetParameter(string name)
        {
            return OracleMethodHelper.GetParameterInfo(Parameters[Clean(name)].AttachedParam);
        }

        void SqlMapper.IDynamicParameters.AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            AddParameters(command, identity);
        }

        protected virtual void AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            if (ArrayBindCount > 0)
            {
                OracleMethodHelper.SetArrayBindCount(command, ArrayBindCount);
            }

            if (InitialLOBFetchSize > 0)
            {
                OracleMethodHelper.SetInitialLOBFetchSize(command, InitialLOBFetchSize);
            }

            if (BindByName)
            {
                OracleMethodHelper.SetBindByName(command, BindByName);
            }

            if (templates != null)
            {
                foreach (object template in templates)
                {
                    SqlMapper.Identity identity2 = identity.ForDynamicParameters(template.GetType());
                    Action<IDbCommand, object> value;
                    lock (ParamReaderCache)
                    {
                        if (!ParamReaderCache.TryGetValue(identity2, out value))
                        {
                            value = SqlMapper.CreateParamInfoGenerator(identity2, checkForDuplicates: false, removeUnused: false);
                            ParamReaderCache[identity2] = value;
                        }
                    }

                    value(command, template);
                }
            }

            foreach (OracleParameterInfo value3 in Parameters.Values)
            {
                string parameterName = Clean(value3.Name);
                bool num = !command.Parameters.Contains(parameterName);
                IDbDataParameter dbDataParameter;
                if (num)
                {
                    dbDataParameter = command.CreateParameter();
                    dbDataParameter.ParameterName = parameterName;
                }
                else
                {
                    dbDataParameter = (IDbDataParameter)command.Parameters[parameterName];
                }

                OracleMethodHelper.SetOracleParameters(dbDataParameter, value3);
                object value2 = value3.Value;
                dbDataParameter.Value = value2 ?? DBNull.Value;
                dbDataParameter.Direction = value3.ParameterDirection;
                string obj = value2 as string;
                if (obj != null && obj.Length <= 4000)
                {
                    dbDataParameter.Size = 4000;
                }

                if (value3.Size.HasValue)
                {
                    dbDataParameter.Size = value3.Size.Value;
                }

                if (num)
                {
                    command.Parameters.Add(dbDataParameter);
                    value3.AttachedParam = dbDataParameter;
                }
            }
        }

        private static string Clean(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                char c = name[0];
                if (c == ':' || c == '?' || c == '@')
                {
                    return name.Substring(1);
                }
            }

            return name;
        }
    }
}
