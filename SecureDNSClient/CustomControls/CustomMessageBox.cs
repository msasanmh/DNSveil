using MsmhTools;
using System.Runtime.InteropServices;
/*
 * Copyright MSasanMH, April 14, 2022.
 * Needs CustomButton.
 */

namespace CustomControls
{
    public class CustomMessageBox : Form
    {
        // Make CustomMessageBox movable.
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        static DialogResult dialogResult;
        public new static Color BackColor { get; set; }
        public new static Color ForeColor { get; set; }
        public static Color BorderColor { get; set; }
        private static int ImageBox(int iconOffset, Icon icon, Panel textPanel)
        {
            PictureBox pb = new();
            pb.Image = icon.ToBitmap();
            pb.Size = icon.Size;
            pb.Location = new(iconOffset, textPanel.Height / 2 - pb.Height / 2);
            textPanel.Controls.Add(pb);
            return pb.Height;
        }
        public CustomMessageBox(string text, string? caption, MessageBoxButtons? buttons, MessageBoxIcon? icon) : base()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            int iconOffset = 5;
            int buttonOffset = 5;
            int testLabelOffset = iconOffset;
            Rectangle screen = Screen.FromControl(this).Bounds;

            // Box Size (Auto)
            ////// Box Width
            AutoSize = false;
            int maxWidth = 400;
            int minWidth = 140; // Shouldn't be smaller than 140.
            Size computeSize = TextRenderer.MeasureText(text, DefaultFont);
            int mWidth = computeSize.Width + 30;
            int iconWidth = 32;
            if (icon != null && icon != MessageBoxIcon.None)
            {
                mWidth += iconWidth + iconOffset;
                testLabelOffset = iconWidth - iconOffset * 2;
            }
            int buttonWidth = 75;
            if (buttons == null || buttons == MessageBoxButtons.OK)
            {
                int previousWidth = mWidth;
                mWidth = buttonWidth + buttonOffset * 2;
                mWidth = Math.Max(previousWidth, mWidth);
                mWidth += buttonOffset;
            }
            else if (buttons == MessageBoxButtons.OKCancel || buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.RetryCancel)
            {
                int previousWidth = mWidth;
                mWidth = buttonWidth * 2 + buttonOffset * 3;
                mWidth = Math.Max(previousWidth, mWidth);
                mWidth += buttonOffset;
            }
            else if (buttons == MessageBoxButtons.AbortRetryIgnore || buttons == MessageBoxButtons.YesNoCancel || buttons == MessageBoxButtons.CancelTryContinue)
            {
                int previousWidth = mWidth;
                mWidth = buttonWidth * 3 + buttonOffset * 4;
                mWidth = Math.Max(previousWidth, mWidth);
                mWidth += buttonOffset;
            }
            mWidth = Math.Min(maxWidth, mWidth);
            mWidth = Math.Max(minWidth, mWidth);

            ////// Box Height
            int minHeight = 130;
            int maxHeight = screen.Height - 50;
            Label testLabel = new();
            testLabel.AutoSize = true;
            testLabel.MaximumSize = new Size(mWidth - 2 - testLabelOffset, 0);
            testLabel.TextAlign = ContentAlignment.MiddleLeft;
            testLabel.Text = text;
            Size testLabelSize = testLabel.GetPreferredSize(Size.Empty);
            int mHeight = testLabelSize.Height + 100;
            mHeight = Math.Max(minHeight, mHeight);
            if (mHeight > maxHeight)
            {
                mWidth += maxWidth;
                testLabel.MaximumSize = new Size(mWidth - 2 - testLabelOffset, 0);
                testLabelSize = testLabel.GetPreferredSize(Size.Empty);
                mHeight = testLabelSize.Height + 100;
                mHeight = Math.Max(minHeight, mHeight);
                if (mHeight > maxHeight)
                {
                    mWidth += maxWidth;
                    testLabel.MaximumSize = new Size(mWidth - 2 - testLabelOffset, 0);
                    testLabelSize = testLabel.GetPreferredSize(Size.Empty);
                    mHeight = testLabelSize.Height + 100;
                    mHeight = Math.Max(minHeight, mHeight);
                    if (mHeight > maxHeight)
                    {
                        mWidth += maxWidth;
                        testLabel.MaximumSize = new Size(mWidth - 2 - testLabelOffset, 0);
                        testLabelSize = testLabel.GetPreferredSize(Size.Empty);
                        mHeight = testLabelSize.Height + 100;
                        mHeight = Math.Max(minHeight, mHeight);
                        if (mHeight > maxHeight)
                        {
                            mWidth += maxWidth;
                            testLabel.MaximumSize = new Size(mWidth - 2 - testLabelOffset, 0);
                            testLabelSize = testLabel.GetPreferredSize(Size.Empty);
                            mHeight = testLabelSize.Height + 100;
                            mHeight = Math.Max(minHeight, mHeight);
                            if (mWidth > screen.Width)
                            {
                                mWidth = screen.Width;
                                mHeight = maxHeight;
                            }
                        }
                    }
                }
            }
            Size = new(mWidth, mHeight);

