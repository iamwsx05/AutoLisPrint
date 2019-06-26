using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Common.Controls;
using DevExpress.XtraEditors;
using weCare.Core.Utils;

namespace MessageBoxDemo
{
    public partial class frmShowMessage : frmBase
    {
        public frmShowMessage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// setMessage method is used to display message
        /// on form and it's height adjust automatically.
        /// I am displaying message in a Label control.
        /// </summary>
        /// <param name="messageText">Message which needs to be displayed to user.</param>
        private void setMessage(string messageText)
        {
            int number = Math.Abs(messageText.Length / 60);
            if (number != 0)
                this.lblMessageText.Height = number * 25;
            this.lblMessageText.Text = messageText;
        }

        /// <summary>
        /// This method is used to add button on message box.
        /// </summary>
        /// <param name="MessageButton">MessageButton is type of enumMessageButton
        /// through which I get type of button which needs to be displayed.</param>
        private void addButton(enumMessageButton MessageButton)
        {
            switch (MessageButton)
            {
                case enumMessageButton.OK:
                    {
                        //If type of enumButton is OK then we add OK button only.
                        SimpleButton btnOk = new SimpleButton();  //Create object of Button.
                        btnOk.Text = "确定";  //Here we set text of Button.
                        btnOk.Appearance.Font = new System.Drawing.Font("Tahoma", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        btnOk.DialogResult = DialogResult.Yes;  //Set DialogResult property of button.
                        btnOk.SetBounds(pnlShowMessage.ClientSize.Width / 2, 1, 105, 40);  // Set bounds of button.
                        //btnOk.Click += new EventHandler(btnOk_Click);
                        pnlShowMessage.Controls.Add(btnOk);  //Finally Add button control on panel.
                    }
                    break;
                case enumMessageButton.OKCancel:
                    {
                        Button btnOk = new Button();
                        btnOk.Text = "OK";
                        btnOk.DialogResult = DialogResult.OK;
                        btnOk.FlatStyle = FlatStyle.Popup;
                        btnOk.FlatAppearance.BorderSize = 0;
                        btnOk.SetBounds((pnlShowMessage.ClientSize.Width - 70), 5, 65, 25);
                        pnlShowMessage.Controls.Add(btnOk);

                        Button btnCancel = new Button();
                        btnCancel.Text = "Cancel";
                        btnCancel.DialogResult = DialogResult.Cancel;
                        btnCancel.FlatStyle = FlatStyle.Popup;
                        btnCancel.FlatAppearance.BorderSize = 0;
                        btnCancel.SetBounds((pnlShowMessage.ClientSize.Width - (btnOk.ClientSize.Width + 5 + 80)), 5, 75, 25);
                        pnlShowMessage.Controls.Add(btnCancel);

                    }
                    break;
                case enumMessageButton.YesNo:
                    {
                        Button btnNo = new Button();
                        btnNo.Text = "No";
                        btnNo.DialogResult = DialogResult.No;
                        btnNo.FlatStyle = FlatStyle.Popup;
                        btnNo.FlatAppearance.BorderSize = 0;
                        btnNo.SetBounds((pnlShowMessage.ClientSize.Width - 70), 5, 65, 25);
                        pnlShowMessage.Controls.Add(btnNo);

                        Button btnYes = new Button();
                        btnYes.Text = "Yes";
                        btnYes.DialogResult = DialogResult.Yes;
                        btnYes.FlatStyle = FlatStyle.Popup;
                        btnYes.FlatAppearance.BorderSize = 0;
                        btnYes.SetBounds((pnlShowMessage.ClientSize.Width - (btnNo.ClientSize.Width + 5 + 80)), 5, 75, 25);
                        pnlShowMessage.Controls.Add(btnYes);
                    }
                    break;
                case enumMessageButton.YesNoCancel:
                    {
                        Button btnCancel = new Button();
                        btnCancel.Text = "取消";
                        btnCancel.DialogResult = DialogResult.Cancel;
                        btnCancel.FlatStyle = FlatStyle.Popup;
                        btnCancel.FlatAppearance.BorderSize = 0;
                        btnCancel.SetBounds((pnlShowMessage.ClientSize.Width - 70), 5, 65, 25);
                        pnlShowMessage.Controls.Add(btnCancel);

                        Button btnNo = new Button();
                        btnNo.Text = "No";
                        btnNo.DialogResult = DialogResult.No;
                        btnNo.FlatStyle = FlatStyle.Popup;
                        btnNo.FlatAppearance.BorderSize = 0;
                        btnNo.SetBounds((pnlShowMessage.ClientSize.Width - (btnCancel.ClientSize.Width + 5 + 80)), 5, 75, 25);
                        pnlShowMessage.Controls.Add(btnNo);

                        Button btnYes = new Button();
                        btnYes.Text = "确定";
                        btnYes.DialogResult = DialogResult.No;
                        btnYes.FlatStyle = FlatStyle.Popup;
                        btnYes.FlatAppearance.BorderSize = 0;
                        btnYes.SetBounds((pnlShowMessage.ClientSize.Width - (btnCancel.ClientSize.Width + btnNo.ClientSize.Width + 10 + 80)), 5, 75, 25);
                        pnlShowMessage.Controls.Add(btnYes);
                    }
                    break;
            }
        }

        /// <summary>
        /// We can use this method to add image on message box.
        /// I had taken all images in ImageList control so that
        /// I can eaily add images. Image is displayed in 
        /// PictureBox control.
        /// </summary>
        /// <param name="MessageIcon">Type of image to be displayed.</param>
        private void addIconImage(enumMessageIcon MessageIcon)
        {
            switch (MessageIcon)
            {
                case enumMessageIcon.Error:
                    pictureBox1.Image = imageList1.Images["Error"];  //Error is key name in imagelist control which uniqly identified images in ImageList control.
                    break;
                case enumMessageIcon.Information:
                    pictureBox1.Image = imageList1.Images["Information"];
                    break;
                case enumMessageIcon.Question:
                    pictureBox1.Image = imageList1.Images["Question"];
                    break;
                case enumMessageIcon.Warning:
                    pictureBox1.Image = imageList1.Images["Warning"];
                    break;
            }
        }

        #region Overloaded Show message to display message box.

        /// <summary>
        /// Show method is overloaded which is used to display message
        /// and this is static method so that we don't need to create 
        /// object of this class to call this method.
        /// </summary>
        /// <param name="messageText"></param>
        internal static void Show(string messageText)
        {
            frmShowMessage frmMessage = new frmShowMessage();
            frmMessage.setMessage(messageText);
            frmMessage.addIconImage(enumMessageIcon.Information);
            frmMessage.addButton(enumMessageButton.OK);
            frmMessage.ShowDialog();
        }

        internal static void Show(string messageText, string messageTitle)
        {
            frmShowMessage frmMessage = new frmShowMessage();
            frmMessage.Text = messageTitle;
            frmMessage.setMessage(messageText);
            frmMessage.addIconImage(enumMessageIcon.Information);
            frmMessage.addButton(enumMessageButton.OK);
            frmMessage.ShowDialog();
        }

        internal static void Show(string messageText, string messageTitle, enumMessageIcon messageIcon, enumMessageButton messageButton)
        {
            frmShowMessage frmMessage = new frmShowMessage();
            frmMessage.setMessage(messageText);
            frmMessage.Text = messageTitle;
            frmMessage.addIconImage(messageIcon);
            frmMessage.addButton(messageButton);
            frmMessage.ShowDialog();
        }

        internal static DialogResult Show(string messageText, enumMessageIcon messageIcon, enumMessageButton messageButton)
        {
            frmShowMessage frmMessage = new frmShowMessage();
            frmMessage.setMessage(messageText);
            frmMessage.addIconImage(messageIcon);
            frmMessage.addButton(messageButton);
            return frmMessage.ShowDialog();
        }

        #endregion

        private void btnOk_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Yes;
        }

        private void frmShowMessage_Load(object sender, EventArgs e)
        {
            Function.AnimateWindow(base.Handle);
        }
    }

    #region constant defiend in form of enumration which is used in showMessage class.

    internal enum enumMessageIcon
    {
        Error,
        Warning,
        Information,
        Question,
    }

    internal enum enumMessageButton
    {
        OK,
        YesNo,
        YesNoCancel,
        OKCancel
    }

    #endregion
}
