using NSL.Refactoring.FastAction.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSL.Refactoring.FastAction.UI
{

    internal class Win32WindowWrapper : IWin32Window
    {
        private readonly IntPtr _handle;
        public Win32WindowWrapper(IntPtr handle) { _handle = handle; }
        public IntPtr Handle => _handle;
    }
    public class RefactorUI
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        public static Dictionary<string, string> PromptUserForInputValues(TemplateInputFieldData[] fields)
        {
            var hwnd = GetForegroundWindow();
            var owner = new Win32WindowWrapper(hwnd);

            using (var form = new DynamicInputForm(fields))
            {
                form.ShowInTaskbar = false;
                form.TopMost = true;
                form.StartPosition = FormStartPosition.CenterParent;
                var result = form.ShowDialog(owner);
                return form.Result;
            }
        }
    }

    internal class DynamicInputForm : Form
    {
        private readonly Dictionary<string, Control> _inputs = new Dictionary<string, Control>();

        public Dictionary<string, string> Result { get; private set; }

        public DynamicInputForm(TemplateInputFieldData[] fields)
        {
            this.Text = "Enter values";
            this.Height = 60 + fields.Length * 35 + 80;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;

            var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            this.Controls.Add(panel);

            int left = 0;
            int top = 10;
            foreach (var field in fields)
            {
                if (top == 605)
                {
                    top = 10;
                    left += 350;

                }

                var label = new Label
                {
                    Text = field.DisplayName ?? field.Name,
                    Left = left + 10,
                    Top = top + 4,
                    Width = 150
                };
                panel.Controls.Add(label);

                Control inputControl = CreateInputControl(field, top, left);
                panel.Controls.Add(inputControl);

                _inputs[field.Name] = inputControl;

                top += 35;
            }

            if (left > 0)
            {
                top = 605;
            }

            this.Width = left + 400;

            var okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Left = 10,
                Width = 80,
                Top = top + 10
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Left = 100,
                Width = 80,
                Top = top + 10
            };

            panel.Controls.Add(okButton);
            panel.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }

        private Control CreateInputControl(TemplateInputFieldData field, int top, int left)
        {
            switch (field.Type?.ToLowerInvariant())
            {
                case "number":
                    return new NumericUpDown
                    {
                        Left = left + 170,
                        Top = top,
                        Width = 180,
                        Minimum = long.MinValue,
                        Maximum = long.MaxValue,
                        Value = long.TryParse(field.DefaultValue, out var lval) ? lval : 0
                    };

                case "floatnumber":
                    return new TextBox
                    {
                        Left = left + 170,
                        Top = top,
                        Width = 180,
                        Text = field.DefaultValue ?? "0.0"
                    };

                case "text":
                default:
                    return new TextBox
                    {
                        Left = left + 170,
                        Top = top,
                        Width = 180,
                        Text = field.DefaultValue ?? ""
                    };
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (this.DialogResult == DialogResult.OK)
            {
                Result = new Dictionary<string, string>();

                foreach (var kv in _inputs)
                {
                    string value = default;

                    if (kv.Value is TextBox tb)
                    {
                        value = tb.Text; if (tb.Text != null && double.TryParse(tb.Text, out var d))
                        {
                            value = d.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }
                    else if (kv.Value is NumericUpDown nud) value = nud.Value.ToString();


                    value = value ?? string.Empty;

                    foreach (var k in kv.Key.Split('/'))
                    {
                        Result[k] = value;
                    }
                }
            }
            else
            {
                Result = null;
            }
        }
    }

}
