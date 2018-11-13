using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ChatUp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        //Another approach to bind the view and viewmodel
        /*
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ChatUp.ChatView window = new ChatView();
            ChatViewModel VM = new ChatViewModel();
            window.DataContext = VM;
            window.Show();
        }*/

        /*
        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;

            InitializeComponent();
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //this. = new ChatView();
            
        }

        private void Application_Exit(object sender, EventArgs e)
        {

        }*/
    }
}
