using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TableDesignInfo.Controls;
using TableDesignInfo.Entity;

namespace TableDesignInfo.Forms
{
    public partial class SourceCreateForm : Form
    {
        private SourceGenerateInfo _createInfo;

        public SourceCreateForm()
        {
            InitializeComponent();
            this._createInfo = new SourceGenerateInfo();

        }

        public SourceGenerateInfo CreateInfo
        {
            get
            {
                return this._createInfo;
            }
        }

        private void DocumentCreateForm_Load(object sender, EventArgs e)
        {
            try
            {
                InitControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void InitControls()
        {
            //テンプレート一覧
            SourceGenerateTemplate.TemplatesDataTable templates = SourceGenerateTemplate.LoadTemplate();
            this.cmbTemplate.DisplayMember = "Name";
            this.cmbTemplate.DataSource = templates;
            //テーブル一覧
            SearchOptions opts = SearchOptions.None;
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.DataSource = LinqSqlHelp.Search("", SearchModes.Contain, opts);
            //選択状態
            SetDataViewState(true);
        }


        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtSaveFolder.Text))
            {
                MessageBox.Show(this, "出力場所を指定してください。", "", MessageBoxButtons.OK);
                return;
            }
            if(this.cmbTemplate.SelectedItem==null){
                MessageBox.Show(this, "テンプレートを選択してください。", "", MessageBoxButtons.OK);
                return;
            }
            this._createInfo.SavePath = this.txtSaveFolder.Text;
            this._createInfo.TemplateInfo = (SourceGenerateTemplate.TemplatesRow)((DataRowView)this.cmbTemplate.SelectedItem).Row;
            foreach (DataGridViewRow row in this.dataGridView1.Rows)
            {
                bool isChecked =(bool) row.Cells[0].FormattedValue;
                if (isChecked)
                {
                    TableList selectedTable =  row.DataBoundItem as TableList;
                    this._createInfo.TableNames.Add(selectedTable.TableName);
                }
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            SetDataViewState(true);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            SetDataViewState(false);
        }

        private void SetDataViewState(bool check)
        {
            foreach (DataGridViewRow row in this.dataGridView1.Rows)
            {
                row.Cells[0].Value = check;
            }
        }

        private void btnFileName_Click(object sender, EventArgs e)
        {

            this.folderBrowserDialog1.SelectedPath=this.txtSaveFolder.Text;
            if (this.folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                this.txtSaveFolder.Text = this.folderBrowserDialog1.SelectedPath;
            }
        }

    }
}
