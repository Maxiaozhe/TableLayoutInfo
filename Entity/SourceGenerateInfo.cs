using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableDesignInfo.Common;

namespace TableDesignInfo.Entity
{
    public class SourceGenerateInfo
    {
        private List<string> _tableNames;

        public SourceGenerateInfo()
        {
            this._tableNames = new List<string>();
        }

        /// <summary>
        /// ファイル名
        /// </summary>
        public string SavePath
        {
            get;
            set;
        }
        /// <summary>
        /// 生成するテーブル一覧
        /// </summary>
        public List<string> TableNames
        {
            get
            {
                return this._tableNames;
            }
        }
        /// <summary>
        /// テンプレート情報
        /// </summary>
        public SourceGenerateTemplate.TemplatesRow TemplateInfo
        {
            get;
            set;
        }
    }
}
