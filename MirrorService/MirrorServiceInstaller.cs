using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace MirrorService
{
    [RunInstaller(true)]
    public partial class MirrorServiceInstaller : System.Configuration.Install.Installer
    {
        public MirrorServiceInstaller()
        {
            InitializeComponent();
        }
    }
}
