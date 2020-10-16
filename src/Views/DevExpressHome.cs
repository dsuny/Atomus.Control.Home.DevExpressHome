using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Web;

namespace Atomus.Control.Home
{
    public partial class DevExpressHome : XtraUserControl, IAction
    {
        private AtomusControlEventHandler beforeActionEventHandler;
        private AtomusControlEventHandler afterActionEventHandler;
        
        #region Init
        public DevExpressHome()
        {
            InitializeComponent();
        }
        #endregion

        #region Dictionary
        #endregion

        #region Spread
        #endregion

        #region IO
        object IAction.ControlAction(ICore sender, AtomusControlArgs e)
        {
            try
            {
                this.beforeActionEventHandler?.Invoke(this, e);

                if (e.Action.StartsWith("Button"))
                    this.ExecuteSSO(e.Action);

                switch (e.Action)
                {
                    default:
                        return null;// throw new AtomusException("'{0}'은 처리할 수 없는 Action 입니다.".Translate(e.Action));
                }
            }
            finally
            {
                this.afterActionEventHandler?.Invoke(this, e);
            }
        }
        #endregion

        #region Event
        event AtomusControlEventHandler IAction.BeforeActionEventHandler
        {
            add
            {
                this.beforeActionEventHandler += value;
            }
            remove
            {
                this.beforeActionEventHandler -= value;
            }
        }
        event AtomusControlEventHandler IAction.AfterActionEventHandler
        {
            add
            {
                this.afterActionEventHandler += value;
            }
            remove
            {
                this.afterActionEventHandler -= value;
            }
        }


        private void DevExpressHome_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.Control control;
            string[] temps;

            try
            {
                control = null;

                temps = this.GetAttribute("Controls").Split(',');

                if (temps == null || temps.Count() < 1)
                    return;

                foreach (string tmp in temps)
                {
                    if (this.GetAttribute(string.Format("Controls.{0}.Namespace", tmp)).Equals("Menu.GetControl"))
                    {
                        AtomusControlEventArgs e1;

                        e1 = new AtomusControlEventArgs("Menu.GetControl", new object[] { this.GetAttributeDecimal(string.Format("Controls.{0}.MenuID", tmp))
                                                                                , this.GetAttributeDecimal(string.Format("Controls.{0}.AssemblyID", tmp))
                                                                                , null, false });

                        this.afterActionEventHandler?.Invoke(this, e1);

                        if (e1.Value is System.Windows.Forms.Control)
                            control = (System.Windows.Forms.Control)e1.Value;

                    }
                    else
                    {
                        control = (System.Windows.Forms.Control)this.CreateInstance(string.Format("Controls.{0}.Namespace", tmp));

                        control.Dock = DockStyle.Fill;
                    }

                    if (control != null)
                    {
                        this.SetDock(control, this.GetAttribute(string.Format("Controls.{0}.Dock", tmp)));

                        this.Controls.Add(control);

                        control.BringToFront();
                    }
                }
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }

        private void Menu_BeforeActionEventHandler(ICore sender, AtomusControlEventArgs e) { }
        private void Menu_AfterActionEventHandler(ICore sender, AtomusControlEventArgs e)
        {
            this.afterActionEventHandler?.Invoke(this, e);
        }

        private void Home_BeforeActionEventHandler(ICore sender, AtomusControlEventArgs e) { }
        private void Home_AfterActionEventHandler(ICore sender, AtomusControlEventArgs e) { }

        //private void Temp_ClosingPanel(object sender, DevExpress.XtraBars.Docking.DockPanelCancelEventArgs e)
        //{
        //    this.FindForm().Close();

        //    e.Cancel = true;
        //}
        #endregion

        #region "ETC"
        private void SetDock(System.Windows.Forms.Control control, string dock)
        {
            if (dock == null)
                return;

            control.Dock = (DockStyle)Enum.Parse(typeof(DockStyle), dock);
        }

