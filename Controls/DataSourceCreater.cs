using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableDesignInfo.Entity;

namespace TableDesignInfo.Controls
{
    public class DataSourceCreater
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public void Create(SourceGenerateInfo createInfo)
        {

            var tempinfo = createInfo.TemplateInfo;

            foreach (string tableName in createInfo.TableNames)
            {
                TableList table = LinqSqlHelp.GetTable(tableName);
                if (table != null)
                {
                    string code = this.CreateSourceCode(tempinfo, table,createInfo.SavePath);
                    string fileName = tempinfo.FileName.Replace("{@TableNameCamelU}", this.GetCamelName(table.TableName, true));

                    string filePath = System.IO.Path.Combine(createInfo.SavePath, fileName);
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath, false, Encoding.UTF8))
                    {
                        writer.Write(code);
                    }
                }
            }

        }

        private string GetCamelName(string tableName, bool UpperBegin)
        {
            string[] slices = tableName.Split('_');
            string result = "";
            for (int i = 0; i < slices.Length; i++)
            {
                string slice = slices[i];
                if (i == 0)
                {

                    result += getCamelWord(slice, UpperBegin);
                }
                else
                {
                    result += getCamelWord(slice,true);
                }
            }
            return result;
        }

        private string getCamelWord(string name, bool UpperBegin)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            string beginChar = UpperBegin ? name.Substring(0,1).ToUpper() : name.Substring(0,1).ToLower();
            if (name.Length > 1)
            {
                return beginChar + name.Substring(1).ToLower();
            }
            else
            {
                return beginChar;
            }
        }

        /// <summary>
        /// クラスソースを作成する
        /// </summary>
        /// <param name="info"></param>
        /// <param name="table"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>

        private string CreateSourceCode(SourceGenerateTemplate.TemplatesRow info, TableList table,string filePath)
        {
            string folder = System.IO.Path.GetFileName(filePath).ToLower();
            //Create Class
            string classTemp= info.ClassTemplace;
            Dictionary<string, string> replaceKeys = new Dictionary<string, string>();
            var columns = LinqSqlHelp.GetTableLayout(table.TableName);
            replaceKeys.Add("@Folder", folder);
            replaceKeys.Add("@TableNameCamelU", this.GetCamelName(table.TableName,true));
            replaceKeys.Add("@TableNameCamelL", this.GetCamelName(table.TableName, false));
            replaceKeys.Add("@TableName", table.TableName);
            replaceKeys.Add("@TableDisplayName", table.TableDisplayName);
            replaceKeys.Add("@TableComment", table.Comment);
            replaceKeys.Add("@Coulmns", this.GetColumns(info, columns));
            replaceKeys.Add("@KEY", this.GetKeys(info, columns));
            return Replacekeyword(classTemp, replaceKeys);
        }

        private string GetKeys(SourceGenerateTemplate.TemplatesRow info, IList<TableLayoutInfo> columns)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("@Key(");
            int index = 0;
            foreach(var column in columns)
            {
                if (column.IsPrimaryKey)
                {
                    if (index > 0)
                    {
                        sb.Append(",");
                    }
                    sb.Append(column.ColumnName);
                    index++;
                }
            }
            sb.Append(")");
            return sb.ToString();
        }

        private string GetColumns(SourceGenerateTemplate.TemplatesRow info, IList<TableLayoutInfo> columns)
        {
          
            StringBuilder sb = new StringBuilder();
            string[] ignoreColumns = info.GetIgnoreColumns();
            foreach (var col in columns)
            {
                if(ignoreColumns.Any(x=>x.Equals(col.ColumnName.ToLower(), StringComparison.CurrentCultureIgnoreCase)))
                {
                    continue;
                }
                Dictionary<string, string> replaceKeys = new Dictionary<string, string>();
                replaceKeys.Add("@ColumnComment", col.Comment);
                replaceKeys.Add("@DataType", col.GetDataTypeForJava());
                replaceKeys.Add("@FieldDisplayName", col.ColumnDisplayName);
                replaceKeys.Add("@FieldName", col.ColumnName);
                replaceKeys.Add("@FieldNameCamelU", GetCamelName( col.ColumnName,true));
                replaceKeys.Add("@FieldNameCamelL", GetCamelName(col.ColumnName, false));
                replaceKeys.Add("@KEY", col.IsPrimaryKey? " [Key(" + col.ColumnName +")]":"" );
                sb.AppendLine(Replacekeyword(info.FieldTemplace, replaceKeys));
            }
            return sb.ToString();
        }


        private string Replacekeyword(string template, Dictionary<string, string> dics)
        {
            foreach (string key in dics.Keys)
            {
                template = template.Replace("{" + key + "}", dics[key]);
            }
            return template;
        }
        
        private string GetTemplateFile(DbDocumentInfo docInfo)
        {
            string basePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string fileName = System.IO.Path.Combine(basePath, "Template", docInfo.TemplateInfo.TemplateFileName);
            return fileName;
        }
    }
}
