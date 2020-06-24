
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

namespace TableDesignInfo
{
    public partial class LiveDataContext
    {
        #region Transaction

        partial void OnCreated()
        {
            this.CommandTimeout = Properties.Settings.Default.CommandTimeOut;
        }

        partial void InsertTableList(TableList instance)
        {
            base.ExecuteDynamicInsert(instance);
            using (var cmd = this.GetCommand())
            {
                string sql = this.UpdateTableComment(instance);
                cmd.CommandText = sql;
                this.ExecuteNonQuery(cmd);
            }

        }

        partial void InsertTableLayoutInfo(TableLayoutInfo instance)
        {
            base.ExecuteDynamicInsert(instance);
            using (var cmd = this.GetCommand())
            {
                string sql = this.UpdateTableLayoutComment(instance);
                cmd.CommandText = sql;
                this.ExecuteNonQuery(cmd);
            }
        }
        partial void UpdateTableList(TableList instance)
        {
            base.ExecuteDynamicUpdate(instance);
            using (var cmd = this.GetCommand())
            {
                string sql = this.UpdateTableComment(instance);
                cmd.CommandText = sql;
                this.ExecuteNonQuery(cmd);
            }
        }
        partial void UpdateTableLayoutInfo(TableLayoutInfo instance)
        {
            base.ExecuteDynamicUpdate(instance);
            using (var cmd = this.GetCommand())
            {
                string sql = this.UpdateTableLayoutComment(instance);
                cmd.CommandText = sql;
                this.ExecuteNonQuery(cmd);
            }
        }
        partial void DeleteTableList(TableList instance)
        {
            base.ExecuteDynamicDelete(instance);
            using (var cmd = this.GetCommand())
            {
                string sql = this.DeleteTableComment(instance);
                cmd.CommandText = sql;
                this.ExecuteNonQuery(cmd);
            }
        }
        partial void DeleteTableLayoutInfo(TableLayoutInfo instance)
        {
            base.ExecuteDynamicDelete(instance);
            using (var cmd = this.GetCommand())
            {
                string sql = this.DeleteTableLayoutComment(instance);
                cmd.CommandText = sql;
                this.ExecuteNonQuery(cmd);
            }
        }

        private string replaceGo(string sql)
        {
            Regex reg = new Regex(@"\s*GO\s*\r{0,1}\n");
            return reg.Replace(sql, "");
        }

        private string DeleteTableLayoutComment(TableLayoutInfo instance)
        {
            StringBuilder sb = new StringBuilder();
            //�J��������Drop
            string colTemp = Properties.Resources.DropColumnDescription;
            string commentTemp = Properties.Resources.DropColumnComment;
            //��\�����폜
            sb.AppendFormat(colTemp, instance.TableName, instance.ColumnName);
            sb.AppendLine();
            //�R�����g�폜
            sb.AppendFormat(commentTemp, instance.TableName, instance.ColumnName);
            sb.AppendLine();
            
            return replaceGo(sb.ToString());
        }

        private string DeleteTableComment(TableList instance)
        {
            StringBuilder sb = new StringBuilder();
            //�e�[�u������Drop
            string template = Properties.Resources.DropTableDescription;
            sb.AppendFormat(template, instance.TableName);
            sb.AppendLine();
            return replaceGo(sb.ToString());

        }