            // Rectangle
            Rectangle rect = new(ClientRectangle.X + 1, ClientRectangle.Y + 1, ClientRectangle.Width - 2, ClientRectangle.Height - 2);
            int buttonPanelHeight = 30;
            int titlePanelHeight = 25;
            Paint += CustomMessageBox_Paint;
            MouseDown += CustomMessageBox_MouseDown;
            Move += CustomMessageBox_Move;

            // Title (Caption)
            if (caption != null)
            {
                Label titleLabel = new();
                titleLabel.AutoSize = true;
                titleLabel.TextAlign = ContentAlignment.MiddleLeft;
                titleLabel.Text = caption;
                titleLabel.BackColor = BackColor;
                titleLabel.ForeColor = ForeColor;
                titleLabel.Location = new(2, rect.Y + titlePanelHeight / 2 - titleLabel.GetPreferredSize(Size.Empty).Height / 2);
                Controls.Add(titleLabel);
            }

            // Text Body
            Panel textPanel = new();
            textPanel.BackColor = BackColor;
            textPanel.ForeColor = ForeColor;
            textPanel.BorderStyle = BorderStyle.None;
            textPanel.Margin = new Padding(0);
            textPanel.Location = new(rect.X, titlePanelHeight);
            textPanel.Size = new(rect.Width, rect.Height - titlePanelHeight - buttonPanelHeight);
            textPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(textPanel);

            // Enum MessageBoxIcon
            if (icon != null)
            {
                iconOffset = 5;
                if (icon == MessageBoxIcon.Asterisk)
                {
                    Icon ic = new(SystemIcons.Asterisk, 32, 32);
                    int pbWidth = ImageBox(iconOffset, ic, textPanel);
                    iconOffset = iconOffset + pbWidth + iconOffset;
                }
                else if (icon == MessageBoxIcon.Error)
                {
                    Icon ic = new(SystemIcons.Error, 32, 32);
                    int pbWidth = ImageBox(iconOffset, ic, textPanel);
                    iconOffset = iconOffset + pbWidth + iconOffset;
                }
                else if (icon == MessageBoxIcon.Exclamation)
                {
                    Icon ic = new(SystemIcons.Exclamation, 32, 32);
                    int pbWidth = ImageBox(iconOffset, ic, textPanel);
                    iconOffset = iconOffset + pbWidth + iconOffset;
                }
                else if (icon == MessageBoxIcon.Hand)
                {
                    Icon ic = new(SystemIcons.Hand, 32, 32);
                    int pbWidth = ImageBox(iconOffset, ic, textPanel);
                    iconOffset = iconOffset + pbWidth + iconOffset;
                }
                else if (icon == MessageBoxIcon.Information)
                {
                    Icon ic = new(SystemIcons.Information, 32, 32);
                    int pbWidth = ImageBox(iconOffset, ic, textPanel);
                    iconOffset = iconOffset + pbWidth + iconOffset;
                }
                else if (icon == MessageBoxIcon.None)
                {
                    // Do Nothing
                }
                else if (icon == MessageBoxIcon.Question)
                {
                    Icon ic = new(SystemIcons.Question, 32, 32);
                    int pbWidth = ImageBox(iconOffset, ic, textPanel);
                    iconOffset = iconOffset + pbWidth + iconOffset;
                }
                else if (icon == MessageBoxIcon.Stop)
                {
                    Icon ic = new(SystemIcons.Error, 32, 32);
                    int pbWidth = ImageBox(iconOffset, ic, textPanel);
                    iconOffset = iconOffset + pbWidth + iconOffset;
                }
                else if (icon == MessageBoxIcon.Warning)
                {
                    Icon ic = new(SystemIcons.Warning, 32, 32);
                    int pbWidth = ImageBox(iconOffset, ic, textPanel);
                    iconOffset = iconOffset + pbWidth + iconOffset;
                }
            }

