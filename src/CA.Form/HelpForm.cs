using System.Windows.Forms;

namespace CA.Form
{
    public partial class HelpForm : System.Windows.Forms.Form
    {
        public HelpForm()
        {
            InitializeComponent();

            KeyPreview = true;
            KeyDown += OnKeyDown;


            textBox.Text = @"Consistent Cellular Automata.

Usage:
    Arrow keys: Move the view port around.

    F1 - This help
    
    G - Draw grid lines (y/n)
    V - Draw view port (y/n)
    N - Draw edge chunks as noise/evolved
    F - Draw inside chunks as noise/final
    E - Draw edge chunks (y/n)

    Space - Colorize inside chunks (y/n)

    R - Reset World
    S - Save image file to folder

    ESC - Close the application
";

            textBox.Select(0, 0);
        }

        private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            keyEventArgs.SuppressKeyPress = true;
            keyEventArgs.Handled = true;
            switch (keyEventArgs.KeyCode)
            {
                case Keys.F1:
                case Keys.Escape:
                    Close();
                    break;

                default:
                    keyEventArgs.SuppressKeyPress = false;
                    keyEventArgs.Handled = false;
                    break;
            }
        }
    }
}
