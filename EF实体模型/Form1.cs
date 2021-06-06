using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EF实体模型.Models2;

namespace EF实体模型
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            using(TestContext db=new TestContext())
            {
                db.Numeber1s.Add(new Numeber1() { Id = 2, Num1 = 333 });
                db.SaveChanges();
            }
        }
    }
}