            Label textLabel = new();
            textLabel.AutoSize = true;
            textLabel.MaximumSize = new Size(rect.Width - iconOffset, 0);
            textLabel.TextAlign = ContentAlignment.MiddleLeft;
            textLabel.Text = text;
            Size textLabelSize = textLabel.GetPreferredSize(Size.Empty);
            textLabel.Location = new(iconOffset, textPanel.Height / 2 - textLabelSize.Height / 2);
            textPanel.Controls.Add(textLabel);

            // Button
            Panel buttonPanel = new();
            buttonPanel.BackColor = BackColor.ChangeBrightness(-0.2f);
            buttonPanel.ForeColor = ForeColor;
            buttonPanel.BorderStyle = BorderStyle.None;
            buttonPanel.Margin = new Padding(0);
            buttonPanel.Location = new(rect.X, Height - buttonPanelHeight - 1); // 1 is bottom border
            buttonPanel.Size = new(rect.Width, buttonPanelHeight);
            buttonPanel.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            Controls.Add(buttonPanel);

            // Enum DialogResult
            if (buttons == null)
                buttons = MessageBoxButtons.OK;
            if (buttons == MessageBoxButtons.AbortRetryIgnore)
            {
                CustomButton btn1 = new();
                CustomButton btn2 = new();
                CustomButton btn3 = new();

                btn1.Location = new(rect.Width - btn1.Width - buttonOffset - btn2.Width - buttonOffset - btn3.Width - buttonOffset, buttonPanel.Height / 2 - btn1.Height / 2);
                btn1.Text = "Abort";
                btn1.DialogResult = DialogResult.Abort;
                btn1.Click += (object? sender, EventArgs e) =>
                {
                    dialogResult = DialogResult.Abort;
                };
                buttonPanel.Controls.Add(btn1);

                btn2.Location = new(rect.Width - btn2.Width - buttonOffset - btn3.Width - buttonOffset, buttonPanel.Height / 2 - btn2.Height / 2);
                btn2.Text = "Retry";
                btn2.DialogResult = DialogResult.Retry;
                btn2.Click += (object? sender, EventArgs e) =>
                {
                    dialogResult = DialogResult.Retry;
                };
                buttonPanel.Controls.Add(btn2);

                btn3.Location = new(rect.Width - btn3.Width - buttonOffset, buttonPanel.Height / 2 - btn3.Height / 2);
                btn3.Text = "Ignore";
                btn3.DialogResult = DialogResult.Ignore;
                btn3.Click += (object? sender, EventArgs e) =>
                {
                    dialogResult = DialogResult.Ignore;
                };
                buttonPanel.Controls.Add(btn3);

                AcceptButton = btn2;
                CancelButton = btn1;
            }
            else if (buttons == MessageBoxButtons.CancelTryContinue)
            {
                CustomButton btn1 = new();
                CustomButton btn2 = new();
                CustomButton btn3 = new();

                btn1.Location = new(rect.Width - btn1.Width - buttonOffset - btn2.Width - buttonOffset - btn3.Width - buttonOffset, buttonPanel.Height / 2 - btn1.Height / 2);
                btn1.Text = "Cancel";
                btn1.DialogResult = DialogResult.Cancel;
                btn1.Click += (object? sender, EventArgs e) =>
                {
                    dialogResult = DialogResult.Cancel;
                };
                buttonPanel.Controls.Add(btn1);

                btn2.Location = new(rect.Width - btn2.Width - buttonOffset - btn3.Width - buttonOffset, buttonPanel.Height / 2 - btn2.Height / 2);
                btn2.Text = "Try Again";
                btn2.DialogResult = DialogResult.TryAgain;
                btn2.Click += (object? sender, EventArgs e) =>
                {
                    dialogResult = DialogResult.TryAgain;
                };
                buttonPanel.Controls.Add(btn2);

                btn3.Location = new(rect.Width - btn3.Width - buttonOffset, buttonPanel.Height / 2 - btn3.Height / 2);
                btn3.Text = "Continue";
                btn3.DialogResult = DialogResult.Continue;
                btn3.Click += (object? sender, EventArgs e) =>
                {
                    dialogResult = DialogResult.Continue;
                };
                buttonPanel.Controls.Add(btn3);

                AcceptButton = btn2;
                CancelButton = btn1;
            }
            else if (buttons == MessageBoxButtons.OK)
            {
                CustomButton btn1 = new();

                btn1.Location = new(rect.Width - btn1.Width - buttonOffset, buttonPanel.Height / 2 - btn1.Height / 2);
                btn1.Text = "OK";
                btn1.DialogResult = DialogResult.OK;
                btn1.Click += (object? sender, EventArgs e) =>
                {
                    dialogResult = DialogResult.OK;
                };
                buttonPanel.Controls.Add(btn1);

                AcceptButton = btn1;
            }
            else if (buttons == MessageBoxButtons.OKCancel)
            {
                CustomButton btn1 = new();
                CustomButton btn2 = new();

                btn1.Location = new(rect.Width - btn1.Width - buttonOffset - btn2.Width - buttonOffset, buttonPanel.Height / 2 - btn1.Height / 2);
                btn1.Text = "OK";
                btn1.DialogResult = DialogResult.OK;
                btn1.Click += (object? sender, EventArgs e) =>
                {
                    dialogResult = DialogResult.OK;
                };
                buttonPanel.Controls.Add(btn1);

                btn2.Location = new(rect.Width - btn2.Width - buttonOffset, buttonPanel.Height / 2 - btn2.Height / 2);
                btn2.Text = "Cancel";
                btn2.DialogResult = DialogResult.Cancel;
                btn2.Click += (object? sender, EventArgs e) =>
                {
                    dialogResult = DialogResult.Cancel;
                };
                buttonPanel.Controls.Add(btn2);

                AcceptButton = btn1;
                CancelButton = btn2;
            }
            else if (buttons == MessageBoxButtons.RetryCancel)
            {
                CustomButton btn1 = new();
                CustomButton btn2 = new();

                btn1.Location = new(rect.Width - btn1.Width - buttonOffset - btn2.Width - buttonOffset, buttonPanel.Height / 2 - btn1.Height / 2);
                btn1.Text = "Retry";
                btn1.DialogResult = DialogResult.Retry;
                btn1.Click += (object? sender, EventArgs e) =>
                {
                    dialogResult = DialogResult.Retry;
                };
                buttonPanel.Controls.Add(btn1);

                btn2.Location = new(rect.Width - btn2.Width - buttonOffset, buttonPanel.Height / 2 - btn2.Height / 2);
                btn2.Text = "Cancel";
                btn2.DialogResult = DialogResult.Cancel;
                btn2.Click += (object? sender, EventArgs e) =>
                {
                    dialogResult = DialogResult.Cancel;
                };
                buttonPanel.Controls.Add(btn2);

                AcceptButton = btn1;
                CancelButton = btn2;
            }
            else if (buttons == MessageBoxButtons.YesNo)
            {
                CustomButton btn1 = new();
                CustomButton btn2 = new();

                btn1.Location = new(rect.Width - btn1.Width - buttonOffset - btn2.Width - buttonOffset, buttonPanel.Height / 2 - btn1.Height / 2);
                btn1.Text = "Yes";
                btn1.DialogResult = DialogResult.Yes;
                btn1.Click += (object? sender, EventArgs e) =>
                {
                    dialogResult = DialogResult.Yes;
                };
                buttonPanel.Controls.Add(btn1);

                btn2.Location = new(rect.Width - btn2.Width - buttonOffset, buttonPanel.Height / 2 - btn2.Height / 2);
                btn2.Text = "No";
                btn2.DialogResult = DialogResult.No;
                btn2.Click += (object? sender, EventArgs e) =>
                {
                    dialogResult = DialogResult.No;
                };
                buttonPanel.Controls.Add(btn2);

                AcceptButton = btn1;
                CancelButton = btn2;
            }
            else if (buttons == MessageBoxButtons.YesNoCancel)
            {
                CustomButton btn1 = new();
                CustomButton btn2 = new();
                CustomButton btn3 = new();

                btn1.Location = new(rect.Width - btn1.Width - buttonOffset - btn2.Width - buttonOffset - btn3.Width - buttonOffset, buttonPanel.Height / 2 - btn1.Height / 2);
                btn1.Text = "Yes";
                btn1.DialogResult = DialogResult.Yes;
                btn1.Click += (object? sender, EventArgs e) =>
                {
                    dialogResult = DialogResult.Yes;
                };
                buttonPanel.Controls.Add(btn1);

                btn2.Location = new(rect.Width - btn2.Width - buttonOffset - btn3.Width - buttonOffset, buttonPanel.Height / 2 - btn2.Height / 2);
                btn2.Text = "No";
                btn2.DialogResult = DialogResult.No;
                btn2.Click += (object? sender, EventArgs e) =>
                {
                    dialogResult = DialogResult.No;
                };
                buttonPanel.Controls.Add(btn2);

                btn3.Location = new(rect.Width - btn3.Width - buttonOffset, buttonPanel.Height / 2 - btn3.Height / 2);
                btn3.Text = "Cancel";
                btn3.DialogResult = DialogResult.Cancel;
                btn3.Click += (object? sender, EventArgs e) =>
                {
                    dialogResult = DialogResult.Cancel;
                };
                buttonPanel.Controls.Add(btn3);

                AcceptButton = btn1;
                CancelButton = btn3;
            }