        private string UpdateTableComment(TableList instance)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DeleteTableComment(instance));
            //�e�[�u������
            string template = Properties.Resources.AddTableDescription;
            if (!string.IsNullOrWhiteSpace(instance.TableDisplayName))
            {
                sb.AppendFormat(template, instance.TableName, instance.TableDisplayName);
            }
            //�e�[�u���T�v
            if (!string.IsNullOrWhiteSpace(instance.Comment))
            {
                template = Properties.Resources.AddTableComment;
                sb.AppendFormat(template, instance.TableName, instance.Comment);
            }
            return replaceGo(sb.ToString());

        }

        private string UpdateTableLayoutComment(TableLayoutInfo instance)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.DeleteTableLayoutComment(instance));
            //�J��������
            string colTemp = Properties.Resources.AddColumnDescription;
            string commentTemp = Properties.Resources.AddColumnComment;
            if (!string.IsNullOrWhiteSpace(instance.ColumnDisplayName))
            {
                sb.AppendFormat(colTemp, instance.TableName, instance.ColumnName, instance.ColumnDisplayName);
            }
            if (!string.IsNullOrWhiteSpace(instance.Comment))
            {
                sb.AppendLine();
                sb.AppendFormat(commentTemp, instance.TableName, instance.ColumnName, instance.Comment);
            }
            return replaceGo(sb.ToString());

        }

        /// <summary>
        /// �f�[�^�x�[�X
        /// </summary>
        /// <returns></returns>
        public void BeginTransaction()
        {
            this.Transaction = this.Connection.BeginTransaction();
        }
        /// <summary>
        /// ���[���o�b�N
        /// </summary>
        /// <returns></returns>
        public void RollbackTransaction()
        {
            if (this.Transaction != null)
            {
                this.Transaction.Rollback();
                this.Transaction = null;
            }
        }

        /// <summary>
        /// �R�~�b�g(BeginTransactoin���g�p���Ă���ꍇ�̂�)
        /// </summary>
        /// <returns></returns>
        public void CommitTransaction()
        {
            if (this.Transaction == null) return;
            this.Transaction.Commit();
            this.Transaction = null;
        }

        #endregion

        public DbConnection GetConnection()
        {
            if (this.Connection.State == ConnectionState.Closed)
            {
                this.Connection.Open();
            }
            return this.Connection;
        }

        public DbCommand GetCommand()
        {
            DbCommand cmd = this.GetConnection().CreateCommand();
            if (this.Transaction != null)
            {
                cmd.Transaction = this.Transaction;
            }
            cmd.CommandTimeout = this.CommandTimeout;
            return cmd;
        }


        #region Execute

        /// <summary>
        /// SqlCommand�����s����
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(DbCommand cmd)
        {
            if (cmd.Connection == null)
            {
                cmd.Connection = this.GetConnection();
            }
            if (this.Transaction != null)
            {
                cmd.Transaction = this.Transaction;
            }
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// �N�G�������s���A���̃N�G�����Ԃ����ʃZ�b�g�̍ŏ��̍s�ɂ���ŏ��̗��Ԃ��܂��B 
        /// �c��̗�܂��͍s�͖�������܂��B
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public T ExecuteScalar<T>(DbCommand cmd)
        {
            if (cmd.Connection == null)
            {
                cmd.Connection = this.GetConnection();
            }
            if (this.Transaction != null)
            {
                cmd.Transaction = this.Transaction;
            }
            return (T)cmd.ExecuteScalar();
        }

        public DbDataReader ExecuteReader(DbCommand cmd)
        {
            DbDataReader reader;

            if (cmd.Connection == null)
            {
                cmd.Connection = this.GetConnection();
            }
            if (this.Transaction != null)
            {
                cmd.Transaction = this.Transaction;
            }
            reader = cmd.ExecuteReader();
            return reader;
        }

        public DataSet ExecuteResultSetForSql(DbCommand cmd)
        {
            DataSet dts;

            if (cmd.Connection == null)
            {
                cmd.Connection = this.GetConnection();
            }
            if (this.Transaction != null)
            {
                cmd.Transaction = this.Transaction;
            }
            dts = new DataSet();
            using (DbDataAdapter adapter = new SqlDataAdapter((SqlCommand)cmd))
            {
                adapter.Fill(dts);
            }
            return dts;
        }

        public DataSet ExecuteResultSetForSql(DbCommand cmd, bool IsFillSchema)
        {
            DataSet dts;

            if (cmd.Connection == null)
            {
                cmd.Connection = this.GetConnection();
            }
            if (this.Transaction != null)
            {
                cmd.Transaction = this.Transaction;
            }
            dts = new DataSet();
            using (DbDataAdapter adapter = new SqlDataAdapter((SqlCommand)cmd))
            {
                if (IsFillSchema)
                {
                    adapter.FillSchema(dts, SchemaType.Mapped);
                }
                adapter.Fill(dts);
            }
            return dts;
        }

        public DataTable ExecuteResultSetForSql(DbCommand cmd, string TableName, bool IsFillSchema)
        {
            return ExecuteResultSetForSql(cmd, new DataSet(), TableName, IsFillSchema);
        }

        public DataTable ExecuteResultSetForSql(DbCommand cmd, DataSet rDataSet, string TableName, bool IsFillSchema)
        {
            DataSet dts;

            if (cmd.Connection == null)
            {
                cmd.Connection = this.GetConnection();
            }
            if (this.Transaction != null)
            {
                cmd.Transaction = this.Transaction;
            }
            dts = new DataSet();
            using (DbDataAdapter adapter = new SqlDataAdapter((SqlCommand)cmd))
            {
                if (IsFillSchema)
                {
                    adapter.FillSchema(rDataSet, SchemaType.Mapped, TableName);
                }
                adapter.Fill(rDataSet, TableName);
                if (rDataSet.Tables.Contains(TableName))
                {
                    return rDataSet.Tables[TableName];
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion



    }
    public partial class TableLayoutInfo
    {
        private bool _isChanged = false;
        /// <summary>
        /// �v���p�e�B�ǉ�
        /// </summary>
        public string SqlDataType
        {
            get
            {
                if (string.IsNullOrEmpty(this.DataType))
                {
                    return string.Empty;
                }
                string sql = this.DataType;
                string lengthStr = "";
                switch (this.DataType.ToLower())
                {
                    case "varchar":
                    case "nvarchar":
                    case "varbinary":
                        if (this.Length > 0)
                        {
                            lengthStr = "(" + this.Length + ")";
                        }
                        else
                        {
                            lengthStr = "( max )";
                        }
                        break;
                    case "char":
                    case "nchar":
                    case "binary":
                        if (this.Length > 0)
                        {
                            lengthStr = "(" + this.Length + ")";
                        }
                        break;
                    case "decimal":
                    case "numeric":
                        if (this.Scale.HasValue && this.Precision.HasValue && this.Scale > 0 && this.Precision > 0)
                        {
                            lengthStr = "(" + this.Precision.Value + " , " + this.Scale.Value + ")";
                        }
                        else
                        {
                            int precision = this.Precision.HasValue ? this.Precision.Value : this.Length.HasValue ? this.Length.Value : 0;
                            if (precision > 0)
                            {
                                lengthStr = "(" + this.Precision + ")";
                            }
                        }
                        break;
                    default:
                        break;
                }
                sql = sql + lengthStr;
                return sql;
            }
        }
        /// <summary>
        /// �v���p�e�B�ǉ�
        /// </summary>
        public string DataLengthDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(this.DataType))
                {
                    return string.Empty;
                }
                string lengthStr = "";
                switch (this.DataType.ToLower())
                {
                    case "varchar":
                    case "nvarchar":
                    case "varbinary":
                        if (this.Length > 0)
                        {
                            lengthStr = Convert.ToString(this.Length);
                        }
                        else
                        {
                            lengthStr = "max";
                        }
                        break;
                    case "char":
                    case "nchar":
                    case "binary":
                        if (this.Length > 0)
                        {
                            lengthStr = Convert.ToString(this.Length);
                        }
                        break;
                    case "decimal":
                    case "numeric":
                        if (this.Scale.HasValue && this.Precision.HasValue && this.Scale > 0 && this.Precision > 0)
                        {
                            lengthStr = this.Precision.Value + " , " + this.Scale.Value;
                        }
                        else
                        {
                            int precision = this.Precision.HasValue ? this.Precision.Value : this.Length.HasValue ? this.Length.Value : 0;
                            if (precision > 0)
                            {
                                lengthStr = Convert.ToString(precision);
                            }
                        }
                        break;
                    default:
                        break;
                }
                return lengthStr;
            }
        }


        public string GetDataTypeForJava()
        {
            if (string.IsNullOrEmpty(this.DataType))
            {
                return "Object";
            }
            switch (this.DataType.ToLower())
            {
                case "varchar":
                case "nvarchar":
                case "char":
                case "nchar":
                    return "String";
                case "binary":
                case "varbinary":
                    return "byte[]";
                case "float":
                case "decimal":
                    return "Float";
                case "int":
                    return "Integer";
                case "numeric":
                    if (this.Scale.HasValue && this.Scale > 0)
                    {
                        return "Float";
                    }
                    else
                    {
                        return "Integer";
                    }
                case "date":
                    return "Date";
                case "datetime":
                    return "Timestamp";
                default:
                    return "Object";
            }
        }

        /// <summary>
        /// �f�[�^�ύX�����邩�ǂ�����\��
        /// </summary>
        public bool IsChanged
        {
            get
            {
                return _isChanged;
            }
        }
        /// <summary>
        /// partial���\�b�h����������
        /// </summary>
        partial void OnCreated()
        {
            this.PropertyChanged += OnPropertyChanged;
        }


        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this._isChanged = true;
        }


    }
    public partial class TableList
    {
        private bool _isChanged = false;
        private long? _dataCount = null;

        /// <summary>
        /// �f�[�^�ύX�����邩�ǂ�����\��
        /// </summary>
        public bool IsChanged
        {
            get
            {
                return _isChanged;
            }
        }

        public long DataCount
        {
            get
            {
                if (!this._dataCount.HasValue)
                {
                    this._dataCount = Controls.LinqSqlHelp.GetTableDataCount(this.TableName);
                }
                return this._dataCount.Value;
            }
        }

        /// <summary>
        /// partial���\�b�h����������
        /// </summary>
        partial void OnCreated()
        {
            this.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this._isChanged = true;
        }
    }
}