        private void ExecuteSSO(string action)
        {
            if (this.GetAttribute(string.Format("{0}.ActionType", action)) != null && this.GetAttribute(string.Format("{0}.ActionType", action)) == "SSO")
            {
                string tmp1;
                string tmp2;
                string timeKey;

                while (true)
                {
                    timeKey = DateTime.Now.ToString("yyyyMMddhhmmssfff");

                    tmp1 = HttpUtility.UrlEncode(this.Encrypt(Config.Client.GetAttribute("Account.EMAIL").ToString(), timeKey));
                    tmp2 = HttpUtility.UrlDecode(tmp1);

                    if (tmp1 == tmp2)
                        break;
                }

                tmp1 = string.Format(this.GetAttribute(string.Format("{0}.ActionValue", action)), tmp1, "", timeKey);

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tmp1));
            }
        }
        private string Encrypt(string cipher, string type)
        {
            string EncryptionKey;
            byte[] cipherBytes;

            EncryptionKey = string.Format(this.GetAttribute("EncryptKey"), type);

            cipherBytes = System.Text.Encoding.Unicode.GetBytes(cipher);

            using (System.Security.Cryptography.Rijndael encryptor = System.Security.Cryptography.Rijndael.Create())
            {
                System.Security.Cryptography.Rfc2898DeriveBytes pdb = new System.Security.Cryptography.Rfc2898DeriveBytes(EncryptionKey, Convert.FromBase64String(this.GetAttribute("EncryptSalt")));

                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    using (System.Security.Cryptography.CryptoStream cs = new System.Security.Cryptography.CryptoStream(ms, encryptor.CreateEncryptor(), System.Security.Cryptography.CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }

                    cipher = Convert.ToBase64String(ms.ToArray());
                }
            }

            return cipher;
        }



        //private void CreateDockPanel()
        //{
        //    string[] temps;
        //    DevExpress.XtraBars.Docking.DockPanel parent;
        //    DevExpress.XtraBars.Docking.DockPanel temp;

        //    parent = dockManager1.ActivePanel;
        //    temps = this.GetAttribute("DockPanel").Split(',');

        //    if (temps == null || temps.Count() < 1)
        //        return;

        //    foreach (string tmp in temps)
        //    {
        //        if (parent != null)
        //            temp = parent.AddPanel();
        //        else
        //            temp = dockManager1.AddPanel(DevExpress.XtraBars.Docking.DockingStyle.Left);

        //        temp.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;

        //        SetDock(temp, this.GetAttribute(string.Format("DockPanel.{0}.DockingStyle", tmp)));

        //        this.CreateDockPanel(string.Format("DockPanel.{0}", tmp), temp);

        //        temp.Text = this.GetAttribute(string.Format("DockPanel.{0}.Text", tmp));
        //        temp.Size = this.GetAttributeSize(string.Format("DockPanel.{0}.Size", tmp));

        //        temp.DockedAsTabbedDocument = this.GetAttributeBool(string.Format("DockPanel.{0}.DockedAsTabbedDocument", tmp));

        //        AddAtomusUserControl(temp, string.Format("DockPanel.{0}", tmp));

        //        if (tmp.Contains("Home"))
        //            temp.ClosingPanel += Temp_ClosingPanel;
        //    }
        //}
        //private void CreateDockPanel(string baseAttributeName, DevExpress.XtraBars.Docking.DockPanel parent)
        //{
        //    DevExpress.XtraBars.Docking.DockPanel temp;
        //    string temp1;
        //    string[] temps;


        //    temp1 = this.GetAttribute(string.Format("{0}.Child", baseAttributeName));

        //    if (temp1 == null || temp1.Equals(string.Empty))
        //        return;

        //    temps = temp1.Split(',');

        //    foreach (string tmp in temps)
        //    {
        //        if (tmp.Equals(string.Empty))
        //            continue;

        //        if (parent != null)
        //            temp = parent.AddPanel();
        //        else
        //            temp = dockManager1.AddPanel(DevExpress.XtraBars.Docking.DockingStyle.Left);


        //        SetDock(temp, this.GetAttribute(string.Format("{0}.{1}.DockingStyle", baseAttributeName, tmp)));

        //        this.CreateDockPanel(string.Format("{0}.{1}", baseAttributeName, tmp), temp);

        //        temp.Text = this.GetAttribute(string.Format("{0}.{1}.Text", baseAttributeName, tmp));
        //        temp.Size = this.GetAttributeSize(string.Format("{0}.{1}.Size", baseAttributeName, tmp));

        //        temp.DockedAsTabbedDocument = this.GetAttributeBool(string.Format("DockPanel.{0}.DockedAsTabbedDocument", tmp));

        //        AddAtomusUserControl(temp, string.Format("{0}.{1}", baseAttributeName, tmp));
        //    }
        //}

        //private void AddAtomusUserControl(DevExpress.XtraBars.Docking.DockPanel dockPanel, string baseAttributeName)
        //{
        //    System.Windows.Forms.Control control;
        //    IAction action;
        //    string Namespace;

        //    control = null;

        //    Namespace = string.Format("{0}.Namespace", baseAttributeName);

        //    if (Namespace == null || this.GetAttribute(Namespace) == null)
        //        return;

        //    if (this.GetAttribute(Namespace).Equals("Menu.GetControl"))
        //    {
        //        AtomusControlEventArgs e;

        //        //Action, new object[] { _MENU_ID, _ASSEMBLY_ID, AtomusControlEventArgs, addTabControl }
        //        e = new AtomusControlEventArgs("Menu.GetControl", new object[] { this.GetAttribute(string.Format(".{0}.MenuID", baseAttributeName)).ToDecimal()
        //                                                                        , this.GetAttribute(string.Format(".{0}.AssemblyID", baseAttributeName)).ToDecimal()
        //                                                                        , null, false });

        //        this.afterActionEventHandler?.Invoke(this, e);

        //        if (e.Value is System.Windows.Forms.Control)
        //            control = (System.Windows.Forms.Control)e.Value;
        //    }
        //    else
        //        //control = new Menu.DefaultMenu();
        //        control = (System.Windows.Forms.Control)this.CreateInstance(Namespace);

        //    control.Name = baseAttributeName;

        //    if (baseAttributeName.Contains("Menu"))
        //    {
        //        action = (IAction)control;
        //        action.BeforeActionEventHandler += Menu_BeforeActionEventHandler;
        //        action.AfterActionEventHandler += Menu_AfterActionEventHandler;
        //    }
        //    else//if (controlName.Contains("Home"))
        //    {
        //        action = (IAction)control;
        //        action.BeforeActionEventHandler += Home_BeforeActionEventHandler;
        //        action.AfterActionEventHandler += Home_AfterActionEventHandler;
        //    }

        //    control.Dock = DockStyle.Fill;

        //    dockPanel.Controls.Add(control);
        //    control.BringToFront();
        //}

        //private void SetDock(DevExpress.XtraBars.Docking.DockPanel control, string dock)
        //{
        //    if (dock == null)
        //        return;

        //    control.Dock = (DevExpress.XtraBars.Docking.DockingStyle)Enum.Parse(typeof(DevExpress.XtraBars.Docking.DockingStyle), dock);
        //}
        #endregion
    }
}