            // Set CustomButton Colors
            var cs = Controllers.GetAllControls(this);
            foreach (Control c in cs)
                if (c is CustomButton customButton)
                {
                    customButton.BackColor = BackColor;
                    customButton.ForeColor = ForeColor;
                    customButton.BorderColor = BorderColor;
                    customButton.SelectionColor = BorderColor;
                    customButton.Invalidate();
                }
        }

        /// <summary>Displays a message box with specified text.</summary>
        ///
        /// <param name="text">The text to display in the message box.</param>
        ///
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(string text)
        {
            // using construct ensures the resources are freed when form is closed.
            using CustomMessageBox form = new(text, null, null, null);
            form.ShowDialog();
            return dialogResult;
        }

        /// <summary>Displays a message box with specified text.</summary>
        ///
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        ///
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(string text, string caption)
        {
            // using construct ensures the resources are freed when form is closed.
            using CustomMessageBox form = new(text, caption, null, null);
            form.ShowDialog();
            return dialogResult;
        }

        /// <summary>Displays a message box with specified text.</summary>
        ///
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">One of the System.Windows.Forms.MessageBoxButtons values that specifies which buttons to display in the message box.</param>
        ///
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
        {
            // using construct ensures the resources are freed when form is closed.
            using CustomMessageBox form = new(text, caption, buttons, null);
            form.ShowDialog();
            return dialogResult;
        }

        /// <summary>Displays a message box with specified text.</summary>
        ///
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">One of the System.Windows.Forms.MessageBoxButtons values that specifies which buttons to display in the message box.</param>
        /// <param name="icon">One of the System.Windows.Forms.MessageBoxIcon values that specifies which icon to display in the message box.</param>
        ///
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            // using construct ensures the resources are freed when form is closed.
            using CustomMessageBox form = new(text, caption, buttons, icon);
            form.ShowDialog();
            return dialogResult;
        }

        private void CustomMessageBox_Paint(object? sender, PaintEventArgs e)
        {
            Rectangle rect = new(e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width - 1, e.ClipRectangle.Height - 1);
            Graphics g = e.Graphics;

            // Fill BackColor (Static)
            using SolidBrush sb = new(BackColor);
            g.FillRectangle(sb, rect);

            // Draw Border
            using Pen pen = new(BorderColor);
            g.DrawRectangle(pen, rect);
        }

        private void CustomMessageBox_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                _ = SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                Invalidate();
            }
        }

        private void CustomMessageBox_Move(object? sender, EventArgs e)
        {
            Invalidate();
            var cs = Controllers.GetAllControls(this);
            foreach (Control c in cs)
                c.Invalidate();
        }

    }
}
