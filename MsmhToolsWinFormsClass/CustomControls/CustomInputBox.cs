using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using System.Runtime.InteropServices;
/*
 * Copyright MSasanMH, May 13, 2022.
 * Needs CustomButton.
 */

namespace CustomControls
{
    public class CustomInputBox : Form
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private extern static int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string? pszSubIdList);

        // Make CustomInputBox movable.
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        private static CustomTextBox inputTextBox = new();
        private static DialogResult dialogResult;
        private static string Input = string.Empty;
        private readonly int ButtonRoundedCorners = 5;

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

        private void SetInput(string input)
        {
            Input = input;
        }

        public CustomInputBox()
        {

        }

        public CustomInputBox(string text, bool multiline, string? caption, MessageBoxIcon? icon, int? addWidth, int? addHeight) : base()
        {
            AutoScaleMode = AutoScaleMode.None;
            MessageBoxButtons buttons = MessageBoxButtons.OKCancel;

            addWidth ??= 0;
            addHeight ??= 0;

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
            if (buttons == MessageBoxButtons.OK)
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
            mWidth += 20;

            ////// Box Height
            int minHeight = 130;
            int maxHeight = screen.Height - 50;
            Label testLabel = new();
            testLabel.AutoSize = true;
            testLabel.MaximumSize = new Size(mWidth - 2 - testLabelOffset, 0);
            testLabel.TextAlign = ContentAlignment.MiddleLeft;
            testLabel.Text = text;
            Size testLabelSize = testLabel.GetPreferredSize(Size.Empty);
            int iconHeight;
            if (icon == null || icon == MessageBoxIcon.None)
                iconHeight = 0;
            else
                iconHeight = 32;
            int mHeight;
            if (multiline)
                mHeight = testLabelSize.Height * 5 + 100 + iconHeight;
            else
                mHeight = testLabelSize.Height * 3 / 2 + 110;
            mHeight = Math.Max(minHeight, mHeight);
            if (mHeight > maxHeight)
            {
                mWidth += maxWidth;
                testLabel.MaximumSize = new Size(mWidth - 2 - testLabelOffset, 0);
                testLabelSize = testLabel.GetPreferredSize(Size.Empty);
                if (multiline)
                    mHeight = testLabelSize.Height * 5 + 100 + iconHeight;
                else
                    mHeight = testLabelSize.Height * 3 / 2 + 110;
                mHeight = Math.Max(minHeight, mHeight);
                if (mHeight > maxHeight)
                {
                    mWidth += maxWidth;
                    testLabel.MaximumSize = new Size(mWidth - 2 - testLabelOffset, 0);
                    testLabelSize = testLabel.GetPreferredSize(Size.Empty);
                    if (multiline)
                        mHeight = testLabelSize.Height * 5 + 100 + iconHeight;
                    else
                        mHeight = testLabelSize.Height * 3 / 2 + 110;
                    mHeight = Math.Max(minHeight, mHeight);
                    if (mHeight > maxHeight)
                    {
                        mWidth += maxWidth;
                        testLabel.MaximumSize = new Size(mWidth - 2 - testLabelOffset, 0);
                        testLabelSize = testLabel.GetPreferredSize(Size.Empty);
                        if (multiline)
                            mHeight = testLabelSize.Height * 5 + 100 + iconHeight;
                        else
                            mHeight = testLabelSize.Height * 3 / 2 + 110;
                        mHeight = Math.Max(minHeight, mHeight);
                        if (mHeight > maxHeight)
                        {
                            mWidth += maxWidth;
                            testLabel.MaximumSize = new Size(mWidth - 2 - testLabelOffset, 0);
                            testLabelSize = testLabel.GetPreferredSize(Size.Empty);
                            if (multiline)
                                mHeight = testLabelSize.Height * 5 + 100 + iconHeight;
                            else
                                mHeight = testLabelSize.Height * 3 / 2 + 110;
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

            mHeight += 20;
            Size = new(mWidth + (int)addWidth, mHeight + (int)addHeight);

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

            // Text Body Panel
            Panel textPanel = new();
            textPanel.BackColor = BackColor;
            textPanel.ForeColor = ForeColor;
            textPanel.BorderStyle = BorderStyle.None;
            textPanel.Margin = new Padding(0);
            textPanel.Location = new(rect.X, titlePanelHeight);
            if (multiline)
            {
                if (icon == null || icon == MessageBoxIcon.None)
                    textPanel.Size = new(rect.Width, (rect.Height - titlePanelHeight - buttonPanelHeight) / 5);
                else
                    textPanel.Size = new(rect.Width, (rect.Height - titlePanelHeight - buttonPanelHeight) / 4);
            }
            else
                textPanel.Size = new(rect.Width, (rect.Height - titlePanelHeight - buttonPanelHeight) * 3 / 5); //
            textPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(textPanel);

            // Input Panel
            Panel inputPanel = new();
            inputPanel.BackColor = BackColor;
            inputPanel.ForeColor = ForeColor;
            inputPanel.BorderStyle = BorderStyle.None;
            inputPanel.Margin = new Padding(0);
            inputPanel.Location = new(rect.X, titlePanelHeight + textPanel.Height);
            if (multiline)
            {
                if (icon == null || icon == MessageBoxIcon.None)
                    inputPanel.Size = new(rect.Width, (rect.Height - titlePanelHeight - buttonPanelHeight) * 4 / 5);
                else
                    inputPanel.Size = new(rect.Width, (rect.Height - titlePanelHeight - buttonPanelHeight) * 3 / 4);
            }
            else
                inputPanel.Size = new(rect.Width, (rect.Height - titlePanelHeight - buttonPanelHeight) * 2 / 5); //
            inputPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(inputPanel);

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

            // Text Body Label
            Label textLabel = new();
            textLabel.AutoSize = true;
            textLabel.MaximumSize = new Size(rect.Width - iconOffset, 0);
            textLabel.TextAlign = ContentAlignment.MiddleLeft;
            textLabel.Text = text;
            Size textLabelSize = textLabel.GetPreferredSize(Size.Empty);
            textLabel.Location = new(iconOffset, textPanel.Height / 2 - textLabelSize.Height / 2);
            textPanel.Controls.Add(textLabel);

            // Input Box
            inputTextBox = new();
            inputTextBox.Text = Input;
            if (multiline == false)
            {
                inputTextBox.Size = new Size(rect.Width - (5 * 2), inputTextBox.Height);
                inputTextBox.MaximumSize = new Size(rect.Width - (5 * 2), 0);
                Size inputLabelSize = inputTextBox.GetPreferredSize(Size.Empty);
                inputTextBox.Location = new(5, inputPanel.Height / 2 - inputLabelSize.Height / 2);
            }
            else
            {
                inputTextBox.Multiline = true;
                inputTextBox.ScrollBars = ScrollBars.Vertical;
                inputTextBox.Size = new Size(rect.Width - (5 * 2), inputPanel.Height);
                inputTextBox.MaximumSize = new Size(rect.Width - (5 * 2), inputPanel.Height);
                inputTextBox.Location = new(5, 0);
            }
            inputPanel.Controls.Add(inputTextBox);

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

                if (multiline == false)
                {
                    AcceptButton = btn2;
                    CancelButton = btn1;
                }
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

                if (multiline == false)
                {
                    AcceptButton = btn2;
                    CancelButton = btn1; 
                }
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

                if (multiline == false)
                {
                    AcceptButton = btn1; 
                }
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

                if (multiline == false)
                {
                    AcceptButton = btn1;
                    CancelButton = btn2;
                }
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

                if (multiline == false)
                {
                    AcceptButton = btn1;
                    CancelButton = btn2;
                }
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

                if (multiline == false)
                {
                    AcceptButton = btn1;
                    CancelButton = btn2;
                }
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

                if (multiline == false)
                {
                    AcceptButton = btn1;
                    CancelButton = btn3;
                }
            }

            // Set CustomTextBox and CustomButton Colors
            var cs = Controllers.GetAllControls(this);
            foreach (Control c in cs)
            {
                if (c is CustomTextBox customTextBox)
                {
                    if (BackColor.DarkOrLight() == "Dark")
                    {
                        _ = SetWindowTheme(customTextBox.Handle, "DarkMode_Explorer", null);
                        foreach (Control ctb in customTextBox.Controls)
                        {
                            _ = SetWindowTheme(ctb.Handle, "DarkMode_Explorer", null);
                        }
                    }
                    customTextBox.BackColor = BackColor;
                    customTextBox.ForeColor = ForeColor;
                    customTextBox.BorderColor = BorderColor;
                    customTextBox.Invalidate();
                }
                else if (c is CustomButton customButton)
                {
                    customButton.BackColor = BackColor;
                    customButton.ForeColor = ForeColor;
                    customButton.BorderColor = BorderColor;
                    customButton.SelectionColor = BorderColor;
                    customButton.RoundedCorners = ButtonRoundedCorners;
                    customButton.Invalidate();
                }
            }
        }

        /// <summary>Displays a prompt in a dialog box, waits for the user to input text or click a button.</summary>
        ///
        /// <param name="input">Update a String containing the contents of the text box.</param>
        /// <param name="text">The text to display in the input box.</param>
        /// <param name="multiline">TextBox multiline.</param>
        ///
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(Control owner, ref string input, string text, bool multiline)
        {
            // using construct ensures the resources are freed when form is closed.
            CustomInputBox form = new();
            form.SetInput(input);
            owner.InvokeIt(() =>
            {
                form = new(text, multiline, null, null, null, null);
                form.Font = owner.Font;
                // Fix Screen DPI
                ScreenDPI.FixDpiAfterInitializeComponent(form);
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(owner);
                form.Activate();
                owner.BringToFront();
            });
            input = inputTextBox.Text;
            form.FormClosing += (s, e) => form.Dispose();
            return dialogResult;
        }

        /// <summary>Displays a prompt in a dialog box, waits for the user to input text or click a button.</summary>
        ///
        /// <param name="input">Update a String containing the contents of the text box.</param>
        /// <param name="text">The text to display in the input box.</param>
        /// <param name="multiline">TextBox multiline.</param>
        /// <param name="addWidth">Add amount to width.</param>
        ///
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(Control owner, ref string input, string text, bool multiline, int addWidth)
        {
            // using construct ensures the resources are freed when form is closed.
            CustomInputBox form = new();
            form.SetInput(input);
            owner.InvokeIt(() =>
            {
                form = new(text, multiline, null, null, addWidth, null);
                form.Font = owner.Font;
                // Fix Screen DPI
                ScreenDPI.FixDpiAfterInitializeComponent(form);
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(owner);
                form.Activate();
                owner.BringToFront();
            });
            input = inputTextBox.Text;
            form.FormClosing += (s, e) => form.Dispose();
            return dialogResult;
        }

        /// <summary>Displays a prompt in a dialog box, waits for the user to input text or click a button.</summary>
        ///
        /// <param name="input">Update a String containing the contents of the text box.</param>
        /// <param name="text">The text to display in the input box.</param>
        /// <param name="multiline">TextBox multiline.</param>
        /// <param name="addWidth">Add amount to width.</param>
        /// <param name="addHeight">Add amount to height.</param>
        ///
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(Control owner, ref string input, string text, bool multiline, int addWidth, int addHeight)
        {
            // using construct ensures the resources are freed when form is closed.
            CustomInputBox form = new();
            form.SetInput(input);
            owner.InvokeIt(() =>
            {
                form = new(text, multiline, null, null, addWidth, addHeight);
                form.Font = owner.Font;
                // Fix Screen DPI
                ScreenDPI.FixDpiAfterInitializeComponent(form);
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(owner);
                form.Activate();
                owner.BringToFront();
            });
            input = inputTextBox.Text;
            form.FormClosing += (s, e) => form.Dispose();
            return dialogResult;
        }

        /// <summary>Displays a prompt in a dialog box, waits for the user to input text or click a button.</summary>
        ///
        /// <param name="input">Update a String containing the contents of the text box.</param>
        /// <param name="text">The text to display in the input box.</param>
        /// <param name="multiline">TextBox multiline.</param>
        /// <param name="caption">The text to display in the title bar of the input box.</param>
        ///
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(Control owner, ref string input, string text, bool multiline, string caption)
        {
            // using construct ensures the resources are freed when form is closed.
            CustomInputBox form = new();
            form.SetInput(input);
            owner.InvokeIt(() =>
            {
                CustomInputBox form = new(text, multiline, caption, null, null, null);
                form.Font = owner.Font;
                // Fix Screen DPI
                ScreenDPI.FixDpiAfterInitializeComponent(form);
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(owner);
                form.Activate();
                owner.BringToFront();
            });
            input = inputTextBox.Text;
            form.FormClosing += (s, e) => form.Dispose();
            return dialogResult;
        }

        /// <summary>Displays a prompt in a dialog box, waits for the user to input text or click a button.</summary>
        ///
        /// <param name="input">Update a String containing the contents of the text box.</param>
        /// <param name="text">The text to display in the input box.</param>
        /// <param name="multiline">TextBox multiline.</param>
        /// <param name="caption">The text to display in the title bar of the input box.</param>
        /// <param name="addWidth">Add amount to width.</param>
        ///
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(Control owner, ref string input, string text, bool multiline, string caption, int addWidth)
        {
            // using construct ensures the resources are freed when form is closed.
            CustomInputBox form = new();
            form.SetInput(input);
            owner.InvokeIt(() =>
            {
                form = new(text, multiline, caption, null, addWidth, null);
                form.Font = owner.Font;
                // Fix Screen DPI
                ScreenDPI.FixDpiAfterInitializeComponent(form);
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(owner);
                form.Activate();
                owner.BringToFront();
            });
            input = inputTextBox.Text;
            form.FormClosing += (s, e) => form.Dispose();
            return dialogResult;
        }

        /// <summary>Displays a prompt in a dialog box, waits for the user to input text or click a button.</summary>
        ///
        /// <param name="input">Update a String containing the contents of the text box.</param>
        /// <param name="text">The text to display in the input box.</param>
        /// <param name="multiline">TextBox multiline.</param>
        /// <param name="caption">The text to display in the title bar of the input box.</param>
        /// <param name="addWidth">Add amount to width.</param>
        /// <param name="addHeight">Add amount to height.</param>
        ///
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(Control owner, ref string input, string text, bool multiline, string caption, int addWidth, int addHeight)
        {
            // using construct ensures the resources are freed when form is closed.
            CustomInputBox form = new();
            form.SetInput(input);
            owner.InvokeIt(() =>
            {
                form = new(text, multiline, caption, null, addWidth, addHeight);
                form.Font = owner.Font;
                // Fix Screen DPI
                ScreenDPI.FixDpiAfterInitializeComponent(form);
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(owner);
                form.Activate();
                owner.BringToFront();
            });
            input = inputTextBox.Text;
            form.FormClosing += (s, e) => form.Dispose();
            return dialogResult;
        }

        /// <summary>Displays a prompt in a dialog box, waits for the user to input text or click a button.</summary>
        ///
        /// <param name="input">Update a String containing the contents of the text box.</param>
        /// <param name="text">The text to display in the input box.</param>
        /// <param name="multiline">TextBox multiline.</param>
        /// <param name="caption">The text to display in the title bar of the input box.</param>
        /// <param name="icon">One of the System.Windows.Forms.MessageBoxIcon values that specifies which icon to display in the input box.</param>
        ///
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(Control owner, ref string input, string text, bool multiline, string caption, MessageBoxIcon icon)
        {
            // using construct ensures the resources are freed when form is closed.
            CustomInputBox form = new();
            form.SetInput(input);
            owner.InvokeIt(() =>
            {
                form = new(text, multiline, caption, icon, null, null);
                form.Font = owner.Font;
                // Fix Screen DPI
                ScreenDPI.FixDpiAfterInitializeComponent(form);
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(owner);
                form.Activate();
                owner.BringToFront();
            });
            input = inputTextBox.Text;
            form.FormClosing += (s, e) => form.Dispose();
            return dialogResult;
        }

        /// <summary>Displays a prompt in a dialog box, waits for the user to input text or click a button.</summary>
        ///
        /// <param name="input">Update a String containing the contents of the text box.</param>
        /// <param name="text">The text to display in the input box.</param>
        /// <param name="multiline">TextBox multiline.</param>
        /// <param name="caption">The text to display in the title bar of the input box.</param>
        /// <param name="icon">One of the System.Windows.Forms.MessageBoxIcon values that specifies which icon to display in the input box.</param>
        /// <param name="addWidth">Add amount to width.</param>
        ///
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(Control owner, ref string input, string text, bool multiline, string caption, MessageBoxIcon icon, int addWidth)
        {
            // using construct ensures the resources are freed when form is closed.
            CustomInputBox form = new();
            form.SetInput(input);
            owner.InvokeIt(() =>
            {
                form = new(text, multiline, caption, icon, addWidth, null);
                form.Font = owner.Font;
                // Fix Screen DPI
                ScreenDPI.FixDpiAfterInitializeComponent(form);
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(owner);
                form.Activate();
                owner.BringToFront();
            });
            input = inputTextBox.Text;
            form.FormClosing += (s, e) => form.Dispose();
            return dialogResult;
        }

        /// <summary>Displays a prompt in a dialog box, waits for the user to input text or click a button.</summary>
        ///
        /// <param name="input">Update a String containing the contents of the text box.</param>
        /// <param name="text">The text to display in the input box.</param>
        /// <param name="multiline">TextBox multiline.</param>
        /// <param name="caption">The text to display in the title bar of the input box.</param>
        /// <param name="icon">One of the System.Windows.Forms.MessageBoxIcon values that specifies which icon to display in the input box.</param>
        /// <param name="addWidth">Add amount to width.</param>
        /// <param name="addHeight">Add amount to height.</param>
        ///
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        public static DialogResult Show(Control owner, ref string input, string text, bool multiline, string caption, MessageBoxIcon icon, int addWidth, int addHeight)
        {
            // using construct ensures the resources are freed when form is closed.
            CustomInputBox form = new();
            form.SetInput(input);
            owner.InvokeIt(() =>
            {
                form = new(text, multiline, caption, icon, addWidth, addHeight);
                form.Font = owner.Font;
                // Fix Screen DPI
                ScreenDPI.FixDpiAfterInitializeComponent(form);
                form.StartPosition = FormStartPosition.CenterParent;
                form.ShowDialog(owner);
                form.Activate();
                owner.BringToFront();
            });
            input = inputTextBox.Text;
            form.FormClosing += (s, e) => form.Dispose();
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
