namespace TableDesignInfo.Entity
{
    partial class SourceGenerateTemplate
    {

        public static SourceGenerateTemplate.TemplatesDataTable LoadTemplate()
        {
            string basePath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string xmlfile = System.IO.Path.Combine(basePath, @"Template\SourceGenerateTemplate.xml");

            SourceGenerateTemplate info = new SourceGenerateTemplate();
            info.ReadXml(xmlfile);
            return info.Templates;
        }

        public partial class TemplatesRow
        {
            public string[] GetIgnoreColumns()
            {
                return this.IgnoreColumns.Split(',');
            }
        }

    }

}
