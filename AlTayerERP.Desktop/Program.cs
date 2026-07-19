namespace AlTayerERP.Desktop
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
       //     Application.Run(new FrmMain());
            Application.Run(new FrmLogin());
            //    Application.Run(new Form1());
            //     Application.Run(new CompanyForm());
            //          Application.Run(new FrmChartOfAccounts());
            //    Application.Run(new FrmChartOfAccounts());
         //   Application.Run(new BranchForm());
            //    Application.Run(new FrmNumberingSettings());
           // Application.Run(new FiscalYearForm());

       //     Application.Run(new FrmUsers());

            //      FrmChartOfAccounts frm = new FrmChartOfAccounts();
            //     frm.ShowDialog();

        }

    }
}