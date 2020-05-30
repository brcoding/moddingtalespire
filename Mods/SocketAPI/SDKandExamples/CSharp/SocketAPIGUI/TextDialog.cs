using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketAPIGUI
{
    public partial class TextDialog : Form
    {
        public TextDialog(string title, string question, string defaultAnswer = "")
        {
            InitializeComponent();
            this.Text = title;
            lbQuestion.Text = question;
            tbAnswer.Text = defaultAnswer;
        }
        public string Answer
        {
            get { return tbAnswer.Text; }
        }

        private void TextDialog_Shown(object sender, EventArgs e)
        {
            tbAnswer.SelectAll();
            tbAnswer.Focus();
        }

    }
}
